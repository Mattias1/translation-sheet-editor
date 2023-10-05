using FluentAssertions;
using TranslationSheetEditor.Utils;
using Xunit;

namespace TranslationSheetEditor.Test;

public class RegexUtilTest {
  [Theory]
  [InlineData("First|second", "First", "second")]
  public void TestToRegexParts(string expected, params string[] parts) {
    parts.ToList().ToRegex().Should().Be(expected);
  }
}
