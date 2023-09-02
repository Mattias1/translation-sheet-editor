namespace TranslationSheetEditor.Model;

public class TranslationData {
  // The language this translation data is for
  public string? Language { get; set; }

  // Bible book translations
  public BibleBooks BibleBooks { get; } = new();

  // Visual translations
  public string LoadingStatus { get; set; } = "";
  public string NoResultStatus { get; set; } = "";
  public string ErrorCodeStatus { get; set; } = "";
  public string ReadMoreStatus { get; set; } = "";

  // Detection specific expressions
  public List<string> WordsForVerse { get; } = new();
  public List<string> VerseSelectionWords { get; } = new();
  public List<string> ChapterVerseSeparator { get; } = new();
  public List<string> VerseVerseSeparator { get; } = new();
}
