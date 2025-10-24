namespace Brim.Parse.Green;

public sealed record UnionConstruct(
  TypeRef Type,
  GreenToken UnionOpen,
  VariantInit Variant,
  GreenToken CloseBrace)
  : ExprNode(SyntaxKind.UnionConstruct, Type.Offset)
{
  public override int FullWidth =>
    Type.FullWidth + UnionOpen.FullWidth + Variant.FullWidth + CloseBrace.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Type;
    yield return UnionOpen;
    yield return Variant;
    yield return CloseBrace;
  }
}
