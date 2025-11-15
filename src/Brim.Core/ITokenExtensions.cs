namespace Brim.Core;

public static class ITokenExtensions
{
  /// <summary>
  /// Gets the value of the token from the given source span.
  /// </summary>
  /// <param name="token">The token.</param>
  /// <param name="sourceSpan">The source span.</param>
  /// <returns>The string value of the token.</returns>
  public static ReadOnlySpan<char> Chars<T>(this T token, ReadOnlySpan<char> sourceSpan) where T : IToken => sourceSpan.Slice(token.Offset, token.Length);

  /// <summary>
  /// Returns a string representation of the token's position.
  /// </summary>
  public static string ToPositionString<T>(this T token) where T : IToken => $"{token.TokenKind}@{token.Line}:{token.Column} [{token.Offset}({token.Length})]";
}
