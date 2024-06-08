using TranslationSheetEditor.Model;

namespace TranslationSheetEditor.Test.TestUtils;

public static class TranslationDataTestUtil {
  public static TranslationData BuildPartialDutchExample() {
    var data = new TranslationData();
    data.FirstTimeInit();
    data.Language = "Nederlands";

    data.BibleBooks.BibleBookData[BibleBooks.GENESIS].TranslatedName = "Genesis";
    data.BibleBooks.BibleBookData[BibleBooks.GENESIS].RegexParts
        .AddRange(new[] { "Genesis", "Gen" });
    data.BibleBooks.BibleBookData[BibleBooks.EXODUS].TranslatedName = "Exodus";
    data.BibleBooks.BibleBookData[BibleBooks.KINGS].TranslatedName = "Koningen";
    data.BibleBooks.BibleBookData[BibleBooks.REVELATION].TranslatedName = "Openbaring";
    data.BibleBooks.BibleBookData[BibleBooks.REVELATION].RegexParts
        .AddRange(new [] { "Openbaring", "Op", "Openbaringen" });

    data.LoadingStatus = "Laden...";
    data.NoResultStatus = "Geen resultaat";
    data.ErrorCodeStatus = "Fout code";
    data.ReadMoreStatus = "Lees meer";
    data.NotFoundStatus = "Niet gevonden";

    data.WordsForVerse.AddRange(new[] { "Vers", "vs" });
    data.VerseSelectionWords.AddRange(new[] { "tot", "tot en met", "t/m" });
    data.ChapterVerseSeparator.Add(":");
    data.VerseVerseSeparator.Add("-");
    data.WordsForChapter.AddRange(new [] { "hoofdstuk", "h" });
    data.WordsOrCharactersForListingReferences.AddRange(new [] { "en", "of", "en ook" });

    data.PrefixNumberOptionsForFirst.AddRange(new [] { "1", "I", "Een", "Eén" });
    data.PrefixNumberOptionsForSecond.AddRange(new [] { "2", "II", "Twee" });
    data.PrefixNumberOptionsForThird.AddRange(new [] { "3", "III", "Drie" });
    return data;
  }

  public static TranslationData BuildFullEnglishExample() {
    var data = new TranslationData();
    data.FirstTimeInit();
    data.Language = "English";

    foreach (string book in BibleBooks.ALL_BOOKS) {
      string translation = book == BibleBooks.JOHN_LETTER ? BibleBooks.JOHN : book;
      data.BibleBooks.BibleBookData[book].TranslatedName = translation;
      data.BibleBooks.BibleBookData[book].RegexParts.Add(translation);
    }

    data.LoadingStatus = "Loading...";
    data.NoResultStatus = "No result";
    data.ErrorCodeStatus = "Error code";
    data.ReadMoreStatus = "Read more";
    data.NotFoundStatus = "Not found";

    data.WordsForVerse.AddRange(new[] { "Verse", "vs" });
    data.VerseSelectionWords.AddRange(new[] { "to", "until" });
    data.ChapterVerseSeparator.Add(":");
    data.VerseVerseSeparator.Add("-");
    data.WordsForChapter.AddRange(new [] { "chapter", "ch" });
    data.WordsOrCharactersForListingReferences.AddRange(new [] { "and", "or" });

    data.PrefixNumberOptionsForFirst.AddRange(new [] { "1", "I", "One" });
    data.PrefixNumberOptionsForSecond.AddRange(new [] { "2", "II", "Two" });
    data.PrefixNumberOptionsForThird.AddRange(new [] { "3", "III", "Three" });
    return data;
  }
}
