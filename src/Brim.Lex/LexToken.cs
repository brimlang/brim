namespace Brim.Lex;

/// <summary>
/// A token produced by the lexer.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
public readonly record struct LexToken : IToken
{
  /// <param name="Kind">The kind of token.</param>
  /// <param name="Offset">The offset of the token in the input buffer.</param>
  /// <param name="Length">The length of the token in the input buffer.</param>
  /// <param name="Line">The line number of the token (1-based).</param>
  /// <param name="Column">The column number of the token (1-based).</param>
  public LexToken(
    TokenKind kind,
    int offset,
    int length,
    int line,
    int column)
  {
    ArgumentOutOfRangeException.ThrowForInvalidToken(kind, k => k.IsValidLexToken);

    TokenKind = kind;
    Offset = offset;
    Length = length;
    Line = line;
    Column = column;
  }

  public TokenKind TokenKind { get; }
  public int Offset { get; }
  public int Length { get; }
  public int Line { get; }
  public int Column { get; }
}
