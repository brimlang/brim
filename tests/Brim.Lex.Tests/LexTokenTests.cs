using Brim.Core;

namespace Brim.Lex.Tests;

public class LexTokenTests
{
  [Fact]
  public void ConstructorRejectsInvalidTokenKind()
  {
    Assert.Throws<ArgumentOutOfRangeException>(() =>
      new LexToken(TokenKind.Unitialized, offset: 0, length: 0, line: 1, column: 1));
  }

  [Fact]
  public void ConstructorAllowsValidTokenKind()
  {
    LexToken token = new(TokenKind.Identifier, 2, 3, 4, 5);

    Assert.Equal(TokenKind.Identifier, token.TokenKind);
    Assert.Equal(2, token.Offset);
    Assert.Equal(3, token.Length);
    Assert.Equal(4, token.Line);
    Assert.Equal(5, token.Column);
  }
}
