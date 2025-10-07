namespace Brim.Parse.Green;

public sealed record ProtocolRef(
  TypeExpr TypeNode) : GreenNode(SyntaxKind.GenericArgument, TypeNode.Offset)
{
  public override int FullWidth => TypeNode.FullWidth;
  public override IEnumerable<GreenNode> GetChildren()
  {
    foreach (GreenNode child in TypeNode.GetChildren())
      yield return child;
  }

  public static ProtocolRef Parse(Parser p) => new(TypeExpr.Parse(p));
}

