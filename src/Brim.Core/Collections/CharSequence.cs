namespace Brim.Core.Collections;

/// <summary>
/// Immutable, stack-only sequence of up to 3 chars, for efficient operator matching.
/// </summary>
/// <remarks>
/// When created from inputs longer than <see cref="MaxLength"/>, the value is silently
/// truncated to the first <see cref="MaxLength"/> characters (see <c>From</c> factory methods
/// and implicit conversions).
/// </remarks>
public readonly struct CharSequence :
  IEquatable<CharSequence>,
  ISpanFormattable
{
  /// <summary>
  /// Maximum allowed length of a <see cref="CharSequence"/> (always 3).
  /// </summary>
  public const int MaxLength = 3;

  /// <summary>
  /// Represents an empty sequence (equivalent to <c>default</c>).
  /// </summary>
  public static readonly CharSequence Empty = new();

  private readonly char _c0 = char.MaxValue;
  private readonly char _c1 = char.MaxValue;
  private readonly char _c2 = char.MaxValue;

  /// <summary>
  /// Number of characters contained in this sequence (0 to <see cref="MaxLength"/>).
  /// </summary>
  public int Length { get; }

  /// <summary>
  /// Initializes an empty <see cref="CharSequence"/> (Length = 0).
  /// </summary>
  public CharSequence() => Length = 0;

  /// <summary>
  /// Initializes a <see cref="CharSequence"/> with one character.
  /// </summary>
  /// <param name="c0">The first character.</param>
  public CharSequence(char c0)
  {
    _c0 = c0;
    Length = 1;
  }

  /// <summary>
  /// Initializes a <see cref="CharSequence"/> with two characters.
  /// </summary>
  /// <param name="c0">The first character.</param>
  /// <param name="c1">The second character.</param>
  public CharSequence(char c0, char c1)
  {
    _c0 = c0;
    _c1 = c1;
    Length = 2;
  }

  /// <summary>
  /// Initializes a <see cref="CharSequence"/> with three characters.
  /// </summary>
  /// <param name="c0">The first character.</param>
  /// <param name="c1">The second character.</param>
  /// <param name="c2">The third character.</param>
  public CharSequence(char c0, char c1, char c2)
  {
    _c0 = c0;
    _c1 = c1;
    _c2 = c2;
    Length = 3;
  }

  /// <summary>
  /// Creates a <see cref="CharSequence"/> from the first (up to) three characters of the provided span.
  /// </summary>
  /// <param name="span">Source characters. If longer than <see cref="MaxLength"/>, only the first three are used.</param>
  /// <remarks>Inputs longer than <see cref="MaxLength"/> are silently truncated; no exception is thrown.</remarks>
  public static CharSequence From(ReadOnlySpan<char> span)
  {
    return span.Length switch
    {
      0 => default,
      1 => new CharSequence(span[0]),
      2 => new CharSequence(span[0], span[1]),
      _ => new CharSequence(span[0], span[1], span[2]),
    };
  }

  public static implicit operator CharSequence(char c) => new(c);
  public static implicit operator CharSequence(ReadOnlySpan<char> s) => From(s);
  public static implicit operator CharSequence(string s)
  {
    ArgumentNullException.ThrowIfNull(s);
    return From(s);
  }

  public static bool operator ==(CharSequence left, CharSequence right) => left.Equals(right);
  public static bool operator !=(CharSequence left, CharSequence right) => !left.Equals(right);

  /// <summary>
  /// Get the char at the given index.
  /// </summary>
  public char this[int index] {
    get => index switch
    {
      0 => _c0,
      1 => _c1,
      2 => _c2,
      _ => throw new IndexOutOfRangeException(),
    };
  }

#pragma warning disable IDE0046 // Convert to conditional expression
  /// <summary>
  /// Returns true if the first Length chars of input match this sequence.
  /// </summary>
  public bool PrefixMatch(ReadOnlySpan<char> input)
  {
    if (input.Length < Length) return false;
    return Length switch
    {
      0 => true,
      1 => input[0] == _c0,
      2 => input[0] == _c0 && input[1] == _c1,
      _ => input[0] == _c0 && input[1] == _c1 && input[2] == _c2,
    };
  }

  public bool Equals(ReadOnlySpan<char> span)
  {
    if (span.Length != Length) return false;
    return Length switch
    {
      0 => true,
      1 => span[0] == _c0,
      2 => span[0] == _c0 && span[1] == _c1,
      _ => span[0] == _c0 && span[1] == _c1 && span[2] == _c2,
    };
  }
#pragma warning restore IDE0046

  public bool Equals(CharSequence other)
  {
    return Length == other.Length &&
      _c0 == other._c0 &&
      _c1 == other._c1 &&
      _c2 == other._c2;
  }

  public override bool Equals(object? obj)
      => obj is CharSequence other && Equals(other);

  public override int GetHashCode() => HashCode.Combine(_c0, _c1, _c2, Length);
  public override string ToString() => ToString(null, null);

  public bool TryFormat(
      Span<char> destination,
      out int charsWritten,
#pragma warning disable IDE0060 // Remove unused parameter
      ReadOnlySpan<char> format = default,
      IFormatProvider? provider = null)
#pragma warning restore IDE0060 // Remove unused parameter
  {
    if (destination.Length < Length)
    {
      charsWritten = 0;
      return false;
    }

    for (int i = 0; i < Length; i++)
      destination[i] = this[i];
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
