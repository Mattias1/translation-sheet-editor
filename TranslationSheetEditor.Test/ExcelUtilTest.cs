using FluentAssertions;
using TranslationSheetEditor.Model;
using TranslationSheetEditor.Utils;
using Xunit;

namespace TranslationSheetEditor.Test;

public class ExcelUtilTest {
  [Fact]
  public void TestBuildingATranslationDataArray() {
    var data = new TranslationData();
    data.FirstTimeInit();

    data.BibleBooks.BibleBookData[BibleBooks.GENESIS].TranslatedName = "Genesis";
    data.BibleBooks.BibleBookData[BibleBooks.GENESIS].RegexParts.AddRange(new[] { "Genesis", "Gen" });
    data.BibleBooks.BibleBookData[BibleBooks.EXODUS].TranslatedName = "Exodus";

    data.LoadingStatus = "Loading...";
    data.NoResultStatus = "No result";
    data.ErrorCodeStatus = "Error code";
    data.ReadMoreStatus = "Read more";

    data.WordsForVerse.AddRange(new[] { "Verse", "vs" });
    data.VerseSelectionWords.AddRange(new[] { "to", "till", "until" });
    data.ChapterVerseSeparator.Add(":");
    data.VerseVerseSeparator.Add("-");

    var excelData = ExcelUtil.BuildTranslationDataExcelArray(data);

    excelData.Length.Should().Be(2 + 66 + 2 + 4 + 2 + 4);

    excelData[0].IsHeader.Should().BeTrue();
    excelData[1].IsHeader.Should().BeTrue();
    excelData[2].IsHeader.Should().BeFalse();
    excelData[2].RawData.Should().BeEquivalentTo("", "Genesis", "", "Genesis", "Genesis|Gen", "", "Genesis", "Gen");
    excelData[3].RawData.Should().BeEquivalentTo("", "Exodus", "", "Exodus", "", "");

    excelData[69].IsHeader.Should().BeTrue();
    excelData[70].IsHeader.Should().BeFalse();
    excelData[70][3].Should().Be("Loading...");
    excelData[71][3].Should().Be("No result");
    excelData[72][3].Should().Be("Error code");
    excelData[73][3].Should().Be("Read more");

    excelData[75].IsHeader.Should().BeTrue();
    excelData[76].IsHeader.Should().BeFalse();
    excelData[76][3].Should().Be("Verse|vs");
    excelData[77][3].Should().Be("to|till|until");
    excelData[78][3].Should().Be(":");
    excelData[79][3].Should().Be("-");
  }
}
