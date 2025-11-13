namespace Brim.Parse;

public static class Tokens
{
  public static bool IsEob<T>(in T token) where T : IToken => token.TokenKind == TokenKind.Eob;
  public static bool IsDefault<T>(in T token) where T : IToken => token.TokenKind == TokenKind.Unitialized;
  public static bool IsError<T>(in T token) where T : IToken => token.TokenKind == TokenKind.Error;
}

