using FluentAssertions;
using TranslationSheetEditor.Model;
using TranslationSheetEditor.Test.TestUtils;
using TranslationSheetEditor.Utils;
using Xunit;

namespace TranslationSheetEditor.Test.UnitTests;

public class ValidationTest {
  [Fact]
  public void TestValidationForMissingValues() {
    var data = new TranslationData();
    data.FirstTimeInit();

    string errorText = ValidationUtil.ValidateTranslationData(data);

    string expectedError = """
      - Missing values at 'Initial book names'.
      - Missing values at 'Other translations'.
      - Missing values at 'Prefix numbers'.
      """;
    foreach (string book in BibleBooks.ALL_BOOKS) {
      expectedError += $"\n- Missing values for book '{book}'.";
    }
    expectedError = expectedError.ReplaceLineEndings().Trim();
    errorText = errorText.ReplaceLineEndings().Trim();
    errorText.Should().Be(expectedError);
  }

  [Fact]
  public void TestNoDuplicates() {
    var data = TranslationDataTestUtil.BuildFullEnglishExample(); // Note that both John books have the same value, but that's permitted
    string errorText = ValidationUtil.ValidateTranslationData(data);
    errorText.Should().BeNullOrEmpty();
  }

  [Fact]
  public void TestDuplicates() {
    var data = TranslationDataTestUtil.BuildFullEnglishExample();
    data.BibleBooks[BibleBooks.GENESIS].RegexParts.Add(BibleBooks.JOHN);

    string errorText = ValidationUtil.ValidateTranslationData(data);

    errorText = errorText.ReplaceLineEndings().Trim();
    errorText.Should().Be("""
      - Book alternative 'John' is used multiple times; Genesis, John.
      - Book alternative 'John' is used multiple times; Genesis, John (letter).
      """);
  }
}
