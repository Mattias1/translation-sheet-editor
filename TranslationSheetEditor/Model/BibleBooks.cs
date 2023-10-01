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
  public const string ONE_SAMUEL = "1 Samuel";
  public const string TWO_SAMUEL = "2 Samuel";
  public const string ONE_KINGS = "1 Kings";
  public const string TWO_KINGS = "2 Kings";
  public const string ONE_CHRONICLES = "1 Chronicles";
  public const string TWO_CHRONICLES = "2 Chronicles";
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
  public const string ONE_CORINTHIANS = "1 Corinthians";
  public const string TWO_CORINTHIANS = "2 Corinthians";
  public const string GALATIANS = "Galatians";
  public const string EPHESIANS = "Ephesians";
  public const string PHILIPPIANS = "Philippians";
  public const string COLOSSIANS = "Colossians";
  public const string ONE_THESSALONIANS = "1 Thessalonians";
  public const string TWO_THESSALONIANS = "2 Thessalonians";
  public const string ONE_TIMOTHY = "1 Timothy";
  public const string TWO_TIMOTHY = "2 Timothy";
  public const string TITUS = "Titus";
  public const string PHILEMON = "Philemon";
  public const string HEBREWS = "Hebrews";
  public const string JAMES = "James";
  public const string ONE_PETER = "1 Peter";
  public const string TWO_PETER = "2 Peter";
  public const string ONE_JOHN = "1 John";
  public const string TWO_JOHN = "2 John";
  public const string THREE_JOHN = "3 John";
  public const string JUDE = "Jude";
  public const string REVELATION = "Revelation";

  public static readonly string[] BOOKS_PT1 = {
      GENESIS, EXODUS, LEVITICUS, NUMBERS, DEUTERONOMY, JOSHUA, JUDGES, RUTH, ONE_SAMUEL, TWO_SAMUEL,
      ONE_KINGS, TWO_KINGS, ONE_CHRONICLES, TWO_CHRONICLES, EZRA, NEHEMIAH, ESTHER
  };
  public static readonly string[] BOOKS_PT2 = {
      JOB, PSALMS, PROVERBS, ECCLESIASTES, SONG, ISAIAH, JEREMIAH, LAMENTATIONS, EZEKIEL, DANIEL, HOSEA,
      JOEL, AMOS, OBADIAH, JONAH, MICAH, NAHUM
  };
  public static readonly string[] BOOKS_PT3 = {
      HABAKKUK, ZEPHANIAH, HAGGAI, ZECHARIAH, MALACHI, MATTHEW, MARK, LUKE, JOHN, ACTS, ROMANS,
      ONE_CORINTHIANS, TWO_CORINTHIANS, GALATIANS, EPHESIANS, PHILIPPIANS, COLOSSIANS
  };
  public static readonly string[] BOOKS_PT4 = {
      ONE_THESSALONIANS, TWO_THESSALONIANS, ONE_TIMOTHY, TWO_TIMOTHY, TITUS, PHILEMON, HEBREWS,
      JAMES, ONE_PETER, TWO_PETER, ONE_JOHN, TWO_JOHN, THREE_JOHN, JUDE, REVELATION
  };
  public static readonly string[] ALL_BOOKS = BOOKS_PT1.Concat(BOOKS_PT2)
      .Concat(BOOKS_PT3).Concat(BOOKS_PT4).ToArray();

  public Dictionary<string, BibleBookData> BibleBookData { get; set; } // This one is public for the JSON serializer

  public BibleBookData this[string englishBookName] => BibleBookData[englishBookName];

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
  public string EnglishName { get; private set; }
  public string? TranslatedName { get; set; }
  public List<string> RegexParts { get; set; }

  public static BibleBookData FirstTimeInit(string englishName) {
    return new BibleBookData { EnglishName = englishName, RegexParts = new List<string>() };
  }
}
