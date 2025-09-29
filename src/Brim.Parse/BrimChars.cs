using System.Globalization;
using System.Text;

namespace Brim.Parse;

static class BrimChars
{
  /// <summary>
  /// End Of Buffer marker for stream processing.
  /// </summary>
  public const char EOB = '\uFFFF'; // Private use character
  public const char NewLine = '\n';

  /// <summary>
  /// Characters that terminate statements or declarations.
  /// </summary>
  public static bool IsTerminator(char c) => c == '\n';

  /// <summary>
  /// Whitespace characters allowed by the Unicode spec - only specific codepoints.
  /// </summary>
  public static bool IsAllowedWhitespace(char c) =>
    c is ' ' or '\t' or '\n' or '\r'; // space, tab, LF, CR

  /// <summary>
  /// Checks if a char can start an identifier using XID_Start-like rules.
  /// Per Unicode spec: XID_Start + underscore.
  /// </summary>
  public static bool IsIdentifierStart(char c)
  {
    if (c == '_') return true;

    // Approximate XID_Start using UnicodeCategory
    return CharUnicodeInfo.GetUnicodeCategory(c) switch
    {
      UnicodeCategory.UppercaseLetter or
      UnicodeCategory.LowercaseLetter or
      UnicodeCategory.TitlecaseLetter or
      UnicodeCategory.ModifierLetter or
      UnicodeCategory.OtherLetter or
      UnicodeCategory.LetterNumber => true,
      _ => false
    };
  }

  /// <summary>
  /// Checks if a char can continue an identifier using XID_Continue-like rules.
  /// </summary>
  public static bool IsIdentifierContinue(char c)
  {
    if (IsIdentifierStart(c)) return true;

    // Approximate XID_Continue with additional categories
    return CharUnicodeInfo.GetUnicodeCategory(c) switch
    {
      UnicodeCategory.DecimalDigitNumber or
      UnicodeCategory.ConnectorPunctuation or
      UnicodeCategory.NonSpacingMark or
      UnicodeCategory.SpacingCombiningMark or
      UnicodeCategory.Format => true,
      _ => false
    };
  }

  /// <summary>
  /// Check if a char is a decimal digit (0-9).
  /// </summary>
  public static bool IsDecimalDigit(char c) => c is >= '0' and <= '9';

  /// <summary>
  /// Check if a char is an ASCII letter.
  /// </summary>
  public static bool IsAsciiLetter(char c) =>
    c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';

  /// <summary>
  /// Normalizes an identifier to NFC form per Unicode spec requirement.
  /// </summary>
  public static string NormalizeIdentifier(ReadOnlySpan<char> identifier) =>
    new string(identifier).Normalize(NormalizationForm.FormC);
}
