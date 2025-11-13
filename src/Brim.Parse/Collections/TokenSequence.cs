namespace Brim.Parse.Collections;

/// <summary>
/// Fixed-size (≤4) token sequence used for prediction lookahead (explicit length, trailing Any).
/// </summary>
internal readonly struct TokenSequence
{
  readonly TokenKind _k1;
  readonly TokenKind _k2;
  readonly TokenKind _k3;
  readonly TokenKind _k4;

  public TokenSequence(TokenKind k1) { _k1 = k1; _k2 = TokenKind.Any; _k3 = TokenKind.Any; _k4 = TokenKind.Any; Length = 1; }
  public TokenSequence(TokenKind k1, TokenKind k2) { _k1 = k1; _k2 = k2; _k3 = TokenKind.Any; _k4 = TokenKind.Any; Length = 2; }
  public TokenSequence(TokenKind k1, TokenKind k2, TokenKind k3) { _k1 = k1; _k2 = k2; _k3 = k3; _k4 = TokenKind.Any; Length = 3; }
  public TokenSequence(TokenKind k1, TokenKind k2, TokenKind k3, TokenKind k4) { _k1 = k1; _k2 = k2; _k3 = k3; _k4 = k4; Length = 4; }

  public byte Length { get; }

  public TokenKind this[int i] => i switch
  {
    0 => _k1,
    1 => _k2,
    2 => _k3,
    3 => _k4,
    _ => throw new ArgumentOutOfRangeException(nameof(i))
  };

  public static implicit operator TokenSequence(TokenKind one) => new(one);
  public static implicit operator TokenSequence((TokenKind one, TokenKind two) t) => new(t.one, t.two);
  public static implicit operator TokenSequence((TokenKind one, TokenKind two, TokenKind three) t) => new(t.one, t.two, t.three);
  public static implicit operator TokenSequence((TokenKind one, TokenKind two, TokenKind three, TokenKind four) t) => new(t.one, t.two, t.three, t.four);
}

