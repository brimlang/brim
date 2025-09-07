using System.Globalization;

namespace Brim.Parse;

static class BrimChars
{
  /// <summary>
  /// Characters not allowed in identifiers.
  /// </summary>
  public static readonly HashSet<char> Reserved = [.. "!@#$%^&*(){}<>[];:'\"`~\\/?|.,-_=+"];

  /// <summary>
  /// End Of Buffer character, used to simplify lexing logic.
  /// </summary>
  public const char EOB = char.MaxValue;
  public const char Null = '\0';
  public const char NewLine = '\n';

  /// <summary>
  /// Characters that terminate statements or declarations.
  /// </summary>
  public static bool IsTerminator(char c) => c is NewLine or ';';

  /// <summary>
  /// Whitespace characters excluding terminators.
  /// </summary>
  public static bool IsNonTerminalWhitespace(char c) => char.IsWhiteSpace(c) && !IsTerminator(c);

  public static bool IsIdentifierStart(char c)
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

  public static bool IsIdentifierPart(char c)
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
}
