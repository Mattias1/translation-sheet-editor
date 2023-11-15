using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using AvaloniaExtensions;
using TranslationSheetEditor.Model;

namespace TranslationSheetEditor.UI;

public sealed class BookListComponent : CanvasComponentBase {
  private const int LABEL_WIDTH = 120;

  private TranslationData? _data;
  private TranslationData Data => _data ??= GetSettings<TranslationData>();

  private readonly Dictionary<string, TextBox> _bibleBookTbs = new();

  protected override void InitializeControls() {
    Data.FirstTimeInit();

    var lbl1 = AddTextBlockHeader("Bible book translations").TopLeftInPanel().StretchFractionRightInPanel(1, 4);
    AddBookTextBoxes(BibleBooks.BOOKS_PT1);

    var lbl2 = AddTextBlock($"(language: {Data.Language})").FontWeight(FontWeight.Bold).FontStyle(FontStyle.Italic)
        .XRightOf(lbl1).YAlignBottom(lbl1).StretchFractionRightInPanel(1, 3);
    AddBookTextBoxes(BibleBooks.BOOKS_PT2);

    var lbl3 = AddTextBlock(".").RightOf(lbl2).StretchFractionRightInPanel(1, 2);
    AddBookTextBoxes(BibleBooks.BOOKS_PT3);

    var lbl4 = AddTextBlock(".").RightOf(lbl3).StretchRightInPanel();
    AddBookTextBoxes(BibleBooks.BOOKS_PT4);

    StretchBookTextBoxes(BibleBooks.BOOKS_PT1, tb => tb.StretchRightTo(lbl2));
    StretchBookTextBoxes(BibleBooks.BOOKS_PT2, tb => tb.StretchRightTo(lbl3));
    StretchBookTextBoxes(BibleBooks.BOOKS_PT3, tb => tb.StretchRightTo(lbl4));
    StretchBookTextBoxes(BibleBooks.BOOKS_PT4, tb => tb.StretchRightInPanel());

    NavigationControls.Add(this, SaveData);
  }

  private void AddBookTextBoxes(string[] bookNames) {
    foreach (string englishName in bookNames) {
      _bibleBookTbs.Add(englishName, AddTextBox().Margin(new Thickness(10, 9)).Below());
    }
    foreach (string englishName in bookNames) {
      InsertLabelLeftOf(englishName, _bibleBookTbs[englishName], LABEL_WIDTH);
    }
  }

  private void StretchBookTextBoxes(string[] bookNames, Action<TextBox> stretchFunc) {
    foreach (string englishName in bookNames) {
      stretchFunc(_bibleBookTbs[englishName]);
    }
  }

  protected override void OnLoaded(RoutedEventArgs e) {
    base.OnLoaded(e);
    LoadData();
    _bibleBookTbs[BibleBooks.GENESIS].Focus();
  }

  private void LoadData() {
    if (string.IsNullOrWhiteSpace(Data.Language)) {
      Data.Language = GetSettings<Settings>().SelectedLanguage;
    }
    foreach (var (book, textBox) in _bibleBookTbs) {
      textBox.Text = Data.BibleBooks[book].TranslatedName;
    }
  }

  private void SaveData() {
    foreach (var (book, textBox) in _bibleBookTbs) {
      Data.BibleBooks[book].TranslatedName = textBox.Text;
    }

    SettingsFiles.Get.SaveSettings();
  }
}
