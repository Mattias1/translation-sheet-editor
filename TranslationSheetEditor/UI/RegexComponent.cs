using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using AvaloniaExtensions;
using System.Text.RegularExpressions;
using TranslationSheetEditor.Model;
using TranslationSheetEditor.Utils;

namespace TranslationSheetEditor.UI;

public sealed class RegexComponent : CanvasComponentBaseHack {
  private const int LABEL_WIDTH = 130;

  private Settings? _settings;
  private Settings Settings => _settings ??= SettingsFiles.Get.GetSettings<Settings>();

  private TranslationData? _data;
  private TranslationData Data => _data ??= SettingsFiles.Get.GetSettings<TranslationData>();

  private readonly ThemedBrushes _regexHighlight = ThemedBrushes.FromHex("#008000", "#FFDD33");

  private readonly string[] _allBooks = BibleBooks.ALL_BOOKS;
  private int _currentBookIndex = 0;
  private TextBox _tbPreviewEditor = null!;
  private TextBlock _tbPreview = null!;
  private Button _btnPreviewToggle = null!;
  private readonly Dictionary<string, ExpandingTextBoxes> _tbRegexOptions = new();
  private ComboBox _cbBookSelection = null!;

  protected override void InitializeControls() {
    var header = AddTextBlockHeader("Bible book alternatives").TopLeftInPanel();
    AddTextBlock($"(language: {Data.Language})").FontWeight(FontWeight.Bold).FontStyle(FontStyle.Italic)
        .XRightOf(header).YAlignBottom(header);
    AddTextBlock("Below is a preview to see the result of the translations").Below(header);
    var separator = AddSeparator().Below().StretchFractionRightInPanel(3, 4);

    _btnPreviewToggle = AddButton("Toggle preview", OnPreviewToggle).BottomLeftInPanel();
    _tbPreviewEditor = AddMultilineTextBox(Settings.PreviewText).Below(separator)
        .StretchFractionRightInPanel(3, 4).StretchDownTo(_btnPreviewToggle);
    _tbPreview = Add(new TextBlock()).TextWrapping(TextWrapping.Wrap).Below(separator)
        .StretchFractionRightInPanel(3, 4).StretchDownTo(_btnPreviewToggle);

    foreach (string book in BibleBooks.ALL_BOOKS) {
      var textBoxes = ExpandingTextBoxes.Add(this, tb => tb.RightOf(separator), book,
              "(e.g. 'Genesis',\n 'Gen', 'Ge', 'Gn')", double.NaN, LABEL_WIDTH);
      textBoxes.Data = Data.BibleBooks[book].RegexParts;
      if (textBoxes.Data.All(string.IsNullOrWhiteSpace)) {
        textBoxes.FirstTextBox.Text = Data.BibleBooks[book].TranslatedName;
      }
      _tbRegexOptions.Add(book, textBoxes);
    }
    AddTextBlock("Alternatives (abbreviations)").XAlignLeft(_tbRegexOptions[BibleBooks.GENESIS].Label).YBelow(header);

    AddButton("Next", OnNextClick).BottomRightInPanel();
    _cbBookSelection = AddComboBox(BibleBooks.ALL_BOOKS, OnBookComboboxChanged).Width(LABEL_WIDTH).LeftOf();
    AddButton("Previous", OnPreviousClick).LeftOf();
  }

  private void OnPreviewToggle(RoutedEventArgs e) {
    _tbPreview.Text = _tbPreviewEditor.Text;
    Settings.PreviewText = _tbPreviewEditor.Text ?? "";
    _tbPreviewEditor.IsVisible(_tbPreview.IsVisible);
    _tbPreview.IsVisible(!_tbPreview.IsVisible);
  }

  protected override void OnLoaded(RoutedEventArgs e) {
    base.OnLoaded(e);

    _tbPreview.Inlines = BuildHighlightedPreviewText();
    SetVisibility();
  }

  private void OnNextClick(RoutedEventArgs e) {
    if (_currentBookIndex >= _allBooks.Length - 1) {
      SaveData();
      SwitchToComponent<ExportComponent>();
    } else {
      _cbBookSelection.SelectedIndex += 1;
    }
  }

  private void OnPreviousClick(RoutedEventArgs e) {
    if (_currentBookIndex <= 0) {
      SaveData();
      _currentBookIndex = 0;
      SwitchToComponent<MiscDataComponent>();
    } else {
      _cbBookSelection.SelectedIndex -= 1;
    }
  }

  private void OnBookComboboxChanged(SelectedItemChangedEventArgs<string> e) {
    SaveData();

    int i = Array.IndexOf(BibleBooks.ALL_BOOKS, e.SelectedItem);
    ChangeBookIndex(i);
  }

  private void ChangeBookIndex(int newIndex) {
    _tbRegexOptions[_allBooks[_currentBookIndex]].IsVisible(false);
    _currentBookIndex = newIndex;
    _tbRegexOptions[_allBooks[_currentBookIndex]].IsVisible(true);

    _tbPreview.Inlines = BuildHighlightedPreviewText();
  }

  private void SetVisibility() {
    bool previewTextPresent = !string.IsNullOrWhiteSpace(Settings.PreviewText);
    _tbPreviewEditor.IsVisible(!previewTextPresent);
    _tbPreview.IsVisible(previewTextPresent);

    foreach (var (book, tb) in _tbRegexOptions) {
      tb.IsVisible(book == _allBooks[_currentBookIndex]);
    }
  }

  private void SaveData() {
    Settings.PreviewText = _tbPreviewEditor.Text ?? "";

    foreach (string book in BibleBooks.ALL_BOOKS) {
      Data.BibleBooks[book].RegexParts = _tbRegexOptions[book].Data;
    }

    SettingsFiles.Get.SaveSettings();
  }

  private InlineCollection BuildHighlightedPreviewText() {
    string bookRegexList = "";
    foreach (var (_, data) in Data.BibleBooks.BibleBookData) {
      bookRegexList += data.RegexParts.ToRegex() + '|';
    }
    bookRegexList = bookRegexList.Trim('|');
    string pattern = "(?<!\\p{L}|\\p{N})"
      + $"(?<book>{bookRegexList})\\.?\\s*"
      + "(?<chapter>[0-9]+)"
      + "(\\s*(:|,|verse|vers|v\\.)?\\s*(?<verse>[0-9]+)[abc]?)?" // TODO - NOTE: We may want to replace these too
      + "((\\s*"
      + "(-|–|—|tot|t\\/m|tm)\\s*" // NOTE: We may want to replace these too
      + "(?<chapterEnd>[0-9]+)"
      + "(\\s*(:|,|verse|vers|v\\.)?\\s*" // NOTE: We may want to replace these too
      + "(?<verseEnd>[0-9]+)"
      + ")?)" // NOTE FOR BELOW: Is the 'en' even in the list of things you can provide?
      + "|(\\s*(,|en|\\+)\\s*(?<verse2>[0-9]+)(?!\\s?[a-z])))?(?!\\p{L})";
    var regex = new Regex(pattern, RegexOptions.IgnoreCase);

    var inlines = new InlineCollection();
    var matches = regex.Matches(Settings.PreviewText);
    int previousIndex = 0;
    foreach (Match match in matches) {
      inlines.Add(new Run(Settings.PreviewText.Substring(previousIndex, match.Index - previousIndex)));
      inlines.Add(new Run(Settings.PreviewText.Substring(match.Index, match.Length)) {
          Foreground = _regexHighlight.ForTheme(ActualThemeVariant),
          FontWeight = FontWeight.Bold
      });
      previousIndex = match.Index + match.Length;
    }
    inlines.Add(new Run(Settings.PreviewText.Substring(previousIndex)));

    return inlines;
  }
}
