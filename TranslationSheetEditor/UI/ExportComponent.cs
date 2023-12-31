﻿using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
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
    ValidateEmptyFields();
  }

  private void ValidateEmptyFields() {
    _tblValidation.Text = "Validation:";

    if (Data.BibleBooks.BibleBookData.Any(kv => string.IsNullOrWhiteSpace(kv.Value.TranslatedName))) {
      _tblValidation.Text += "\n- Missing values at 'Initial book names'.";
    }

    if (string.IsNullOrWhiteSpace(Data.LoadingStatus)
        || string.IsNullOrWhiteSpace(Data.NoResultStatus)
        || string.IsNullOrWhiteSpace(Data.ErrorCodeStatus)
        || string.IsNullOrWhiteSpace(Data.ReadMoreStatus)
        || Data.WordsForVerse.All(string.IsNullOrWhiteSpace)
        || Data.VerseSelectionWords.All(string.IsNullOrWhiteSpace)
        || Data.ChapterVerseSeparator.All(string.IsNullOrWhiteSpace)
        || Data.VerseVerseSeparator.All(string.IsNullOrWhiteSpace)) {
      _tblValidation.Text += "\n- Missing values at 'Other translations'.";
    }

    if (Data.PrefixNumberOptionsForFirst.All(string.IsNullOrWhiteSpace)
        || Data.PrefixNumberOptionsForSecond.All(string.IsNullOrWhiteSpace)
        || Data.PrefixNumberOptionsForThird.All(string.IsNullOrWhiteSpace)) {
      _tblValidation.Text += "\n- Missing values at 'Prefix numbers'.";
    }

    var regexPartSet = new HashSet<string>();
    var regexPartSetButNotJohn = new HashSet<string>(); // Gotta love edge cases :)
    foreach(var (name, bookData) in Data.BibleBooks.BibleBookData) {
      if (bookData.RegexParts.All(string.IsNullOrWhiteSpace)) {
        _tblValidation.Text += $"\n- Missing values for book '{name}'.";
      }
      foreach (string regexPart in bookData.RegexParts) {
        string errorMessage = $"\n- Book alternative '{regexPart}' is used multiple times.";
        if (!regexPartSet.Add(regexPart)
            && (name != BibleBooks.JOHN_LETTER || regexPartSetButNotJohn.Contains(regexPart))) {
          _tblValidation.Text += errorMessage;
        }
        if (name != BibleBooks.JOHN) {
          regexPartSetButNotJohn.Add(regexPart);
        }
      }
    }

    if (_tblValidation.Text == "Validation:") {
      _tblValidation.Text += " ok.";
      _tblValidation.Foreground = Brushes.Green;
      _btnExport.IsEnabled = true;
    } else {
      _tblValidation.Foreground = Brushes.Red;
      _btnExport.IsEnabled = false;
    }
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
