namespace Brim.Parse.Collections;

/// <summary>
/// Immutable, stack-only sequence of up to 3 UTF-8 bytes, for efficient operator matching.
/// Designed specifically for ASCII operators in the lexer.
/// </summary>
/// <remarks>
/// When created from inputs longer than <see cref="MaxLength"/>, the value is silently
/// truncated to the first <see cref="MaxLength"/> bytes (see <c>From</c> factory methods
/// and implicit conversions).
/// </remarks>
public readonly struct ByteSequence :
  IEquatable<ByteSequence>,
  ISpanFormattable
{
  /// <summary>
  /// Maximum allowed length of a <see cref="ByteSequence"/> (always 3).
  /// </summary>
  public const int MaxLength = 3;

  /// <summary>
  /// Represents an empty sequence (equivalent to <c>default</c>).
  /// </summary>
  public static readonly ByteSequence Empty = new();

  private readonly byte _b0 = byte.MaxValue;
  private readonly byte _b1 = byte.MaxValue;
  private readonly byte _b2 = byte.MaxValue;

  /// <summary>
  /// Number of bytes contained in this sequence (0 to <see cref="MaxLength"/>).
  /// </summary>
  public int Length { get; }

  /// <summary>
  /// Initializes an empty <see cref="ByteSequence"/> (Length = 0).
  /// </summary>
  public ByteSequence() => Length = 0;

  /// <summary>
  /// Initializes a <see cref="ByteSequence"/> with one byte.
  /// </summary>
  /// <param name="b0">The first byte.</param>
  public ByteSequence(byte b0)
  {
    _b0 = b0;
    Length = 1;
  }

  /// <summary>
  /// Initializes a <see cref="ByteSequence"/> with two bytes.
  /// </summary>
  /// <param name="b0">The first byte.</param>
  /// <param name="b1">The second byte.</param>
  public ByteSequence(byte b0, byte b1)
  {
    _b0 = b0;
    _b1 = b1;
    Length = 2;
  }

  /// <summary>
  /// Initializes a <see cref="ByteSequence"/> with three bytes.
  /// </summary>
  /// <param name="b0">The first byte.</param>
  /// <param name="b1">The second byte.</param>
  /// <param name="b2">The third byte.</param>
  public ByteSequence(byte b0, byte b1, byte b2)
  {
    _b0 = b0;
    _b1 = b1;
    _b2 = b2;
    Length = 3;
  }

  /// <summary>
  /// Creates a <see cref="ByteSequence"/> from the first (up to) three bytes of the provided span.
  /// </summary>
  /// <param name="span">Source bytes. If longer than <see cref="MaxLength"/>, only the first three are used.</param>
  /// <remarks>Inputs longer than <see cref="MaxLength"/> are silently truncated; no exception is thrown.</remarks>
  public static ByteSequence From(ReadOnlySpan<byte> span)
  {
    return span.Length switch
    {
      0 => default,
      1 => new ByteSequence(span[0]),
      2 => new ByteSequence(span[0], span[1]),
      _ => new ByteSequence(span[0], span[1], span[2]),
    };
  }

  public static implicit operator ByteSequence(byte b) => new(b);
  public static implicit operator ByteSequence(ReadOnlySpan<byte> s) => From(s);

  public static bool operator ==(ByteSequence left, ByteSequence right) => left.Equals(right);
  public static bool operator !=(ByteSequence left, ByteSequence right) => !left.Equals(right);

  /// <summary>
  /// Get the byte at the given index.
  /// </summary>
  public byte this[int index] {
    get => index switch
    {
      0 => _b0,
      1 => _b1,
      2 => _b2,
      _ => throw new IndexOutOfRangeException(),
    };
  }

#pragma warning disable IDE0046 // Convert to conditional expression
  /// <summary>
  /// Returns true if the first Length bytes of input match this sequence.
  /// </summary>
  public bool PrefixMatch(ReadOnlySpan<byte> input)
  {
    if (input.Length < Length) return false;
    return Length switch
    {
      0 => true,
      1 => input[0] == _b0,
      2 => input[0] == _b0 && input[1] == _b1,
      _ => input[0] == _b0 && input[1] == _b1 && input[2] == _b2,
    };
  }

  public bool Equals(ReadOnlySpan<byte> span)
  {
    if (span.Length != Length) return false;
    return Length switch
    {
      0 => true,
      1 => span[0] == _b0,
      2 => span[0] == _b0 && span[1] == _b1,
      _ => span[0] == _b0 && span[1] == _b1 && span[2] == _b2,
    };
  }
#pragma warning restore IDE0046

  public bool Equals(ByteSequence other)
  {
    return Length == other.Length &&
      _b0 == other._b0 &&
      _b1 == other._b1 &&
      _b2 == other._b2;
  }

  public override bool Equals(object? obj)
      => obj is ByteSequence other && Equals(other);

  public override int GetHashCode() => HashCode.Combine(_b0, _b1, _b2, Length);
  public override string ToString() => ToString(null, null);

  public bool TryFormat(
      Span<char> destination,
      out int charsWritten,
      ReadOnlySpan<char> format = default,
      IFormatProvider? provider = null)
  {
    if (destination.Length < Length)
    {
      charsWritten = 0;
      return false;
    }

    // Convert bytes to chars for display (ASCII only)
    for (int i = 0; i < Length; i++)
      destination[i] = (char)this[i];
    charsWritten = Length;
    return true;
  }

  public string ToString(string? format, IFormatProvider? formatProvider)
  {
    Span<char> buffer = stackalloc char[MaxLength];
    _ = TryFormat(buffer, out int written, format, formatProvider);
    return new string(buffer[..written]);
  }
}
