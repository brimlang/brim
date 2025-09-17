using System.Runtime.CompilerServices;

namespace Brim.Parse;

/// <summary>
/// Lightweight optional value.
/// </summary>
/// <remarks>
/// - Default value represents <c>None</c>.
/// - Construct with a value for <c>Some</c>.
/// </remarks>
[DebuggerDisplay("{_hasValue ? $\"Some({_value})\" : \"None\"}")]
internal readonly struct Opt<T> : IEquatable<Opt<T>>
{
  readonly bool _hasValue;
  readonly T? _value;

  /// <summary>
  /// Creates an option in the Some state containing <paramref name="value"/>.
  /// </summary>
  /// <param name="value">The value to wrap.</param>
  [SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Default _hasValue must be false")]
  public Opt(in T value)
  {
    _value = value;
    _hasValue = true;
  }

  /// <summary>
  /// An empty option value representing None.
  /// </summary>
  public static Opt<T> None { get; } = default;

  /// <summary>
  /// Implicitly converts a value into an option in the Some state.
  /// </summary>
  /// <param name="value">The value to wrap.</param>
  public static implicit operator Opt<T>(in T value) => new(value);

  /// <summary>
  /// Explicitly extracts the contained value; throws if the option is None.
  /// </summary>
  /// <param name="opt">The option to extract from.</param>
  /// <exception cref="InvalidOperationException">Thrown when <paramref name="opt"/> is None.</exception>
  public static explicit operator T(Opt<T> opt) => opt.Value;

  /// <summary>
  /// Gets a value indicating whether a value is present (Some).
  /// </summary>
  public bool HasValue => _hasValue;

  /// <summary>
  /// Gets the contained value when Some; throws when None.
  /// </summary>
  /// <exception cref="InvalidOperationException">The option is None.</exception>
  public T Value => _hasValue
    ? _value!
    : throw new InvalidOperationException("No value present");

  /// <summary>
  /// Gets a value indicating whether the option is empty (None).
  /// </summary>
  public bool IsNone => !_hasValue;

  /// <summary>
  /// Attempts to get the contained value.
  /// </summary>
  /// <param name="value">When this method returns, contains the value if present; otherwise the default value.</param>
  /// <returns>True if a value is present; otherwise false.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool TryGet(out T value)
  {
    if (_hasValue)
    {
      value = _value!;
      return true;
    }

    value = default!;
    return false;
  }

  /// <summary>
  /// Gets the contained value, or the default of <typeparamref name="T"/> when None.
  /// </summary>
  /// <returns>The contained value or default.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public T? GetValueOrDefault() => _hasValue ? _value : default;

  /// <summary>
  /// Gets the contained value, or <paramref name="defaultValue"/> when None.
  /// </summary>
  /// <param name="defaultValue">The value to return if the option is None.</param>
  /// <returns>The contained value or <paramref name="defaultValue"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public T GetValueOrDefault(in T defaultValue) => _hasValue
    ? _value!
    : defaultValue;

  /// <summary>
  /// Deconstructs this option to its presence flag and value.
  /// </summary>
  /// <param name="hasValue">True if a value is present; otherwise false.</param>
  /// <param name="value">The contained value if present; otherwise default.</param>
  public void Deconstruct(out bool hasValue, out T? value)
  {
    hasValue = _hasValue;
    value = _value;
  }

  /// <summary>
  /// Indicates whether the current option is equal to another option.
  /// </summary>
  /// <param name="other">The option to compare with.</param>
  /// <returns>True if both are None or both are Some with equal values; otherwise false.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Opt<T> other)
  {
    return _hasValue == other._hasValue
      && EqualityComparer<T>.Default.Equals(_value!, other._value!);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object? obj) => obj is Opt<T> other && Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => _hasValue
    ? HashCode.Combine(1, EqualityComparer<T>.Default.GetHashCode(_value!))
    : 0;

  public static bool operator ==(Opt<T> left, Opt<T> right) => left.Equals(right);
  public static bool operator !=(Opt<T> left, Opt<T> right) => !left.Equals(right);

  public override string ToString() => _hasValue ? $"Some({_value})" : "None";
}
