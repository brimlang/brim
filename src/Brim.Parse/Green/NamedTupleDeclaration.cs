namespace Brim.Parse.Green;

public sealed record NamedTupleElement(
  TypeExpr TypeNode
) : GreenNode(SyntaxKind.NamedTupleElement, TypeNode.Offset)
{
  public override int FullWidth => TypeNode.FullWidth;
  public override IEnumerable<GreenNode> GetChildren()
  {
    foreach (GreenNode child in TypeNode.GetChildren())
      yield return child;
  }
  public static NamedTupleElement Parse(Parser p) => new(TypeExpr.Parse(p));
}
