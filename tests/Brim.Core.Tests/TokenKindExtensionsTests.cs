namespace Brim.Core.Tests;

public class TokenKindExtensionsTests
{
  [Fact]
  public void ClassificationFlags_WorkForIdentifiers()
  {
    TokenKind identifier = TokenKind.Identifier;

    Assert.True(identifier.IsValidLexToken);
    Assert.True(identifier.IsValidCoreToken);
    Assert.False(identifier.IsGlyph);
    Assert.False(identifier.IsTrivia);
    Assert.False(identifier.IsLiteral);
    Assert.False(identifier.IsSentinel);
  }

  [Fact]
  public void GlyphsAndTriviaAreDetected()
  {
    Assert.True(TokenKind.LParen.IsGlyph);
    Assert.False(TokenKind.LParen.IsTrivia);
    Assert.True(TokenKind.CommentTrivia.IsTrivia);
    Assert.False(TokenKind.CommentTrivia.IsGlyph);
    Assert.True(TokenKind.IntegerLiteral.IsLiteral);
  }

  [Fact]
  public void ThrowForInvalidToken_UsesPredicate()
  {
    TokenKind invalid = TokenKind._SentinelGlyphs;

    Assert.Throws<ArgumentOutOfRangeException>(() =>
      ArgumentOutOfRangeException.ThrowForInvalidToken(invalid, static k => k.IsValidLexToken));

    TokenKind valid = TokenKind.Less;
    Exception? ex = Record.Exception(() =>
      ArgumentOutOfRangeException.ThrowForInvalidToken(valid, static k => k.IsValidLexToken));

    Assert.Null(ex);
  }

  [Fact]
  public void SentinelAndUnusedKindsAreExcludedFromValidSets()
  {
    TokenKind unused = (TokenKind)((int)TokenKind._SentinelGlyphs - 1);

    Assert.True(TokenKind._SentinelGlyphs.IsSentinel);
    Assert.True(unused.IsUnused);
    Assert.False(unused.IsValidCoreToken);
    Assert.True(TokenKind.Eob.IsValidCoreToken);
  }
}
