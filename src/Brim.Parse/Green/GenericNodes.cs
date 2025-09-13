namespace Brim.Parse.Green;

public sealed record GenericParameterList(
  GreenToken Open,
  ImmutableArray<GenericParameter> Parameters,
  GreenToken Close)
: GreenNode(SyntaxKind.GenericParameterList, Open.Offset)
{
  public override int FullWidth => Close.EndOffset - Open.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Open;
    foreach (GenericParameter p in Parameters) yield return p;
    yield return Close;
  }
}

public sealed record GenericParameter(
  GreenToken Name,
  ConstraintList? Constraints,
  GreenToken? TrailingComma) : GreenNode(SyntaxKind.GenericParameter, Name.Offset)
{
  public override int FullWidth => (TrailingComma?.EndOffset ?? (Constraints?.EndOffset ?? Name.EndOffset)) - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    if (Constraints is not null) yield return Constraints;
    if (TrailingComma is not null) yield return TrailingComma;
  }
}

public sealed record ConstraintList(
  GreenToken Colon,
  StructuralArray<GreenNode> Constraints) : GreenNode(SyntaxKind.ConstraintList, Colon.Offset)
{
  public override int FullWidth => Constraints.Count == 0 ? Colon.FullWidth : Constraints[^1].EndOffset - Colon.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Colon;
    foreach (GreenNode c in Constraints) yield return c;
  }
}

public sealed record GenericArgument(
  GreenNode TypeNode,
  GreenToken? TrailingComma) : GreenNode(SyntaxKind.GenericArgument, TypeNode.Offset)
{
  public override int FullWidth => (TrailingComma?.EndOffset ?? TypeNode.EndOffset) - TypeNode.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return TypeNode;
    if (TrailingComma is not null) yield return TrailingComma;
  }
}

public sealed record GenericArgumentList(
  GreenToken Open,
  StructuralArray<GenericArgument> Arguments,
  GreenToken Close)
: GreenNode(SyntaxKind.GenericArgumentList, Open.Offset)
{
  public override int FullWidth => Close.EndOffset - Open.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Open;
    foreach (GenericArgument a in Arguments) yield return a;
    yield return Close;
  }
}
