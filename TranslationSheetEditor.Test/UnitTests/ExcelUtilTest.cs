using FluentAssertions;
using TranslationSheetEditor.Model;
using TranslationSheetEditor.Utils;
using Xunit;

using ExcelRow = TranslationSheetEditor.Utils.ExcelUtil.ExcelRow;

namespace TranslationSheetEditor.Test.UnitTests;

public class ExcelUtilTest {
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

  // TODO: This is (I think) my version of the excel sheet, which is easier to read
  // TODO: I should also make a test for their (old) version, which is harder (and has 'junk' up front)
  [Fact]
  public void TestParsingRawExcelData() {
    var raw = new List<ExcelRow>(ExcelUtil.EXPECTED_NR_OF_ROWS);
    raw.AddHeader("English", "", "", "Target language (Nederlands)");
    raw.AddHeader("Biblebook translations", "Biblebook", "Expression", "Biblebook", "Expression", "", "Original inputs");
    AddBookRow(raw, BibleBooks.GENESIS, "Genesis", "Gen");
    AddBookRow(raw, BibleBooks.EXODUS, "Exodus", "Ex");
    for (int i = 2; i < BibleBooks.ALL_BOOKS.Length - 2; i++) {
      AddBookRow(raw, BibleBooks.ALL_BOOKS[i]);
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

    var data = ExcelUtil.ParseRawData(raw.ToArray());

    data.Language.Should().Be("Nederlands");

    data.BibleBooks[BibleBooks.GENESIS].RegexParts.Should().BeEquivalentTo("Genesis", "Gen");
    data.BibleBooks[BibleBooks.EXODUS].RegexParts.Should().BeEquivalentTo("Exodus", "Ex");
    data.BibleBooks[BibleBooks.LEVITICUS].RegexParts.Should().BeEquivalentTo();
    data.BibleBooks[BibleBooks.REVELATION].RegexParts.Should().BeEquivalentTo("Openbaring", "Op", "Openbaringen");

    data.LoadingStatus.Should().Be("Laden...");
    data.NoResultStatus.Should().Be("Geen resultaat");
    data.ErrorCodeStatus.Should().Be("Fout code");
    data.ReadMoreStatus.Should().Be("Lees meer");

    data.WordsForVerse.Should().BeEquivalentTo("Vers", "vs");
    data.VerseSelectionWords.Should().BeEquivalentTo("tot", "tot en met");
    data.ChapterVerseSeparator.Should().BeEquivalentTo(":");
    data.VerseVerseSeparator.Should().BeEquivalentTo("-");
  }

  private void AddBookRow(List<ExcelRow> raw, string book, params string[] inputs) {
    AddBookRow(raw, book, inputs.ToList());
  }
  private void AddBookRow(List<ExcelRow> raw, string book, List<string> inputs) {
    raw.AddRegexRow("", book, inputs.FirstOrDefault() ?? "", inputs);
  }
}
