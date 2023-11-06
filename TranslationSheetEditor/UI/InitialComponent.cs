using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
using Avalonia.Platform.Storage;
using AvaloniaExtensions;
using TranslationSheetEditor.Model;
using TranslationSheetEditor.Utils;

namespace TranslationSheetEditor.UI;

public sealed class InitialComponent : CanvasComponentBase {
  private Settings? _settings;
  private Settings Settings => _settings ??= SettingsFiles.Get.GetSettings<Settings>();

  private TextBlock _lastTextBlock = null!;
  private Button? _lastAddedLanguageButton;
  private TextBox _newLanguageTextBox = null!;

  protected override void InitializeControls() {
    Settings.FirstTimeInit();

    AddTextBlockHeader("Bible-link translation sheet editor").TopLeftInPanel();
    AddTextBlock("This application will generate the bible link translation sheets for you.").Below();
    _lastTextBlock = AddTextBlock("For which language do you want to enter a translation?").Below();

    foreach (var language in Settings.Languages) {
      AddLanguageButton(language);
    }

    _newLanguageTextBox = AddTextBox().Width(400).MaxWidth(400).BottomLeftInPanel().WithInitialFocus();
    AddButton("Add", OnAddLanguageClick).RightOf();
    AddLabelAbove("Add a new language:", _newLanguageTextBox);

    AddButton("Import from Excel", OnImportFromExcelClick).BottomRightInPanel();

    Settings.SelectedLanguage = null;
  }

  private void OnAddLanguageClick(RoutedEventArgs _) {
    string? language = _newLanguageTextBox.Text;
    if (string.IsNullOrWhiteSpace(language)) {
      _newLanguageTextBox.Focus();
      return;
    }

    if (Settings.Languages is null) {
      throw new InvalidOperationException("Settings.Languages should've been initialised by now.");
    }
    Settings.Languages.Add(language);
    SettingsFiles.Get.SaveSettings();

    AddLanguageButton(language);
    _newLanguageTextBox.Text = "";
    _newLanguageTextBox.Focus();
    RepositionControls();
  }

  private void AddLanguageButton(string language) {
    var previousControl = (Control?)_lastAddedLanguageButton ?? _lastTextBlock;
    _lastAddedLanguageButton = AddButton(language, e => OnNextClick(language, e))
        .YBelow(previousControl).XCenterInPanel();
  }

  private async void OnImportFromExcelClick(RoutedEventArgs _) {
    var storageFile = await GetPathViaFileDialogAsync().ConfigureAwait(true);
    if (storageFile is null) {
      return;
    }

    var importedTranslationData = ExcelUtil.Import(storageFile.Path);
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

  private void OnNextClick(string language, RoutedEventArgs _) {
    Settings.SelectedLanguage = language;
    SettingsFiles.Get.AddSettingsFileIfNotExists<TranslationData>($"translation-sheet-editor-data-{language}.json");
    SwitchToComponent<BookListComponent>();
  }
}
