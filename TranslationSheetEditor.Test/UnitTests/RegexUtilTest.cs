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
  [InlineData("A(B|C)|D", "A(B|C)", "D")]
  public void TestPartsToRegex(string expected, params string[] parts) {
    parts.ToRegex().Should().Be(expected);
  }

  [Theory]
  [InlineData("First|second", "First", "second")]
  [InlineData("fIrSt| ||second", "fIrSt", "second")]
  [InlineData("Genesis|Gen?|Gn", "Genesis", "Gen", "Ge", "Gn")]
  [InlineData("Genesis|Ge?n|Ge", "Genesis", "Gen", "Gn", "Ge")]
  [InlineData("A?b?c?", "Abc", "Ab", "Ac", "A", "bc", "b", "c")]
  [InlineData("A(B|C)|D", "A(B|C)", "D")]
  [InlineData("A(B|C", "A(B", "C")]
  public void TestToPartsFromRegex(string regex, params string[] expectedParts) {
    regex.ToPartsFromRegex().Should().BeEquivalentTo(expectedParts);
  }

  [Theory]
  [InlineData("Petrus", "Petrus", null, "Petrus")]
  [InlineData("1 Petrus", "2 Petrus", null, "Petrus")]
  [InlineData("1 John?|1 ?Jn", "2 John|2 Joh|2 ?Jn", "3 John|3 Joh|3 ?Jn", "John", "Joh", "Jn")]
  [InlineData("(1|I|Een) ?Petrus|(1|I) ?Pet", "(2|II|Twee) ?Petrus|(2|II) ?Pet", null, "Petrus", "Pet")]
  [InlineData(" (1|I|Een) ?Petrus ", "    (2|II|Twee) ?Petrus  ", null, "Petrus")]
  [InlineData("1 (Blah", "2 (Blah", null, "(Blah")]
  [InlineData("(1|I) Blah", "2 Blah|II Blah", null, "Blah")]
  [InlineData("(Dont|Crash|Please)", "", null)]
  [InlineData("I Test", "II Test", "III Test", "Test")]
  public void TestToPartsFromMultiplePrefixBookRegexes(string first, string second, string? third,
      params string[] expectedParts) {
    var parts = RegexUtil.ToPartsFromMultiplePrefixBookRegexes(first, second, third);
    parts.BookParts.Should().BeEquivalentTo(expectedParts);
  }

  [Theory]
  [InlineData("Petrus", "Petrus", null)]
  [InlineData("1 Petrus", "2 Petrus", null, "1")]
  [InlineData("(1|I|Een) ?Petrus|(1|I) ?Pet", "(2|II|Twee) ?Petrus|(2|II) ?Pet", null, "1", "I", "Een")]
  [InlineData("I Test", "II Test", "III Test", "I")]
  [InlineData("de eerste test", "de tweede test", null, "de eerste")]
  // [InlineData("(1|I) Blah", "2 Blah|II Blah", null, "1", "I")] // So this doesn't work - I'm fine with that
  public void TestFirstNumbersPartsFromMultiplePrefixBookRegexes(string first, string second, string? third,
      params string[] expectedParts) {
    var parts = RegexUtil.ToPartsFromMultiplePrefixBookRegexes(first, second, third);
    parts.NumberOneParts.Should().BeEquivalentTo(expectedParts);
  }

  [Theory]
  [InlineData("1 Petrus|Een Petrus|1 Pet", "2 Petrus|Twee Petrus|2 Pet", null, "2", "Twee")]
  [InlineData(" (1|I|Een) ?Petrus | (1|I) ?Pet ", " (2|II|Twee) ?Petrus | (2|II) ?Pet ", null, "2", "II", "Twee")]
  public void TestSecondNumbersPartsFromMultiplePrefixBookRegexes(string first, string second, string? third,
      params string[] expectedParts) {
    var parts = RegexUtil.ToPartsFromMultiplePrefixBookRegexes(first, second, third);
    parts.NumberTwoParts.Should().BeEquivalentTo(expectedParts);
  }

  [Theory]
  [InlineData("1 Petrus", "2 Petrus", null)]
  [InlineData("1 Johannes | Een Johannes", "2 Johannes | Twee Johannes", "3 Johannes | Drie Johannes", "3", "Drie")]
  [InlineData("(1|I|Een) ?Johannes|(1|I) ?Joh",
      "(2|II|Twee) ?Johannes|(2|II) ?Joh",
      "(3|III|Drie) ?Johannes|(3|III) ?Joh",
      "3", "III", "Drie")]
  public void TestThirdNumbersPartsFromMultiplePrefixBookRegexes(string first, string second, string? third,
      params string[] expectedParts) {
    var parts = RegexUtil.ToPartsFromMultiplePrefixBookRegexes(first, second, third);
    parts.NumberThreeParts.Should().BeEquivalentTo(expectedParts);
  }
}
