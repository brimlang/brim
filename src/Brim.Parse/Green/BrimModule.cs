namespace Brim.Parse.Green;

public sealed record BrimModule(
    ModuleDirective ModuleDirective,
    StructuralArray<GreenNode> Members,
    GreenToken Eof)
: GreenNode(SyntaxKind.Module, ModuleDirective.Offset)
{
  public override int FullWidth => Eof.EndOffset - ModuleDirective.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return ModuleDirective;
    foreach (GreenNode decl in Members)
      yield return decl;
    yield return Eof;
  }
}

