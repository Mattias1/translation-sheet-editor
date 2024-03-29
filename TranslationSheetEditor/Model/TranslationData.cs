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
  public string NotFoundStatus { get; set; }

  // Detection specific expressions
  public List<string> WordsForChapter { get; set; }
  public List<string> WordsForVerse { get; set; }
  public List<string> ChapterVerseSeparator { get; set; }
  public List<string> VerseVerseSeparator { get; set; }
  public List<string> VerseSelectionWords { get; set; }
  public List<string> WordsOrCharactersForListingReferences { get; set; }

  // Prefix numbers
  public List<string> PrefixNumberOptionsForFirst { get; set; }
  public List<string> PrefixNumberOptionsForSecond { get; set; }
  public List<string> PrefixNumberOptionsForThird { get; set; }

  public List<string> GetPrefixNumberOptions(int i) => i switch {
      1 => PrefixNumberOptionsForFirst,
      2 => PrefixNumberOptionsForSecond,
      3 => PrefixNumberOptionsForThird,
      _ => throw new InvalidOperationException($"Invalid prefix number: {i}")
  };

  public void FirstTimeInit() {
    BibleBooks ??= BibleBooks.FirstTimeInit();
    LoadingStatus ??= "";
    NoResultStatus ??= "";
    ErrorCodeStatus ??= "";
    ReadMoreStatus ??= "";
    WordsForChapter ??= new List<string>();
    WordsForVerse ??= new List<string>();
    ChapterVerseSeparator ??= new List<string>();
    VerseVerseSeparator ??= new List<string>();
    VerseSelectionWords ??= new List<string>();
    WordsOrCharactersForListingReferences ??= new List<string>();
    PrefixNumberOptionsForFirst ??= new List<string>();
    PrefixNumberOptionsForSecond ??= new List<string>();
    PrefixNumberOptionsForThird ??= new List<string>();
  }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
