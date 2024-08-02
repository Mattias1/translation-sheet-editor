using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using AvaloniaExtensions;
using TranslationSheetEditor.Model;
using TranslationSheetEditor.Utils;
using Control = Avalonia.Controls.Control;
using Settings = TranslationSheetEditor.Model.Settings;

namespace TranslationSheetEditor.UI;

public sealed class InitialComponent : CanvasComponentBase {
  private Settings? _settings;
  private Settings Settings => _settings ??= SettingsFiles.Get.GetSettings<Settings>();

  private TextBlock _tblLast = null!;
  private Button? _btnLastAddedLanguage;
  private TextBox _tbNewLanguage = null!;
  private TextBlock _tblImportValidation = null!;

  protected override void InitializeControls() {
    Settings.FirstTimeInit();

    AddTextBlockHeader("Bible-link translation sheet editor").TopLeftInPanel();
    AddTextBlock("This application will generate the bible link translation sheets for you.").Below();
    _tblLast = AddTextBlock("For which language do you want to enter a translation?").Below();

    foreach (var language in Settings.Languages) {
      AddLanguageButton(language);
    }

    _tbNewLanguage = AddTextBox().Width(400).MaxWidth(400).BottomLeftInPanel().WithInitialFocus();
    AddButton("Add", OnAddLanguageClick).RightOf();
    AddButton("Refresh", OnRefreshLanguagesClick).RightOf();
    AddLabelAbove("Add a new language:", _tbNewLanguage);

    AddButton("Import from Excel", OnImportFromExcelClick).BottomRightInPanel();
    _tblImportValidation = AddTextBlock("").Width(320).TopRightInPanel();

    Settings.SelectedLanguage = null;
  }

  private void OnAddLanguageClick(RoutedEventArgs _) {
    string? language = _tbNewLanguage.Text;
    if (string.IsNullOrWhiteSpace(language)) {
      _tbNewLanguage.Focus();
      return;
    }

    AddLanguage(language);
    _tbNewLanguage.Text = "";
    _tbNewLanguage.Focus();
  }

  private void OnRefreshLanguagesClick(RoutedEventArgs _) {
    string dataPrefix = "translation-sheet-editor-data-";
    var allDataJsonFiles = Directory.GetFiles(AssetExtensions.StartupPath, $"{dataPrefix}*.json");
    var allLanguages = allDataJsonFiles
        .Select(s => Path.GetFileNameWithoutExtension(s))
        .Select(s => s.Substring(dataPrefix.Length, s.Length - dataPrefix.Length));
    var newLanguages = allLanguages.Except(Settings.Languages).ToList();

    Settings.Languages = Settings.Languages.Intersect(allLanguages).ToList();

    foreach (var language in newLanguages) {
      AddLanguage(language);
    }
  }

  private void AddLanguage(string language) {
    if (Settings.Languages is null) {
      throw new InvalidOperationException("Settings.Languages should've been initialised by now.");
    }
    Settings.Languages.Add(language);
    SettingsFiles.Get.SaveSettings();

    AddLanguageButton(language);
    RepositionControls();
  }

  private void AddLanguageButton(string language) {
    var previousControl = (Control?)_btnLastAddedLanguage ?? _tblLast;
    _btnLastAddedLanguage = AddButton(language, e => OnNextClick(language, e))
        .YBelow(previousControl).XCenterInPanel();
  }

  private async void OnImportFromExcelClick(RoutedEventArgs _) { // async void; it's fine for UI events, but nowhere else ;)
    _tblImportValidation.Text = "";

    string? errors, importedLanguage = null;
    try {
      var storageFile = await GetPathViaFileDialogAsync().ConfigureAwait(true);
      if (storageFile is null) {
        return;
      }

      (errors, importedLanguage) = ImportDataFromExcel(storageFile);
    } catch (Exception e) {
      errors = $"An error occured when trying to import the file.\n\nError message: '{e.Message}'";
    }

    if (string.IsNullOrWhiteSpace(errors)) {
      _tblImportValidation.Text = "Import: Ok";
      _tblImportValidation.Foreground = Brushes.Green;
      if (!string.IsNullOrWhiteSpace(importedLanguage)) {
        OnNextClick(importedLanguage, _); // Lol, this works
      }
    } else {
      _tblImportValidation.Text = "Import: " + errors;
      _tblImportValidation.Foreground = Brushes.Red;
    }
  }

  private async Task<IStorageFile?> GetPathViaFileDialogAsync() {
    var storageProvider = FindWindow().StorageProvider;

    var fileTypeChoice = new FilePickerFileType("Excel files") { Patterns = new string[] { "*.xlsx" } };
    var options = new FilePickerOpenOptions() {
        AllowMultiple = false,
        FileTypeFilter = new[] { fileTypeChoice }
    };

    var files = await storageProvider.OpenFilePickerAsync(options).ConfigureAwait(true);
    return files.FirstOrDefault();
  }

  private (string? errors, string? language) ImportDataFromExcel(IStorageFile storageFile) {
    var importedTranslationData = ExcelUtil.Import(storageFile.Path, out string? errors);
    string? language = importedTranslationData.Language;
    if (!string.IsNullOrWhiteSpace(language)) {
      SettingsFiles.Get.AddSettingsFileIfNotExists<TranslationData>($"translation-sheet-editor-data-{language}.json");
      SettingsFiles.Get.OverwriteSettings(importedTranslationData);
      if (!Settings.Languages.Contains(language)) {
        AddLanguageButton(language);
        RepositionControls();

        Settings.Languages.Add(language);
      }
      SettingsFiles.Get.SaveSettings();
    }
    return (errors, language);
  }

  private void OnNextClick(string language, RoutedEventArgs _) {
    Settings.SelectedLanguage = language;
    SettingsFiles.Get.AddSettingsFileIfNotExists<TranslationData>($"translation-sheet-editor-data-{language}.json");
    SwitchToComponent<BookListComponent>();
  }
}
