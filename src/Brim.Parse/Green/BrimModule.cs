namespace Brim.Parse.Green;

public sealed record BrimModule(
  ModuleDirective ModuleDirective,
  StructuralArray<GreenNode> Members,
  GreenToken Eob) :
GreenNode(SyntaxKind.Module, ModuleDirective.Offset)
{
  public StructuralArray<Diagnostic> Diagnostics { get; init; } = [];

  public override int FullWidth => Eob.EndOffset - ModuleDirective.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return ModuleDirective;
    foreach (GreenNode decl in Members)
      yield return decl;
    yield return Eob;
  }
}

