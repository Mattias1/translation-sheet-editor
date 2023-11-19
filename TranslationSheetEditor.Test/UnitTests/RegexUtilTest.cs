using FluentAssertions;
using TranslationSheetEditor.Utils;
using Xunit;

namespace TranslationSheetEditor.Test.UnitTests;

public class RegexUtilTest {
  [Theory]
  [InlineData("First|second", "First", "second")]
  [InlineData("fIrSt|second", "fIrSt", " ", "", "second")]
  [InlineData("Genesis|Gen?|Gn", "Genesis", "Gen", "Ge", "Gn")]
  [InlineData("Genesis|Ge?n|Ge", "Genesis", "Gen", "Gn", "Ge")]
  public void TestPartsToRegex(string expected, params string[] parts) {
    parts.ToRegex().Should().Be(expected);
  }

  [Theory]
  [InlineData("First|second", "First", "second")]
  [InlineData("fIrSt| ||second", "fIrSt", "second")]
  [InlineData("Genesis|Gen?|Gn", "Genesis", "Gen", "Ge", "Gn")]
  [InlineData("Genesis|Ge?n|Ge", "Genesis", "Gen", "Gn", "Ge")]
  [InlineData("A?b?c?", "Abc", "Ab", "Ac", "A", "bc", "b", "c")]
  public void TestToPartsFromRegex(string regex, params string[] expectedParts) {
    regex.ToPartsFromRegex().Should().BeEquivalentTo(expectedParts);
  }
}
