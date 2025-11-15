namespace Brim.Core.Tests;

public class TokenExtensionsTests
{
  [Fact]
  public void Chars_ReturnsSlice()
  {
    FakeToken token = new(TokenKind.Identifier, 2, 3, 5, 1);
    ReadOnlySpan<char> source = "identifier".AsSpan();

    string value = token.Chars(source).ToString();

    Assert.Equal("ent", value);
  }

  [Fact]
  public void ToString_ShowsKindLocationAndSpan()
  {
    FakeToken token = new(TokenKind.IntegerLiteral, 10, 2, 4, 3);
    string formatted = ITokenExtensions.ToPositionString(token);

    Assert.Equal("IntegerLiteral@4:3 [10(2)]", formatted);
  }
}

file sealed record FakeToken(TokenKind TokenKind, int Offset, int Length, int Line, int Column) : IToken;
