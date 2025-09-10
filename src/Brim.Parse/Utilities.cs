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

    if (!BrimChars.IsIdentifierStart(ident[0]))
      return false;

    for (int i = 1; i < ident.Length; i++)
    {
      if (!BrimChars.IsIdentifierPart(ident[i]))
        return false;
    }

    return true;
  }
}
