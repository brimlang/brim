using Brim.Lex;

namespace Brim.Parse;

public readonly record struct TriviaToken : IToken
{
  internal TriviaToken(TokenKind kind, int offset, int length, int line, int column)
  {
    ArgumentOutOfRangeException.ThrowForInvalidToken(kind, k => k.IsTrivia);

    TokenKind = kind;
    Offset = offset;
    Length = length;
    Line = line;
    Column = column;
  }

  internal static TriviaToken FromLexToken(LexToken raw)
    => new(raw.TokenKind, raw.Offset, raw.Length, raw.Line, raw.Column);

  public TokenKind TokenKind { get; }
  public int Offset { get; }
  public int Length { get; }
  public int Line { get; }
  public int Column { get; }
}

