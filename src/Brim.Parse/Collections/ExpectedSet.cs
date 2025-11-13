using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Brim.Parse.Collections;

/// <summary>
/// Small fixed-capacity (≤4) set of unique RawKind values used when building
/// "unexpected token" diagnostics. Aligned with Diagnostic encoding (4 expected slots).
/// </summary>
public readonly struct ExpectedSet
{
  readonly TokenKind _e0;
  readonly TokenKind _e1;
  readonly TokenKind _e2;
  readonly TokenKind _e3;

  public byte Count { get; }

  public ExpectedSet Add(TokenKind kind)
  {
    if (kind == TokenKind.Any) return this;
    // De-dup linear (N ≤ 4)
    return Count switch
    {
      > 0 when _e0 == kind => this,
      > 1 when _e1 == kind => this,
      > 2 when _e2 == kind => this,
      > 3 when _e3 == kind => this,
      _ => Count switch
      {
        0 => new ExpectedSet(kind, default, default, default, 1),
        1 => new ExpectedSet(_e0, kind, default, default, 2),
        2 => new ExpectedSet(_e0, _e1, kind, default, 3),
        3 => new ExpectedSet(_e0, _e1, _e2, kind, 4),
        _ => this // already full
      },
    };
  }

  public ReadOnlySpan<TokenKind> AsSpan()
  {
    // Use MemoryMarshal to create a span over the first field; fields are laid out sequentially.
    return Count switch
    {
      0 => [],
      1 => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in _e0), 1),
      2 => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in _e0), 2),
      3 => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in _e0), 3),
      _ => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in _e0), 4)
    };
  }

  ExpectedSet(TokenKind e0, TokenKind e1, TokenKind e2, TokenKind e3, byte count)
  {
    _e0 = e0; _e1 = e1; _e2 = e2; _e3 = e3; Count = count;
  }
}
