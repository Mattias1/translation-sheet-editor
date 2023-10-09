using FluentAssertions;
using TranslationSheetEditor.Utils;
using Xunit;

namespace TranslationSheetEditor.Test.UnitTests;

public class RegexUtilTest {
  [Theory]
  [InlineData("First|second", "First", "second")]
  [InlineData("First|second", "First", " ", "","second")]
  public void TestToRegexParts(string expected, params string[] parts) {
    parts.ToList().ToRegex().Should().Be(expected);
  }

  [Theory]
  [InlineData("First|second", "First", "second")]
  [InlineData("fIrSt| ||second", "fIrSt", "second")]
  public void TestToPartsFromRegex(string regex, params string[] expectedParts) {
    regex.ToPartsFromRegex().ToList().Should().BeEquivalentTo(expectedParts);
  }
}
