using System.Runtime.CompilerServices;

namespace Brim.Core;

/// <summary>
/// Extensions for <see cref="TokenKind"/>.
/// </summary>
public static class TokenKindExtensions
{
  extension(ArgumentOutOfRangeException)
  {
    public static void ThrowForInvalidToken(TokenKind kind, Predicate<TokenKind> predicate, [CallerArgumentExpression(nameof(kind))] string? paramName = default)
    {
      if (!predicate(kind))
        throw new ArgumentOutOfRangeException(paramName, kind, $"Invalid token kind: {kind}");
    }
  }

  extension(TokenKind kind)
  {
    public bool IsSpecial => kind is > TokenKind._SentinelSpecial and < TokenKind.Unitialized;
    public bool IsTrivia => kind is > TokenKind._SentinelTrivia and < TokenKind._SentinelCore;
    public bool IsGlyph => kind is > TokenKind._SentinelGlyphs and < TokenKind._SentinelLiteral;
    public bool IsLiteral => kind is > TokenKind._SentinelLiteral and < TokenKind._SentinelSynthetic;
    public bool IsUnused => Enum.IsDefined(kind) is false || kind.IsSentinel;

    public bool IsSentinel => kind
      is TokenKind._SentinelSpecial
      or TokenKind.Unitialized
      or TokenKind._SentinelTrivia
      or TokenKind._SentinelCore
      or TokenKind._SentinelGlyphs
      or TokenKind._SentinelLiteral
      or TokenKind._SentinelSynthetic;

    public bool IsValidLexToken => kind is > TokenKind._SentinelTrivia and <= TokenKind.Eob && !kind.IsUnused;
    public bool IsValidCoreToken => kind is > TokenKind._SentinelCore and <= TokenKind.Eob && !kind.IsUnused;
  }
}

