using FluentAssertions;
using TranslationSheetEditor.Model;
using TranslationSheetEditor.Test.UnitTests;
using TranslationSheetEditor.Utils;
using Xunit;

namespace TranslationSheetEditor.Test.IntegrationTests;

public class ExcelUtilIntegrationTest {
  [Fact]
  public void ExportAndImportExcelFile() {
    var originalData = ExcelUtilTest.BuildDutchTranslationDataExample();
    var filePath =  new Uri(Path.Combine(Path.GetTempPath(), "integration-test-dutch.xlsx"));

    ExcelUtil.Export(originalData, filePath);
    var data = ExcelUtil.Import(filePath, out string? errors);

    errors.Should().BeNull();

    data.Language.Should().Be("Nederlands");

    data.BibleBooks[BibleBooks.GENESIS].TranslatedName.Should().Be("Genesis");
    data.BibleBooks[BibleBooks.GENESIS].RegexParts.Should().BeEquivalentTo("Genesis", "Gen");
    data.BibleBooks[BibleBooks.EXODUS].TranslatedName.Should().Be("Exodus");
    data.BibleBooks[BibleBooks.REVELATION].TranslatedName.Should().Be("Openbaring");
    data.BibleBooks[BibleBooks.REVELATION].RegexParts.Should().BeEquivalentTo("Openbaring", "Op", "Openbaringen");

    data.LoadingStatus.Should().Be("Laden...");
    data.NoResultStatus.Should().Be("Geen resultaat");
    data.ErrorCodeStatus.Should().Be("Fout code");
    data.ReadMoreStatus.Should().Be("Lees meer");

    data.WordsForVerse.Should().BeEquivalentTo("Vers", "vs");
    data.VerseSelectionWords.Should().BeEquivalentTo("tot", "tot en met", "t/m");
    data.ChapterVerseSeparator.Should().BeEquivalentTo(":");
    data.VerseVerseSeparator.Should().BeEquivalentTo("-");

    File.Delete(filePath.AbsolutePath);
  }
}
