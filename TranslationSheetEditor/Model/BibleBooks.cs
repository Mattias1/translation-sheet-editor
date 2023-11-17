namespace TranslationSheetEditor.Model;

public class BibleBooks {
  public const string GENESIS = "Genesis";
  public const string EXODUS = "Exodus";
  public const string LEVITICUS = "Leviticus";
  public const string NUMBERS = "Numbers";
  public const string DEUTERONOMY = "Deuteronomy";
  public const string JOSHUA = "Joshua";
  public const string JUDGES = "Judges";
  public const string RUTH = "Ruth";
  public const string SAMUEL = "Samuel"; // Has 2
  public const string KINGS = "Kings"; // Has 2
  public const string CHRONICLES = "Chronicles"; // Has 2
  public const string EZRA = "Ezra";
  public const string NEHEMIAH = "Nehemiah";
  public const string ESTHER = "Esther";
  public const string JOB = "Job";
  public const string PSALMS = "Psalms";
  public const string PROVERBS = "Proverbs";
  public const string ECCLESIASTES = "Ecclesiastes";
  public const string SONG = "Song of Songs";
  public const string ISAIAH = "Isaiah";
  public const string JEREMIAH = "Jeremiah";
  public const string LAMENTATIONS = "Lamentations";
  public const string EZEKIEL = "Ezekiel";
  public const string DANIEL = "Daniel";
  public const string HOSEA = "Hosea";
  public const string JOEL = "Joel";
  public const string AMOS = "Amos";
  public const string OBADIAH = "Obadiah";
  public const string JONAH = "Jonah";
  public const string MICAH = "Micah";
  public const string NAHUM = "Nahum";
  public const string HABAKKUK = "Habakkuk";
  public const string ZEPHANIAH = "Zephaniah";
  public const string HAGGAI = "Haggai";
  public const string ZECHARIAH = "Zechariah";
  public const string MALACHI = "Malachi";
  public const string MATTHEW = "Matthew";
  public const string MARK = "Mark";
  public const string LUKE = "Luke";
  public const string JOHN = "John";
  public const string ACTS = "Acts";
  public const string ROMANS = "Romans";
  public const string CORINTHIANS = "Corinthians"; // Has 2
  public const string GALATIANS = "Galatians";
  public const string EPHESIANS = "Ephesians";
  public const string PHILIPPIANS = "Philippians";
  public const string COLOSSIANS = "Colossians";
  public const string THESSALONIANS = "Thessalonians"; // Has 2
  public const string TIMOTHY = "Timothy"; // Has 2
  public const string TITUS = "Titus";
  public const string PHILEMON = "Philemon";
  public const string HEBREWS = "Hebrews";
  public const string JAMES = "James";
  public const string PETER = "Peter"; // Has 2
  public const string JOHN_LETTER = "John (letter)"; // Has 3
  public const string JUDE = "Jude";
  public const string REVELATION = "Revelation";

  public static readonly string[] BOOKS_PT1 = {
      GENESIS, EXODUS, LEVITICUS, NUMBERS, DEUTERONOMY, JOSHUA, JUDGES, RUTH, SAMUEL,
      KINGS, CHRONICLES, EZRA, NEHEMIAH, ESTHER, JOB
  };
  public static readonly string[] BOOKS_PT2 = {
      PSALMS, PROVERBS, ECCLESIASTES, SONG, ISAIAH, JEREMIAH, LAMENTATIONS, EZEKIEL, DANIEL, HOSEA,
      JOEL, AMOS, OBADIAH, JONAH, MICAH
  };
  public static readonly string[] BOOKS_PT3 = {
      NAHUM, HABAKKUK, ZEPHANIAH, HAGGAI, ZECHARIAH, MALACHI, MATTHEW, MARK, LUKE, JOHN, ACTS,
      ROMANS, CORINTHIANS, GALATIANS, EPHESIANS
  };
  public static readonly string[] BOOKS_PT4 = {
      PHILIPPIANS, COLOSSIANS, THESSALONIANS, TIMOTHY, TITUS, PHILEMON, HEBREWS,
      JAMES, PETER, JOHN_LETTER, JUDE, REVELATION
  };
  public static readonly string[] ALL_BOOKS = BOOKS_PT1.Concat(BOOKS_PT2)
      .Concat(BOOKS_PT3).Concat(BOOKS_PT4).ToArray();

  public Dictionary<string, BibleBookData> BibleBookData { get; set; } // This one is public for the JSON serializer

  public BibleBookData this[string englishBookName] => BibleBookData[englishBookName];

  public bool ContainsKey(string englishBookName) => BibleBookData.ContainsKey(englishBookName);

  public static BibleBooks FirstTimeInit() {
    var result = new BibleBooks();
    result.BibleBookData = new Dictionary<string, BibleBookData>(ALL_BOOKS.Length);
    foreach (string book in ALL_BOOKS) {
      result.BibleBookData[book] = Model.BibleBookData.FirstTimeInit(book);
    }
    return result;
  }
}

public class BibleBookData {
  public string EnglishName { get; set; } // I want this to be private - but the JSON encoder doesn't allow it :(
  public string DisplayName => EnglishName == BibleBooks.JOHN_LETTER ? "John" : EnglishName;
  public string? TranslatedName { get; set; }
  public List<string> RegexParts { get; set; }

  public int NumberOfBooks() => EnglishName switch {
      BibleBooks.SAMUEL => 2,
      BibleBooks.KINGS => 2,
      BibleBooks.CHRONICLES => 2,
      BibleBooks.CORINTHIANS => 2,
      BibleBooks.THESSALONIANS => 2,
      BibleBooks.TIMOTHY => 2,
      BibleBooks.PETER => 2,
      BibleBooks.JOHN_LETTER => 3,
      _ => 1
  };

  public static BibleBookData FirstTimeInit(string englishName) {
    return new BibleBookData { EnglishName = englishName, RegexParts = new List<string>() };
  }
}
