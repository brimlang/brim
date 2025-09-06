namespace Brim.Parse;

/// <summary>
/// Ref-friendly diagnostic sink that aggregates diagnostics into a single List on the heap.
/// Copies of DiagSink share the same underlying list (reference type), so passing by value is fine.
/// </summary>
public struct DiagSink
{
  readonly List<Diagnostic> _list;
  int _tooManyFlag; // 0 = not tripped, 1 = tripped

  public const int MaxDiagnostics = 512; // flood cap (option 2)

  DiagSink(List<Diagnostic> list) => _list = list;

  public static DiagSink Create(int capacity = 8) => new(new List<Diagnostic>(capacity));

  public void Add(Diagnostic d)
  {
    if (_tooManyFlag != 0) return; // already capped
    if (_list.Count >= MaxDiagnostics)
    {
      // Replace last slot with TooManyErrors if not already present and mark flag.
      if (_list.Count == MaxDiagnostics)
      {
        // Emit special cap diagnostic using last diagnostic for positional context.
        Diagnostic last = _list[^1];
        RawToken pseudo = new(RawTokenKind.Error, last.Offset, 0, last.Line, last.Column);
        _list[^1] = DiagFactory.TooManyErrors(pseudo);
      }
      _tooManyFlag = 1;
      return;
    }
    _list.Add(d);
  }

  public IReadOnlyList<Diagnostic> Items => _list;

  public bool IsCapped => _tooManyFlag != 0;
}
