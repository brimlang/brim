namespace Brim.Parse;

internal readonly struct Opt<T> where T : class
{
  readonly T? _value = null;

  public Opt(in T value)
  {
    ArgumentNullException.ThrowIfNull(value);
    _value = value;
  }

  public static Opt<T> None { get; } = default;

  public static implicit operator Opt<T>(in T value) => new(value);
  public static explicit operator T(Opt<T> opt) => opt.Value;

  public bool HasValue => _value is not null;
  public T Value => _value ?? throw new InvalidOperationException("No value present");
}
