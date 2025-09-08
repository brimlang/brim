using Brim.Parse.Green;

namespace Brim.Parse;

public readonly ref struct TokenView
{
  readonly RawToken _core;

  public readonly RawKind Kind => _core.Kind;
  public readonly int Offset => _core.Offset;
  public readonly int Line => _core.Line;
  public readonly int Column => _core.Column;
  public readonly int Length => _core.Length;

  TokenView(RawToken core) => _core = core;

  public static implicit operator TokenView(in RawToken core) => new(core);
  public static implicit operator TokenView(in SignificantToken token) => new(token.CoreToken);
  public static implicit operator TokenView(in GreenToken token) => new(token.Token);
}

