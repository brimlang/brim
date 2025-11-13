using System.Text;

namespace Brim.Core.Tests;

public class BrimCharsTests
{
  [Theory]
  [InlineData('_')]
  [InlineData('A')]
  [InlineData('\u03B2')]
  public void IsIdentifierStart_AllowsLettersAndUnderscore(char value) =>
    Assert.True(BrimChars.IsIdentifierStart(value));

  [Theory]
  [InlineData('1')]
  [InlineData('_')]
  [InlineData('\u0306')] // combining mark
  public void IsIdentifierContinue_AllowsDigitsAndMarks(char value) =>
    Assert.True(BrimChars.IsIdentifierContinue(value));

  [Fact]
  public void IsIdentifierStart_RejectsDisallowedCharacters()
  {
    Assert.False(BrimChars.IsIdentifierStart('1'));
    Assert.False(BrimChars.IsIdentifierStart('-'));
  }

  [Fact]
  public void IsAllowedWhitespace_IsLimitedSet()
  {
    Assert.True(BrimChars.IsAllowedWhitespace(' '));
    Assert.True(BrimChars.IsAllowedWhitespace('\t'));
    Assert.True(BrimChars.IsAllowedWhitespace('\n'));
    Assert.True(BrimChars.IsAllowedWhitespace('\r'));
    Assert.False(BrimChars.IsAllowedWhitespace('\v'));
  }

  [Fact]
  public void IsTerminator_HandlesNewlinesAndSemicolons()
  {
    Assert.True(BrimChars.IsTerminator('\n'));
    Assert.True(BrimChars.IsTerminator(';'));
    Assert.False(BrimChars.IsTerminator('x'));
  }

  [Fact]
  public void NormalizeIdentifier_ComposesCharacters()
  {
    string decomposed = "o\u0302"; // o + combining circumflex
    string normalized = BrimChars.NormalizeIdentifier(decomposed.AsSpan());
    string expected = decomposed.Normalize(NormalizationForm.FormC);

    Assert.Equal(expected, normalized);
  }

  [Theory]
  [InlineData('0', true)]
  [InlineData('9', true)]
  [InlineData('a', false)]
  [InlineData('٩', false)]
  public void IsDecimalDigit_OnlyAllowsAsciiDigits(char value, bool expected) =>
    Assert.Equal(expected, BrimChars.IsDecimalDigit(value));

  [Theory]
  [InlineData('a', true)]
  [InlineData('Z', true)]
  [InlineData('0', false)]
  [InlineData('β', false)]
  public void IsAsciiLetter_RestrictsToAsciiRange(char value, bool expected) =>
    Assert.Equal(expected, BrimChars.IsAsciiLetter(value));
}
