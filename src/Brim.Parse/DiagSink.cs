namespace Brim.Parse;

/// <summary>
/// Ref-friendly diagnostic sink that aggregates diagnostics into a single List on the heap.
/// Copies of DiagSink share the same underlying list (reference type), so passing by value is fine.
/// </summary>
public readonly struct DiagSink
{
  readonly List<Diagnostic> _list;

  DiagSink(List<Diagnostic> list) => _list = list;

  public static DiagSink Create(int capacity = 8) => new(new List<Diagnostic>(capacity));

  public void Add(Diagnostic d) => _list.Add(d);

  public IReadOnlyList<Diagnostic> Items => _list;
}
