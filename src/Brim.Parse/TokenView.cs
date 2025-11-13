using Brim.Lex;

namespace Brim.Parse;

public readonly ref struct TokenView
{
  readonly LexToken _core;

  public readonly TokenKind Kind => _core.TokenKind;
  public readonly int Offset => _core.Offset;
  public readonly int Line => _core.Line;
  public readonly int Column => _core.Column;
  public readonly int Length => _core.Length;

  TokenView(LexToken core) => _core = core;

  public static implicit operator TokenView(in LexToken core) => new(core);
}

