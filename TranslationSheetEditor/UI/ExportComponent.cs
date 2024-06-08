using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using AvaloniaExtensions;
using TranslationSheetEditor.Model;
using TranslationSheetEditor.Utils;

namespace TranslationSheetEditor.UI;

public sealed class ExportComponent : CanvasComponentBase {
  private Settings? _settings;
  private Settings Settings => _settings ??= GetSettings<Settings>();

  private TranslationData? _data;
  private TranslationData Data => _data ??= GetSettings<TranslationData>();

  private Button _btnExport = null!;
  private TextBlock _tblValidation = null!;

  protected override void InitializeControls() {
    AddTextBlockHeader("Export translation data").TopLeftInPanel();
    var description = AddTextBlock("You can export the translation sheet as an excel sheet.").Below();
    _btnExport = AddButton("Export to Excel", OnExportClick).XCenterInPanel().YBelow(description);
    _tblValidation = AddTextBlock("...").YBelow(_btnExport).XLeftInPanel();

    NavigationControls.Add(this, () => { });
  }

  protected override void OnLoaded(RoutedEventArgs e) {
    base.OnLoaded(e);

    string errorText = ValidationUtil.ValidateTranslationData(Data);

    if (string.IsNullOrWhiteSpace(errorText)) {
      SetValidationContent("Validation: Ok.", Brushes.Green);
      _btnExport.IsEnabled = true;
    } else {
      SetValidationContent($"Validation: {errorText}", Brushes.Red);
      _btnExport.IsEnabled = false;
    }
  }

  private async void OnExportClick(RoutedEventArgs _) { // async void; it's fine for UI events, but nowhere else ;)
    SetValidationContent("Export: ...", Brushes.Green);
    try {
      var storageFile = await GetPathViaSaveDialogAsync().ConfigureAwait(true);
      if (storageFile is null) {
        return;
      }

      ExcelUtil.Export(Data, storageFile.Path);
      SetValidationContent("Export: Ok.", Brushes.Green);
    } catch (Exception e) {
      string errorText = $"Export: An error occured when trying to export the file.\n\nError message: '{e.Message}'";
      SetValidationContent(errorText, Brushes.Red);
    }
  }

  private async Task<IStorageFile?> GetPathViaSaveDialogAsync() {
    var storageProvider = FindWindow().StorageProvider;

    var fileTypeChoice = new FilePickerFileType("Excel files") { Patterns = new string[] { "*.xlsx" } };
    var options = new FilePickerSaveOptions {
        FileTypeChoices = new[] { fileTypeChoice },
        DefaultExtension = "xlsx",
        SuggestedFileName = $"bible-link-translation-sheet-{Settings.SelectedLanguage}-generated.xlsx",
        ShowOverwritePrompt = true
    };

    return await storageProvider.SaveFilePickerAsync(options).ConfigureAwait(true);
  }

  private void SetValidationContent(string text, IBrush brush) {
      _tblValidation.Text = text;
      _tblValidation.Foreground = brush;
  }
}
