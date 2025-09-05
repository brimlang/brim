namespace Brim.Parse;

/// <summary>
/// Fixed-capacity lookahead window over a token producer (includes EOF token).
/// </summary>
public struct LookAheadWindow<T, TProd>
  where T : struct
  where TProd : struct
{
  public delegate bool Reader(ref TProd prod, out T item);
  readonly Reader _read;
  TProd _producer;
  readonly T[] _buffer; // rolling window (we never need to keep older consumed tokens for now)
  int _count;           // number of filled slots (grows until EOF visible)
  int _index;           // current absolute index (0-based)
  bool _sawEof;

  public int Capacity { get; }

  public LookAheadWindow(TProd producer, Reader reader, int capacity)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
    Capacity = capacity;
    _producer = producer;
    _read = reader;
    _buffer = new T[capacity];
    _count = 0;
    _index = 0;
    _sawEof = false;
    EnsureFilled(0); // fill first token(s)
  }

  public readonly ref readonly T Current => ref _buffer[_index % Capacity];

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
    if (_sawEof) return false; // already on EOF
    _index++;
    EnsureFilled(0);
    return !_sawEof; // true if now on non-EOF
  }

  void EnsureFilled(int k)
  {
    if (k >= Capacity) throw new ArgumentOutOfRangeException(nameof(k), $"Lookahead capacity {Capacity} exceeded (requested LA({k})).");
    EnsureFilledInternal(k);
  }

  bool EnsureFilledSoft(int k)
  {
    if (k >= Capacity) return false;
    EnsureFilledInternal(k);
    return k < _count; // we have that many tokens (EOF included counts)
  }

  void EnsureFilledInternal(int k)
  {
    int needed = (_index + k + 1) - _count; // total tokens needed (absolute) - currently produced count
    while (needed > 0 && !_sawEof)
    {
      if (!_read(ref _producer, out T item)) break; // producer exhausted (already handled by item kind)
      int slot = _count % Capacity;
      _buffer[slot] = item;
      _count++;
      // naive EOF detection: rely on caller to interpret; cannot pattern match generic easily
      if (item is RawToken rt && rt.Kind == RawTokenKind.Eof) _sawEof = true;
      else if (item is SignificantToken st && st.Token.Kind == RawTokenKind.Eof) _sawEof = true;
      needed = (_index + k + 1) - _count;
    }
  }
}
