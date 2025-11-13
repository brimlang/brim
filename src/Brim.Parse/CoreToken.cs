using Brim.Lex;

namespace Brim.Parse;

/// <summary>
/// A core token with leading trivia and position information.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
public readonly record struct CoreToken : IToken
{
  internal CoreToken(
    in StructuralArray<TriviaToken> leading,
    TokenKind kind,
    int offset,
    int length,
    int line,
    int column)
  {
    ArgumentOutOfRangeException.ThrowForInvalidToken(kind, k => k.IsValidCoreToken);

    LeadingTrivia = leading;
    TokenKind = kind;
    Offset = offset;
    Length = length;
    Line = line;
    Column = column;
  }

  /// <summary>
  /// The leading trivia associated with this token.
  /// </summary>
  public StructuralArray<TriviaToken> LeadingTrivia { get; }

  public TokenKind TokenKind { get; }
  public int Offset { get; }
  public int Length { get; }
  public int Line { get; }
  public int Column { get; }

  /// <summary>
  /// Indicates whether this token has any leading trivia.
  /// </summary>
  public bool HasLeading => LeadingTrivia.Count > 0;

  /// <summary>
  /// Creates a <see cref="CoreToken"/> from the given leading trivia and lexical token.
  /// </summary>
  /// <param name="leading">The leading trivia tokens.</param>
  /// <param name="lex">The lexical token to convert.</param>
  /// <returns>A new <see cref="CoreToken"/> instance.</returns>
  internal static CoreToken FromLexToken(in StructuralArray<TriviaToken> leading, in LexToken lex) =>
      new(leading, lex.TokenKind, lex.Offset, lex.Length, lex.Line, lex.Column);
}

