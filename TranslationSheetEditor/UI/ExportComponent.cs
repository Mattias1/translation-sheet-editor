﻿using Avalonia.Interactivity;
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

  protected override void InitializeControls() {
    AddTextBlockHeader("Export translation data").TopLeftInPanel();
    var description = AddTextBlock("You can export the translation sheet as an excel sheet.").Below();
    // TODO: A check (green / red label?) if everything has a value
    AddButton("Export to Excel", OnExportClick).XCenterInPanel().YBelow(description);

    NavigationControls.Add(this, () => { });
  }

  private async void OnExportClick(RoutedEventArgs e) { // async void; it's fine for UI events, but nowhere else ;)
    var storageFile = await GetPathViaSaveDialogAsync().ConfigureAwait(true);
    if (storageFile is null) {
      return;
    }

    ExcelUtil.Export(Data, storageFile.Path);
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
}
