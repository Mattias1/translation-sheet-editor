namespace TranslationSheetEditor.Model;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

public class TranslationData {
  // The language this translation data is for
  public string? Language { get; set; }

  // Bible book translations
  public BibleBooks BibleBooks { get; set; }

  // Visual translations
  public string LoadingStatus { get; set; }
  public string NoResultStatus { get; set; }
  public string ErrorCodeStatus { get; set; }
  public string ReadMoreStatus { get; set; }

  // Detection specific expressions
  public List<string> WordsForVerse { get; set; }
  public List<string> VerseSelectionWords { get; set; }
  public List<string> ChapterVerseSeparator { get; set; }
  public List<string> VerseVerseSeparator { get; set; }

  public void FirstTimeInit() {
    BibleBooks ??= BibleBooks.FirstTimeInit();
    LoadingStatus ??= "";
    NoResultStatus ??= "";
    ErrorCodeStatus ??= "";
    ReadMoreStatus ??= "";
    WordsForVerse ??= new List<string>();
    VerseSelectionWords ??= new List<string>();
    ChapterVerseSeparator ??= new List<string>();
    VerseVerseSeparator ??= new List<string>();
  }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
