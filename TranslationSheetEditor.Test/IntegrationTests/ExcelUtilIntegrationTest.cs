using FluentAssertions;
using TranslationSheetEditor.Model;
using TranslationSheetEditor.Test.TestUtils;
using TranslationSheetEditor.Utils;
using Xunit;

namespace TranslationSheetEditor.Test.IntegrationTests;

public class ExcelUtilIntegrationTest {
  [Fact]
  public void ExportAndImportExcelFile() {
    var originalData = TranslationDataTestUtil.BuildPartialDutchExample();
    var filePath =  new Uri(Path.Combine(Path.GetTempPath(), "integration test%20dutch.xlsx"));

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
    data.NotFoundStatus.Should().Be("Niet gevonden");

    data.WordsForVerse.Should().BeEquivalentTo("Vers", "vs");
    data.VerseSelectionWords.Should().BeEquivalentTo("tot", "tot en met", "t/m");
    data.ChapterVerseSeparator.Should().BeEquivalentTo(":");
    data.VerseVerseSeparator.Should().BeEquivalentTo("-");
    data.WordsForChapter.Should().BeEquivalentTo("hoofdstuk", "h");
    data.WordsOrCharactersForListingReferences.Should().BeEquivalentTo("en", "of", "en ook");

    File.Delete(ExcelUtil.ParseAbsolutePath(filePath));
  }
}
