namespace Brim.Core.Collections;

/// <summary>
/// Ref-friendly diagnostic sink that aggregates diagnostics into a single List on the heap.
/// Copies of DiagnosticList share the same underlying list (reference type), so passing by value is fine.
/// </summary>
public struct DiagnosticList
{
  readonly List<Diagnostic> _list;

  public const int MaxDiagnostics = 512;

  DiagnosticList(List<Diagnostic> list) => _list = list;

  public static DiagnosticList Create(int capacity = 8) => new(new List<Diagnostic>(capacity));

  public bool IsCapped { get; private set; }

  public readonly int Count => _list.Count;

  public void Add(Diagnostic diagnostic)
  {
    if (IsCapped) return;
    if (_list.Count >= MaxDiagnostics)
    {
      if (_list.Count == MaxDiagnostics)
      {
        Diagnostic last = _list[^1];
        _list[^1] = Diagnostic.Parse.TooManyErrors(last.Offset, last.Line, last.Column);
      }
      IsCapped = true;
      return;
    }

    _list.Add(diagnostic);
  }

  public readonly ImmutableArray<Diagnostic> GetSortedDiagnostics()
  {
    if (_list.Count > 1)
    {
      _list.Sort(static (a, b) =>
      {
        int cmp = a.Offset.CompareTo(b.Offset);
        if (cmp != 0) return cmp;

        cmp = a.Line.CompareTo(b.Line);
        if (cmp != 0) return cmp;

        return ((int)a.Code).CompareTo((int)b.Code);
      });
    }

    return [.. _list];
  }
}
