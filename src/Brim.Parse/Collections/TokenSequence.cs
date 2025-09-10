namespace Brim.Parse.Collections;

/// <summary>
/// Fixed-size (≤4) token sequence used for prediction lookahead (explicit length, trailing Any).
/// </summary>
internal readonly struct TokenSequence
{
  readonly RawKind _k1;
  readonly RawKind _k2;
  readonly RawKind _k3;
  readonly RawKind _k4;

  public TokenSequence(RawKind k1) { _k1 = k1; _k2 = RawKind.Any; _k3 = RawKind.Any; _k4 = RawKind.Any; Length = 1; }
  public TokenSequence(RawKind k1, RawKind k2) { _k1 = k1; _k2 = k2; _k3 = RawKind.Any; _k4 = RawKind.Any; Length = 2; }
  public TokenSequence(RawKind k1, RawKind k2, RawKind k3) { _k1 = k1; _k2 = k2; _k3 = k3; _k4 = RawKind.Any; Length = 3; }
  public TokenSequence(RawKind k1, RawKind k2, RawKind k3, RawKind k4) { _k1 = k1; _k2 = k2; _k3 = k3; _k4 = k4; Length = 4; }

  public byte Length { get; }

  public RawKind this[int i] => i switch
  {
    0 => _k1,
    1 => _k2,
    2 => _k3,
    3 => _k4,
    _ => throw new ArgumentOutOfRangeException(nameof(i))
  };

  public static implicit operator TokenSequence(RawKind one) => new(one);
  public static implicit operator TokenSequence((RawKind one, RawKind two) t) => new(t.one, t.two);
  public static implicit operator TokenSequence((RawKind one, RawKind two, RawKind three) t) => new(t.one, t.two, t.three);
  public static implicit operator TokenSequence((RawKind one, RawKind two, RawKind three, RawKind four) t) => new(t.one, t.two, t.three, t.four);
}

