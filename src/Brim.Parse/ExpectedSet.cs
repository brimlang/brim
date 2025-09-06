using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Brim.Parse;

/// <summary>
/// Small fixed-capacity (≤4) set of unique RawTokenKind values used when building
/// "unexpected token" diagnostics. Aligned with Diagnostic encoding (4 expected slots).
/// </summary>
public readonly struct ExpectedSet
{
  readonly RawTokenKind _e0;
  readonly RawTokenKind _e1;
  readonly RawTokenKind _e2;
  readonly RawTokenKind _e3;
  readonly byte _count;

  public byte Count => _count;

  public ExpectedSet Add(RawTokenKind kind)
  {
    if (kind == RawTokenKind.Any) return this;
    // De-dup linear (N ≤ 4)
    if (_count > 0 && _e0 == kind) return this;
    if (_count > 1 && _e1 == kind) return this;
    if (_count > 2 && _e2 == kind) return this;
    if (_count > 3 && _e3 == kind) return this;

    return _count switch
    {
      0 => new ExpectedSet(kind, default, default, default, 1),
      1 => new ExpectedSet(_e0, kind, default, default, 2),
      2 => new ExpectedSet(_e0, _e1, kind, default, 3),
      3 => new ExpectedSet(_e0, _e1, _e2, kind, 4),
      _ => this // already full
    };
  }

  public ReadOnlySpan<RawTokenKind> AsSpan()
  {
    // Use MemoryMarshal to create a span over the first field; fields are laid out sequentially.
    return _count switch
    {
      0 => ReadOnlySpan<RawTokenKind>.Empty,
      1 => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in _e0), 1),
      2 => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in _e0), 2),
      3 => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in _e0), 3),
      _ => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in _e0), 4)
    };
  }

  ExpectedSet(RawTokenKind e0, RawTokenKind e1, RawTokenKind e2, RawTokenKind e3, byte count)
  {
    _e0 = e0; _e1 = e1; _e2 = e2; _e3 = e3; _count = count;
  }
}
