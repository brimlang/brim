namespace Brim.Parse.Producers;

/// <summary>
/// A ring buffer that supports lookahead and is backed by a producer.
/// </summary>
public sealed class RingBuffer<T>
  where T : struct, IToken
{
  readonly ITokenSource<T> _source;
  readonly T[] _buffer;

  int _count;
  int _index;
  bool _ended;

  public RingBuffer(ITokenSource<T> producer, int capacity)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
    Capacity = capacity;

    _source = producer;
    _buffer = new T[capacity];
    _count = 0;
    _index = 0;
    _ended = false;

    EnsureFilled(0); // fill first token(s)
  }

  public int Capacity { get; }

  public ref readonly T Current => ref _buffer[_index % Capacity];

  public ref readonly T Peek(int k)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(k);
    EnsureFilled(k);
    return ref _buffer[(_index + k) % Capacity];
  }

  public bool TryPeek(int k, out T value)
  {
    value = default;
    if (k < 0) return false;

    if (!EnsureFilledSoft(k)) return false;

    value = _buffer[(_index + k) % Capacity];
    return true;
  }

  public bool Advance()
  {
    if (_ended) return false;

    _index++;
    EnsureFilled(0);
    return !_ended;
  }

  void EnsureFilled(int k)
  {
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(k, Capacity);
    EnsureFilledInternal(k);
  }

  bool EnsureFilledSoft(int k)
  {
    if (k >= Capacity) return false;
    EnsureFilledInternal(k);
    return k < _count;
  }

  void EnsureFilledInternal(int k)
  {
    int needed = _index + k + 1 - _count; // total tokens needed (absolute) - currently produced count
    while (needed > 0 && !_ended)
    {
      if (!_source.TryRead(out T item)) break; // source exhausted

      int slot = _count % Capacity;
      _buffer[slot] = item;
      _count++;

      if (_source.IsEndOfSource(item))
        _ended = true;

      needed = _index + k + 1 - _count;
    }
  }
}
