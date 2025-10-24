namespace Brim.Parse.Green;

public sealed record TupleConstruct(
  TypeRef Type,
  CommaList<ExprNode> Elements)
  : ExprNode(SyntaxKind.TupleConstruct, Type.Offset)
{
  public override int FullWidth => Type.FullWidth + Elements.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Type;
    yield return Elements;
  }
}
