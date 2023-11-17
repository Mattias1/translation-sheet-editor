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

public sealed class RegexComponent : CanvasComponentBase {
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
      string displayName = Data.BibleBooks[book].DisplayName;
      var textBoxes = ExpandingTextBoxes.Add(this, tb => tb.RightOf(separator), tb => tb.StretchRightInPanel(),
          displayName, "(e.g. 'Genesis',\n 'Gen', 'Ge', 'Gn')", LABEL_WIDTH);
      textBoxes.Data = Data.BibleBooks[book].RegexParts;
      if (textBoxes.Data.All(string.IsNullOrWhiteSpace)) {
        textBoxes.FirstTextBox.Text = Data.BibleBooks[book].TranslatedName;
      }
      textBoxes.ValidateOnChange(content => {
        string currentName = textBoxes.Label.Content?.ToString() ?? string.Empty;
        foreach (var (otherName, bookData) in Data.BibleBooks.BibleBookData) {
          if (currentName != otherName && bookData.RegexParts.Contains(content)
              && !(currentName.StartsWith("John") && otherName.StartsWith("John"))) { // The two John's are allowed to conflict.
            return $"Conflicts with\n'{otherName}'.";
          }
        }
        return "";
      });
      _tbRegexOptions.Add(book, textBoxes);
    }
    AddTextBlock("Alternatives (abbreviations)").XAlignLeft(_tbRegexOptions[BibleBooks.GENESIS].Label).YBelow(header);

    NavigationControls.Add(this, SaveData);
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

  public void ChangeBookIndex(int newIndex) {
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
    string bookListRegex = ""; // Genesis|gen|Exodus|ex|...
    foreach (var (_, data) in Data.BibleBooks.BibleBookData) {
      bookListRegex += data.RegexParts.ToRegex() + '|';
    }
    bookListRegex = bookListRegex.Trim('|');
    string chapterVerseRegex = Data.ChapterVerseSeparator.Concat(Data.WordsForVerse).ToRegex(); // : , vers v
    string verseVerseRegex = Data.VerseVerseSeparator.Concat(Data.VerseSelectionWords).ToRegex(); // - tot t/m
    string pattern = "(?<!\\p{L}|\\p{N})"
      + $"(?<book>{bookListRegex})\\.?\\s*"
      + "(?<chapter>[0-9]+)"
      + $"(\\s*({chapterVerseRegex})?\\s*(?<verse>[0-9]+)[abc]?)?"
      + "((\\s*"
      + $"({verseVerseRegex})\\s*" // TODO: Unicode dashes: – —
      + "(?<chapterEnd>[0-9]+)"
      + $"(\\s*({chapterVerseRegex})?\\s*"
      + "(?<verseEnd>[0-9]+)"
      + ")?)" // TODO FOR BELOW: Is the 'en' even in the list of things you can provide?
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
