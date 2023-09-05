using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using AvaloniaExtensions;
using TranslationSheetEditor.Model;

namespace TranslationSheetEditor.UI;

public sealed class MiscDataComponent : CanvasComponentBaseHack {
  private const int LABEL_WIDTH = 250;
  private const int SEPARATOR_WIDTH = 100;

  private TranslationData? _data;
  private TranslationData Data => _data ??= GetSettings<TranslationData>();

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

    _tbWordsForVerse = ExpandingTextBoxes.Add(this, tb => tb.Below(), "Words for 'Verse'", "(e.g. 'verse',\n 'vs', 'v')");
    _tbVerseSelectionWords = ExpandingTextBoxes.Add(this, tb => tb.RightOf(_tbWordsForVerse.FirstTextBox),
        "Verse selection words", "(e.g. 'to', 'till', 'until')");
    _tbChapterVerseSeparator = ExpandingTextBoxes.Add(this, tb => tb.RightOf(_tbVerseSelectionWords.FirstTextBox),
        "Chapter-verse separators", "(e.g. ':')", SEPARATOR_WIDTH);
    _tbVerseVerseSeparator = ExpandingTextBoxes.Add(this, tb => tb.RightOf(_tbChapterVerseSeparator.FirstTextBox),
        "Verse-verse separators", "(e.g. '-')", SEPARATOR_WIDTH); // TODO: Add alternatives for dashes (like long dashes and other unicode alternatives)

    InsertLabelLeftOf("'Loading...'", _tbLoadingStatus, LABEL_WIDTH);
    InsertLabelLeftOf("'Result'", _tbNoResultStatus, LABEL_WIDTH);
    InsertLabelLeftOf("'Error code'", _tbErrorCodeStatus, LABEL_WIDTH);
    InsertLabelLeftOf("'Read more' (or 'Read further')", _tbReadMoreStatus, LABEL_WIDTH);

    AddTextBlock($"(language: {Data.Language})").FontWeight(FontWeight.Bold).FontStyle(FontStyle.Italic)
        .Above(_tbLoadingStatus);

    AddButton("Next", OnNextClick).BottomRightInPanel();
    AddButton("Previous", OnPreviousClick).LeftOf();
  }

  protected override void OnLoaded(RoutedEventArgs e) {
    base.OnLoaded(e);
    LoadData();

    HandleResize();
  }

  private void OnNextClick(RoutedEventArgs e) {
    SaveData();
  }
  private void OnPreviousClick(RoutedEventArgs e) {
    SaveData();
    SwitchToComponent<BookListComponent>();
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

public abstract class CanvasComponentBaseHack : CanvasComponentBase {
  public void HandleResizeHack() => HandleResize();
}

public sealed class ExpandingTextBoxes {
  private CanvasComponentBaseHack _parent;
  private double _textBoxWidth;
  private List<TextBox> _textBoxes;

  public Label Label { get; private set; } = null!;
  public Label? Description { get; private set; }

  public IReadOnlyList<TextBox> TextBoxes => _textBoxes.AsReadOnly();
  public TextBox FirstTextBox => _textBoxes[0];
  public TextBox LastTextBox => _textBoxes[^1];
  public TextBox ButOneLastTextBox => _textBoxes[^2];

  public List<string> Data {
    get => _textBoxes.Select(tb => tb.Text ?? "").ToList();
    set {
      for (int i = 0; i < value.Count && i < _textBoxes.Count; i++) {
        _textBoxes[i].Text = value[i];
      }
    }
  }

  private ExpandingTextBoxes(CanvasComponentBaseHack parent, double textBoxWidth) {
    _parent = parent;
    _textBoxes = new List<TextBox>();
    _textBoxWidth = textBoxWidth;
  }

  public ExpandingTextBoxes IsVisible(bool isVisible) {
    _textBoxes.ForEach(tb => tb.IsVisible(isVisible));
    Label.IsVisible(isVisible);
    Description?.IsVisible(isVisible);
    return this;
  }

  private void OnTextChanged(TextChangedEventArgs e) {
    if (ReferenceEquals(e.Source, LastTextBox) && !string.IsNullOrEmpty(LastTextBox.Text)) {
      AddTextBoxBelow();
    }
  }

  private TextBox AddTextBoxBelow() {
    var textBox = AddTextBox().Below(ButOneLastTextBox);
    _parent.HandleResizeHack();
    return textBox;
  }

  private TextBox AddTextBox() {
    var textBox = _parent.AddTextBox().OnTextChanged(OnTextChanged);
    if (!double.IsNaN(_textBoxWidth)) {
      textBox.MinWidth(_textBoxWidth);
    }
    _textBoxes.Add(textBox);
    return textBox;
  }

  private void AddLabels(string text, string? description, double labelWidth) {
    Label = _parent.InsertLabelLeftOf(text, FirstTextBox, labelWidth);
    if (description is not null) {
      Description = _parent.AddLabel(description, FirstTextBox).Below(Label);
    }
  }

  public static ExpandingTextBoxes Add(CanvasComponentBaseHack parent, Func<TextBox, TextBox> initialPositionFunc,
      string labelText, string? description = null, double textBoxWidth = double.NaN, double labelWidth = double.NaN) {
    var expandingTextboxes = new ExpandingTextBoxes(parent, textBoxWidth);

    initialPositionFunc(expandingTextboxes.AddTextBox());
    expandingTextboxes.AddLabels(labelText, description, labelWidth);
    expandingTextboxes.AddTextBoxBelow();

    return expandingTextboxes;
  }
}
