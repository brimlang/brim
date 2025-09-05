using System.Globalization;

namespace Brim.Parse;

public static class Utilities
{
  /// <summary>
  /// Characters not allowed in identifiers.
  /// </summary>
  public static readonly HashSet<char> Reserved = [.. "!@#$%^&*(){}<>;:'\"`~\\/?|.,-_=+"];

  /// <summary>
  /// Determines if a string is a valid identifier.
  /// </summary>
  /// <param name="ident">IdentifierToken to check.</param>
  public static bool IsValidIdentifier(string ident)
  {
    if (string.IsNullOrEmpty(ident))
      return false;

    if (!IsIdentifierStart(ident[0]))
      return false;

    for (int i = 1; i < ident.Length; i++)
    {
      if (!IsIdentifierPart(ident[i]))
        return false;
    }

    return true;
  }

  internal static bool IsIdentifierStart(char c)
  {
    return c == '_'
      || char.IsLetter(c)
      || CharUnicodeInfo.GetUnicodeCategory(c) switch
      {
        UnicodeCategory.LetterNumber => true,
        UnicodeCategory.UppercaseLetter => true,
        UnicodeCategory.LowercaseLetter => true,
        UnicodeCategory.TitlecaseLetter => true,
        UnicodeCategory.ModifierLetter => true,
        _ => false
      };
  }

  internal static bool IsIdentifierPart(char c)
  {
    return IsIdentifierStart(c)
      || char.IsDigit(c)
      || CharUnicodeInfo.GetUnicodeCategory(c) switch
      {
        UnicodeCategory.NonSpacingMark => true,
        UnicodeCategory.SpacingCombiningMark => true,
        UnicodeCategory.DecimalDigitNumber => true,
        UnicodeCategory.ConnectorPunctuation => true,
        UnicodeCategory.Format => true,
        _ => false
      };
  }

  internal static bool IsNonTerminalWhitespace(char c) => char.IsWhiteSpace(c) && c != '\n';
}
