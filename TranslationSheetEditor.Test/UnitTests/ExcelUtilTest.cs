using FluentAssertions;
using TranslationSheetEditor.Model;
using TranslationSheetEditor.Utils;
using Xunit;

using ExcelRow = TranslationSheetEditor.Utils.ExcelUtil.ExcelRow;

namespace TranslationSheetEditor.Test.UnitTests;

public class ExcelUtilTest {
  public static string[] ALL_BOOKS = {
    "Genesis", "Exodus", "Leviticus", "Numbers", "Deuteronomy", "Joshua", "Judges", "Ruth",
    "1 Samuel", "2 Samuel", "1 Kings", "2 Kings", "1 Chronicles", "2 Chronicles",
    "Ezra", "Nehemiah", "Esther", "Job", "Psalms", "Proverbs", "Ecclesiastes", "Song of Songs",
    "Isaiah", "Jeremiah", "Lamentations", "Ezekiel", "Daniel", "Hosea", "Joel", "Amos", "Obadiah",
    "Jonah", "Micah", "Nahum", "Habakkuk", "Zephaniah", "Haggai", "Zechariah", "Malachi",
    "Matthew", "Mark", "Luke", "John", "Acts", "Romans",
    "1 Corinthians", "2 Corinthians", "Galatians", "Ephesians", "Philippians", "Colossians",
    "1 Thessalonians", "2 Thessalonians", "Timothy", "Titus", "Philemon", "Hebrews", "James",
    "1 Peter", "2 Peter", "1 John", "2 John", "3 John", "Jude", "Revelation"
  };

  [Fact]
  public void TestBuildingATranslationDataArray() {
    var data = BuildDutchTranslationDataExample();

    var excelData = ExcelUtil.BuildTranslationDataExcelArray(data);

    excelData.Length.Should().Be(2 + 66 + 2 + 4 + 2 + 4 + 2 + 3);

    excelData[0].IsHeader.Should().BeTrue();
    excelData[1].IsHeader.Should().BeTrue();
    excelData[2].IsHeader.Should().BeFalse();
    excelData[2].RawData.Should().BeEquivalentTo("", "Genesis", "", "Genesis", "Genesis|Gen", "", "Genesis", "Gen");
    excelData[3].RawData.Should().BeEquivalentTo("", "Exodus", "", "Exodus", "", "");
    excelData[12].RawData.Should().BeEquivalentTo("", "1 Kings", "", "1 Koningen", "", "");
    excelData[67].RawData.Should().BeEquivalentTo("", "Revelation", "", "Openbaring",
        "Openbaring|Op|Openbaringen", "", "Openbaring", "Op", "Openbaringen");

    excelData[69].IsHeader.Should().BeTrue();
    excelData[70].IsHeader.Should().BeFalse();
    excelData[70][3].Should().Be("Laden...");
    excelData[71][3].Should().Be("Geen resultaat");
    excelData[72][3].Should().Be("Fout code");
    excelData[73][3].Should().Be("Lees meer");

    excelData[75].IsHeader.Should().BeTrue();
    excelData[76].IsHeader.Should().BeFalse();
    excelData[76][3].Should().Be("Vers|vs");
    excelData[77][3].Should().Be("tot|tot en met|t/m");
    excelData[78][3].Should().Be(":");
    excelData[79][3].Should().Be("-");

    excelData[81].IsHeader.Should().BeTrue();
    excelData[82].IsHeader.Should().BeFalse();
    excelData[82][3].Should().Be("1|I|Een|Eén");
    excelData[83][3].Should().Be("2|II|Twee");
    excelData[84][3].Should().Be("3|III|Drie");
  }

  public static TranslationData BuildDutchTranslationDataExample() {
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

    data.WordsForVerse.AddRange(new[] { "Vers", "vs" });
    data.VerseSelectionWords.AddRange(new[] { "tot", "tot en met", "t/m" });
    data.ChapterVerseSeparator.Add(":");
    data.VerseVerseSeparator.Add("-");

    data.PrefixNumberOptionsForFirst.AddRange(new [] { "1", "I", "Een", "Eén" });
    data.PrefixNumberOptionsForSecond.AddRange(new [] { "2", "II", "Twee" });
    data.PrefixNumberOptionsForThird.AddRange(new [] { "3", "III", "Drie" });
    return data;
  }

  [Fact]
  public void TestParsingNicelyGeneratedRawExcelData() {
    var raw = new List<ExcelRow>(ExcelUtil.EXPECTED_NR_OF_ROWS);
    raw.AddHeader("English", "", "", "Target language (Nederlands)");
    raw.AddHeader("Biblebook translations", "Biblebook", "Expression", "Biblebook", "Expression", "", "Original inputs");
    AddBookRow(raw, BibleBooks.GENESIS, "Genesis", "Gen");
    AddBookRow(raw, BibleBooks.EXODUS, "Exodus", "Ex");
    for (int i = 2; i < 10; i++) {
      AddBookRow(raw, ALL_BOOKS[i]);
    }
    AddBookRow(raw, "1 Kings", "Koningen", "Kon", "Kn");
    for (int i = 11; i < 63; i++) {
      AddBookRow(raw, ALL_BOOKS[i]);
    }
    raw.AddRow("", BibleBooks.JUDE);
    AddBookRow(raw, BibleBooks.REVELATION, "Openbaring", "Op", "Openbaringen");

    raw.AddRow();
    raw.AddHeader("Visual translations");
    raw.AddRow("Word for loading a bible text", "Loading...", "", "Laden...");
    raw.AddRow("No result", "No result", "", "Geen resultaat");
    raw.AddRow("Error code", "Error code", "", "Fout code");
    raw.AddRow("Read more (Read further)", "Read mre", "", "Lees meer");

    raw.AddRow();
    raw.AddHeader("Detection specific expressions");
    raw.AddRegexRow("Words for bibleverse", "verse", null, "Vers", "vs");
    raw.AddRegexRow("Words to indicate selection verses", "to|till|until", null, "tot", "tot en met");
    raw.AddRegexRow("Optional: chapter:verse separators", ":", null, ":");
    raw.AddRegexRow("Optional: verse-verse separators", "-", null, "-");

    raw.AddRow();
    raw.AddHeader("Prefix numbers");
    raw.AddRegexRow("1 (First)", "1|I|1st|First", null, "1", "I", "Een", "Eén");
    raw.AddRegexRow("2 (Second)", "2|II|2nd|Second", null, "2", "II", "Twee");
    raw.AddRegexRow("3 (Third)", "3|III|3rd|Third", null, "3", "III", "Drie");

    var data = ExcelUtil.ParseRawData(raw.ToArray(), out string? errors);

    errors.Should().BeNull();

    data.Language.Should().Be("Nederlands");

    data.BibleBooks[BibleBooks.GENESIS].RegexParts.Should().BeEquivalentTo("Genesis", "Gen");
    data.BibleBooks[BibleBooks.EXODUS].RegexParts.Should().BeEquivalentTo("Exodus", "Ex");
    data.BibleBooks[BibleBooks.LEVITICUS].RegexParts.Should().BeEquivalentTo();
    data.BibleBooks[BibleBooks.KINGS].RegexParts.Should().BeEquivalentTo("Koningen", "Kon", "Kn");
    data.BibleBooks[BibleBooks.REVELATION].RegexParts.Should().BeEquivalentTo("Openbaring", "Op", "Openbaringen");

    data.LoadingStatus.Should().Be("Laden...");
    data.NoResultStatus.Should().Be("Geen resultaat");
    data.ErrorCodeStatus.Should().Be("Fout code");
    data.ReadMoreStatus.Should().Be("Lees meer");

    data.WordsForVerse.Should().BeEquivalentTo("Vers", "vs");
    data.VerseSelectionWords.Should().BeEquivalentTo("tot", "tot en met");
    data.ChapterVerseSeparator.Should().BeEquivalentTo(":");
    data.VerseVerseSeparator.Should().BeEquivalentTo("-");

    data.PrefixNumberOptionsForFirst.Should().BeEquivalentTo("1", "I", "Een", "Eén");
    data.PrefixNumberOptionsForSecond.Should().BeEquivalentTo("2", "II", "Twee");
    data.PrefixNumberOptionsForThird.Should().BeEquivalentTo("3", "III", "Drie");
  }

  [Fact]
  public void TestParsingOriginalFormattedRawExcelData() {
    var raw = new List<ExcelRow>(ExcelUtil.EXPECTED_NR_OF_ROWS + 8);
    raw.AddHeader("Bible Link Multilingual translation sheet");
    raw.AddRow("Before adding a new translation to this file please read the guidelines below carefully!");
    raw.AddRow("Check out the plugin page for seeing it in action");
    raw.AddRow();
    raw.AddHeader("1. Biblebook translation guidelines");
    raw.AddRow("More stuff", "Blahblahblah", "etc.", "...");
    raw.AddRow();
    raw.AddHeader("2. Translation Sheet");

    raw.AddHeader("English", "", "", "Nederlands");
    raw.AddHeader("Biblebook translations", "Biblebook", "Expression", "Biblebook", "Expression");
    raw.AddRow("gen", BibleBooks.GENESIS, "Genesis|Gen?|Gn", "Genesis", "Genesis|Gen");
    raw.AddRow("exo", BibleBooks.EXODUS, "Exodus|Ex|Exod?", "Exodus", "Exodus|Ex");
    for (int i = 2; i < 10; i++) {
      AddBookRow(raw, ALL_BOOKS[i]);
    }
    // raw.AddRow("1ki", "1 Kings", "(1|I|1st|First) ?Kings|(1|I) ?Kgs|(1|I) ?Ki",
    //     "1 Koningen", "(1|I|Een) ?Koningen|(1|I) ?Ko?n");
    raw.AddRow("1ki", "1 Kings", "(1|I|1st|First) ?Kings|(1|I) ?Kgs|(1|I) ?Ki",
        "1 Koningen", "1 ?Koningen|I Koningen|Een Koningen|1 ?Ko?n|I Ko?n");
    // TODO: Kn to 2 kings, to test adding if not exists
    for (int i = 11; i < 63; i++) {
      AddBookRow(raw, ALL_BOOKS[i]);
    }
    raw.AddRow("", BibleBooks.JUDE);
    raw.AddRow("rev", BibleBooks.REVELATION, "Revelation|Rev?|The Revelation|Apocalypse|Apoc?|Ap",
        "Openbaring", "Openbaring|Op|Openbaringen");

    raw.AddRow();
    raw.AddHeader("Visual translations");
    raw.AddRow("Word for loading a bible text", "Loading...", "", "Laden...");
    raw.AddRow("No result", "No result", "", "Geen resultaat");
    raw.AddRow("Error code", "Error code", "", "Fout code");
    raw.AddRow("Read more (Read further)", "Read mre", "", "Lees meer");

    raw.AddRow();
    raw.AddHeader("Detection specific expressions");
    raw.AddRow("Words for bibleverse", "verse", "", "Vers|vs");
    raw.AddRow("Words to indicate selection verses", "to|till|until", "", "tot|tot en met");
    raw.AddRow("Optional: chapter:verse separators", ":", "", ":");
    raw.AddRow("Optional: verse-verse separators", "-", "", "-");

    var data = ExcelUtil.ParseRawData(raw.ToArray(), out string? errors);

    // TODO
    // errors.Should().BeNull();

    data.Language.Should().Be("Nederlands");

    data.BibleBooks[BibleBooks.GENESIS].RegexParts.Should().BeEquivalentTo("Genesis", "Gen");
    data.BibleBooks[BibleBooks.EXODUS].RegexParts.Should().BeEquivalentTo("Exodus", "Ex");
    data.BibleBooks[BibleBooks.LEVITICUS].RegexParts.Should().BeEquivalentTo();
    // TODO
    // data.BibleBooks[BibleBooks.KINGS].RegexParts.Should().BeEquivalentTo("Koningen", "Kon", "Kn");
    data.BibleBooks[BibleBooks.REVELATION].RegexParts.Should().BeEquivalentTo("Openbaring", "Op", "Openbaringen");

    data.LoadingStatus.Should().Be("Laden...");
    data.NoResultStatus.Should().Be("Geen resultaat");
    data.ErrorCodeStatus.Should().Be("Fout code");
    data.ReadMoreStatus.Should().Be("Lees meer");

    data.WordsForVerse.Should().BeEquivalentTo("Vers", "vs");
    data.VerseSelectionWords.Should().BeEquivalentTo("tot", "tot en met");
    data.ChapterVerseSeparator.Should().BeEquivalentTo(":");
    data.VerseVerseSeparator.Should().BeEquivalentTo("-");

    // TODO
    // data.PrefixNumberOptionsForFirst.Should().BeEquivalentTo("1", "I", "Een", "Eén");
    // data.PrefixNumberOptionsForSecond.Should().BeEquivalentTo("2", "II", "Twee");
    // data.PrefixNumberOptionsForThird.Should().BeEquivalentTo("3", "III", "Drie");
  }

  private void AddBookRow(List<ExcelRow> raw, string book, params string[] inputs) {
    AddBookRow(raw, book, inputs.ToList());
  }
  private void AddBookRow(List<ExcelRow> raw, string book, List<string> inputs) {
    raw.AddRegexRow("", book, inputs.FirstOrDefault() ?? "", inputs);
  }
}
