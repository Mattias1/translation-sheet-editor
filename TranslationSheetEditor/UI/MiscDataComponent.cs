using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using AvaloniaExtensions;
using TranslationSheetEditor.Model;

namespace TranslationSheetEditor.UI;

public sealed class MiscDataComponent : CanvasComponentBase {
  private const int LABEL_WIDTH = 250;
  private const int SEPARATOR_WIDTH = 100;

  private TranslationData? _data;
  private TranslationData Data => _data ??= SettingsFiles.Get.GetSettings<TranslationData>();

  private TextBox _tbLoadingStatus = null!;
  private TextBox _tbNoResultStatus = null!;
  private TextBox _tbErrorCodeStatus = null!;
  private TextBox _tbReadMoreStatus = null!;

  private ExpandingTextBoxes _tbWordsForVerse = null!;
  private ExpandingTextBoxes _tbVerseSelectionWords = null!;
  private ExpandingTextBoxes _tbChapterVerseSeparator = null!;
  private ExpandingTextBoxes _tbVerseVerseSeparator = null!;

  protected override void InitializeControls() {
    AddTextBlockHeader("Other translations").TopLeftInPanel();

    _tbLoadingStatus = AddTextBox().Below().WithInitialFocus();
    _tbNoResultStatus = AddTextBox().Below();
    _tbErrorCodeStatus = AddTextBox().Below();
    _tbReadMoreStatus = AddTextBox().Below();

    AddSeparator().Below().StretchRightInPanel();

    _tbWordsForVerse = ExpandingTextBoxes.Add(this, tb => tb.Below(), "Words for 'Verse'", "(e.g. 'verse',\n 'vs', 'v.')");
    _tbVerseSelectionWords = ExpandingTextBoxes.Add(this, tb => tb.RightOf(_tbWordsForVerse.FirstTextBox),
        "Verse selection words", "(e.g. 'to', 'till', 'until')"); // TODO: 'and' ?
    _tbChapterVerseSeparator = ExpandingTextBoxes.Add(this, tb => tb.RightOf(_tbVerseSelectionWords.FirstTextBox),
        "Chapter-verse separators", "(e.g. ':')", SEPARATOR_WIDTH); // TODO: ',' ?
    _tbVerseVerseSeparator = ExpandingTextBoxes.Add(this, tb => tb.RightOf(_tbChapterVerseSeparator.FirstTextBox),
        "Verse-verse separators", "(e.g. '-')", SEPARATOR_WIDTH); // TODO: Add alternatives for dashes (like long dashes and other unicode alternatives)

    InsertLabelLeftOf("'Loading...'", _tbLoadingStatus, LABEL_WIDTH);
    InsertLabelLeftOf("'Result'", _tbNoResultStatus, LABEL_WIDTH);
    InsertLabelLeftOf("'Error code'", _tbErrorCodeStatus, LABEL_WIDTH);
    InsertLabelLeftOf("'Read more' (or 'Read further')", _tbReadMoreStatus, LABEL_WIDTH);

    AddTextBlock($"(language: {Data.Language})").FontWeight(FontWeight.Bold).FontStyle(FontStyle.Italic)
        .Above(_tbLoadingStatus);

    NavigationControls.Add(this, SaveData);

    LoadData();
  }

  private void LoadData() {
    _tbLoadingStatus.Text = Data.LoadingStatus;
    _tbNoResultStatus.Text = Data.NoResultStatus;
    _tbErrorCodeStatus.Text = Data.ErrorCodeStatus;
    _tbReadMoreStatus.Text = Data.ReadMoreStatus;

    _tbWordsForVerse.Data = Data.WordsForVerse;
    _tbVerseSelectionWords.Data = Data.VerseSelectionWords;
    _tbChapterVerseSeparator.Data = Data.ChapterVerseSeparator;
    _tbVerseVerseSeparator.Data = Data.VerseVerseSeparator;
  }

  private void SaveData() {
    Data.LoadingStatus = _tbLoadingStatus.Text ?? "";
    Data.NoResultStatus = _tbNoResultStatus.Text ?? "";
    Data.ErrorCodeStatus = _tbErrorCodeStatus.Text ?? "";
    Data.ReadMoreStatus = _tbReadMoreStatus.Text ?? "";

    Data.WordsForVerse = _tbWordsForVerse.Data;
    Data.VerseSelectionWords = _tbVerseSelectionWords.Data;
    Data.ChapterVerseSeparator = _tbChapterVerseSeparator.Data;
    Data.VerseVerseSeparator = _tbVerseVerseSeparator.Data;

    SettingsFiles.Get.SaveSettings();
  }
}
