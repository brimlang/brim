namespace Brim.Parse.Collections;

/// <summary>
/// Ref-friendly diagnostic sink that aggregates diagnostics into a single List on the heap.
/// Copies of DiagnosticList share the same underlying list (reference type), so passing by value is fine.
/// </summary>
public struct DiagnosticList
{
  readonly List<Diagnostic> _list;

  public const int MaxDiagnostics = 512; // flood cap (option 2)

  DiagnosticList(List<Diagnostic> list) => _list = list;

  public static DiagnosticList Create(int capacity = 8) => new(new List<Diagnostic>(capacity));

  public bool IsCapped { get; private set; }

  public readonly int Count => _list.Count;

  public void Add(Diagnostic d)
  {
    if (IsCapped) return; // already capped
    if (_list.Count >= MaxDiagnostics)
    {
      // Replace last slot with TooManyErrors if not already present and mark flag.
      if (_list.Count == MaxDiagnostics)
      {
        // Emit special cap diagnostic using last diagnostic for positional context.
        Diagnostic last = _list[^1];
        RawToken pseudo = new(RawKind.Error, last.Offset, 0, last.Line, last.Column);
        _list[^1] = Diagnostic.TooManyErrors(pseudo);
      }
      IsCapped = true;
      return;
    }

    _list.Add(d);
  }

  public readonly ImmutableArray<Diagnostic> GetSortedDiagnostics()
  {
    // Stable sort diagnostics (already in emission order; ensure order by offset then insertion)
    if (_list.Count > 1)
    {
      _list.Sort(static (a, b) =>
      {
        int cmp = a.Offset.CompareTo(b.Offset);
        if (cmp != 0) return cmp;

        // tie-breaker: line, column to maintain determinism
        cmp = a.Line.CompareTo(b.Line);
        if (cmp != 0) return cmp;

        // fallback: code (semi-stable; list.Sort is not stable in .NET so we emulate minimal extra ordering)
        return ((int)a.Code).CompareTo((int)b.Code);
      });
    }

    return [.. _list];
  }
}
