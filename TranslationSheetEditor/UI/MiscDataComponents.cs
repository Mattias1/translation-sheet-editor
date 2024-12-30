using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using AvaloniaExtensions;
using TranslationSheetEditor.Model;

namespace TranslationSheetEditor.UI;

public class MiscDataComponent : CanvasComponentBase {
  private const int LABEL_WIDTH = 250;
  protected const int TEXTBOX_WIDTH = 160;
  private const int TEXTBOX_WIDTH_LARGE = 220;
  private const int SEPARATOR_WIDTH = 100;

  private TranslationData? _data;
  private TranslationData Data => _data ??= SettingsFiles.Get.GetSettings<TranslationData>();

  private TextBox _tbLoadingStatus = null!;
  private TextBox _tbNoResultStatus = null!;
  private TextBox _tbErrorCodeStatus = null!;
  private TextBox _tbReadMoreStatus = null!;
  private TextBox _tbNotFountStatus = null!;

  private ExpandingTextBoxes? _tbWordsForChapter;
  private ExpandingTextBoxes? _tbWordsForVerse;
  private ExpandingTextBoxes? _tbChapterVerseSeparator;
  private ExpandingTextBoxes? _tbVerseVerseSeparator;
  protected ExpandingTextBoxes? TbVerseSelectionWords;
  protected ExpandingTextBoxes? TbWordsOrCharactersForListingReferences;

  protected override void InitializeControls() {
    PositionTopPart();

    _tbWordsForChapter = ExpandingTextBoxes.Add(this, tb => tb.Below(), "Words for 'Chapter'",
        "(e.g. 'chapter',\n 'chap.', 'ch.')", TEXTBOX_WIDTH);
    _tbWordsForVerse = ExpandingTextBoxes.Add(this, tb => tb.RightOf(_tbWordsForChapter.FirstTextBox), "Words for 'Verse'",
        "(e.g. 'verse',\n 'vs', 'v.')", TEXTBOX_WIDTH);
    _tbChapterVerseSeparator = ExpandingTextBoxes.Add(this, tb => tb.RightOf(_tbWordsForVerse.FirstTextBox),
        "Chapter-verse separators", "(e.g. ':')", SEPARATOR_WIDTH);
    _tbVerseVerseSeparator = ExpandingTextBoxes.Add(this, tb => tb.RightOf(_tbChapterVerseSeparator.FirstTextBox),
        "Verse-verse separators", "(e.g. '-')", SEPARATOR_WIDTH);

    NavigationControls.Add(this, SaveData);
  }

  protected void PositionTopPart() {
    AddTextBlockHeader("Other translations").TopLeftInPanel();

    _tbLoadingStatus = AddTextBox().Width(TEXTBOX_WIDTH_LARGE).Below().WithInitialFocus();
    _tbNoResultStatus = AddTextBox().Width(TEXTBOX_WIDTH_LARGE).Below();
    _tbErrorCodeStatus = AddTextBox().Width(TEXTBOX_WIDTH_LARGE).Below();
    InsertLabelLeftOf("'Loading...'", _tbLoadingStatus, LABEL_WIDTH);
    InsertLabelLeftOf("'No result'", _tbNoResultStatus, LABEL_WIDTH);
    InsertLabelLeftOf("'Error code'", _tbErrorCodeStatus, LABEL_WIDTH);

    _tbReadMoreStatus = AddTextBox().Width(TEXTBOX_WIDTH_LARGE).RightOf(_tbLoadingStatus);
    _tbNotFountStatus = AddTextBox().Width(TEXTBOX_WIDTH_LARGE).Below();
    InsertLabelLeftOf("'Read more' (or 'Read further')", _tbReadMoreStatus, LABEL_WIDTH);
    InsertLabelLeftOf("'Not found'", _tbNotFountStatus, LABEL_WIDTH);

    AddTextBlock($"(language: {Data.Language})").FontWeight(FontWeight.Bold).FontStyle(FontStyle.Italic)
        .Above(_tbLoadingStatus);

    AddSeparator().Below(_tbErrorCodeStatus).XLeftInPanel().StretchRightInPanel();
  }

  protected override void OnSwitchingToComponent() {
    _data = null;
    LoadData();
  }

  protected void LoadData() {
    _tbLoadingStatus.Text = Data.LoadingStatus;
    _tbNoResultStatus.Text = Data.NoResultStatus;
    _tbErrorCodeStatus.Text = Data.ErrorCodeStatus;
    _tbReadMoreStatus.Text = Data.ReadMoreStatus;
    _tbNotFountStatus.Text = Data.NotFoundStatus;

    if (_tbWordsForChapter is not null && _tbWordsForVerse is not null && _tbChapterVerseSeparator is not null &&
        _tbVerseVerseSeparator is not null) {
      _tbWordsForChapter.Data = Data.WordsForChapter;
      _tbWordsForVerse.Data = Data.WordsForVerse;
      _tbChapterVerseSeparator.Data = Data.ChapterVerseSeparator;
      _tbVerseVerseSeparator.Data = Data.VerseVerseSeparator;
    }
    if (TbVerseSelectionWords is not null && TbWordsOrCharactersForListingReferences is not null) {
      TbVerseSelectionWords.Data = Data.VerseSelectionWords;
      TbWordsOrCharactersForListingReferences.Data = Data.WordsOrCharactersForListingReferences;
    }
  }

  protected void SaveData() {
    Data.LoadingStatus = _tbLoadingStatus.Text ?? "";
    Data.NoResultStatus = _tbNoResultStatus.Text ?? "";
    Data.ErrorCodeStatus = _tbErrorCodeStatus.Text ?? "";
    Data.ReadMoreStatus = _tbReadMoreStatus.Text ?? "";
    Data.NotFoundStatus = _tbNotFountStatus.Text ?? "";

    if (_tbWordsForChapter is not null && _tbWordsForVerse is not null && _tbChapterVerseSeparator is not null &&
        _tbVerseVerseSeparator is not null) {
      Data.WordsForChapter = _tbWordsForChapter.Data;
      Data.WordsForVerse = _tbWordsForVerse.Data;
      Data.ChapterVerseSeparator = _tbChapterVerseSeparator.Data;
      Data.VerseVerseSeparator = _tbVerseVerseSeparator.Data;
    }
    if (TbVerseSelectionWords is not null && TbWordsOrCharactersForListingReferences is not null) {
      Data.VerseSelectionWords = TbVerseSelectionWords.Data;
      Data.WordsOrCharactersForListingReferences = TbWordsOrCharactersForListingReferences.Data;
    }

    SettingsFiles.Get.SaveSettings();
  }
}

public class MiscDataComponentPartTwo : MiscDataComponent {
  protected override void InitializeControls() {
    PositionTopPart();

    TbVerseSelectionWords = ExpandingTextBoxes.Add(this, tb => tb.Below(),
        "Verse selection words", "(e.g. 'to', 'till', 'until')", TEXTBOX_WIDTH);
    TbWordsOrCharactersForListingReferences = ExpandingTextBoxes.Add(this, tb => tb.RightOf(TbVerseSelectionWords.FirstTextBox),
        "Words/characters for listing references", "(e.g. 'and', 'or', 'as well as')", TEXTBOX_WIDTH); // ',' ?

    NavigationControls.Add(this, SaveData);
  }
}
