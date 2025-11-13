namespace Brim.Core;

/// <summary>
/// Represents a parsed token with position and kind information.
/// </summary>
public interface IToken
{
  /// <summary>
  /// The kind of token.
  /// </summary>
  TokenKind TokenKind { get; }

  /// <summary>
  /// The offset of the token in the source.
  /// </summary>
  int Offset { get; }

  /// <summary>
  /// The length of the token.
  /// </summary>
  int Length { get; }

  /// <summary>
  /// The line number where the token starts.
  /// </summary>
  int Line { get; }

  /// <summary>
  /// The column number where the token starts.
  /// </summary>
  int Column { get; }
}
