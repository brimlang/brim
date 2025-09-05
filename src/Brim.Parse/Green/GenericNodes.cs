namespace Brim.Parse.Green;

public sealed record GenericParameterList(
  GreenToken Open,
  ImmutableArray<Identifier> Parameters,
  GreenToken Close)
: GreenNode(SyntaxKind.GenericParameterList, Open.Offset)
{
  public override int FullWidth => Close.EndOffset - Open.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Open;
    foreach (Identifier p in Parameters) yield return p;
    yield return Close;
  }
}

public sealed record GenericArgumentList(
  GreenToken Open,
  StructuralArray<GreenNode> Arguments,
  GreenToken Close)
: GreenNode(SyntaxKind.GenericArgumentList, Open.Offset)
{
  public override int FullWidth => Close.EndOffset - Open.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Open;
    foreach (GreenNode a in Arguments) yield return a;
    yield return Close;
  }
}
