namespace Brim.Parse.Green;

public sealed record BrimModule(
  ModuleDirective ModuleDirective,
  StructuralArray<GreenNode> Members,
  GreenToken Eob,
  StructuralArray<Diagnostic> Diagnostics)
: GreenNode(SyntaxKind.Module, ModuleDirective.Offset)
{
  public override int FullWidth => Eob.EndOffset - ModuleDirective.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return ModuleDirective;
    foreach (GreenNode decl in Members)
      yield return decl;
    yield return Eob;
  }

  // Binary search for first diagnostic whose offset >= target. Returns -1 if none.
  public int FindFirstDiagnosticAtOrAfter(int target)
  {
  int count = Diagnostics.Count;
  if (count == 0) return -1;
  int lo = 0, hi = count - 1, result = -1;
    while (lo <= hi)
    {
      int mid = (int)((uint)(lo + hi) >> 1); // avoid overflow
      Diagnostic d = Diagnostics[mid];
      if (d.Offset >= target)
      {
        result = mid;
        hi = mid - 1;
      }
      else
      {
        lo = mid + 1;
      }
    }
    return result;
  }
}

