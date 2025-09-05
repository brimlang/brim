namespace Brim.Parse;

public static class Utilities
{
  /// <summary>
  /// Determines if a string is a valid identifier.
  /// </summary>
  /// <param name="ident">IdentifierToken to check.</param>
  public static bool IsValidIdentifier(string ident)
  {
    if (string.IsNullOrEmpty(ident))
      return false;

    if (!Chars.IsIdentifierStart(ident[0]))
      return false;

    for (int i = 1; i < ident.Length; i++)
    {
      if (!Chars.IsIdentifierPart(ident[i]))
        return false;
    }

    return true;
  }

  ///<summary>
  /// Gets the RawTokenKind of a token implementing IToken.
  ///</summary>
  internal static RawTokenKind GetRawTokenKind<U>(U token) where U : struct, IToken => token.Kind;
}
