using Brim.Parse.Producers;

namespace Brim.Parse;

/// <summary>
/// Fixed-capacity lookahead window over a token producer (includes EOB token).
/// </summary>
public sealed class LookAheadWindow<T, TProd>
  where T : struct, IToken
  where TProd : ITokenProducer<T>
{
  readonly TProd _producer;
  readonly T[] _buffer;

  int _count;
  int _index;
  bool _sawEob;

  public LookAheadWindow(TProd producer, int capacity)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
    Capacity = capacity;

    _producer = producer;
    _buffer = new T[capacity];
    _count = 0;
    _index = 0;
    _sawEob = false;

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
    if (k < 0) { value = default; return false; }
    if (!EnsureFilledSoft(k)) { value = default; return false; }
    value = _buffer[(_index + k) % Capacity];
    return true;
  }

  public bool Advance()
  {
    if (_sawEob) return false; // already on EOB

    _index++;
    EnsureFilled(0);
    return !_sawEob; // true if now on non-EOB
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
    return k < _count; // we have that many tokens (EOB included counts)
  }

  void EnsureFilledInternal(int k)
  {
    int needed = _index + k + 1 - _count; // total tokens needed (absolute) - currently produced count
    while (needed > 0 && !_sawEob)
    {
      if (!_producer.TryRead(out T item)) break; // producer exhausted

      int slot = _count % Capacity;
      _buffer[slot] = item;
      _count++;

      if (Utilities.GetRawTokenKind(item) == RawKind.Eob)
        _sawEob = true;

      needed = _index + k + 1 - _count;
    }
  }
}
