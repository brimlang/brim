namespace Brim.Parse.Green;

public sealed record ProtocolRef(
  GreenNode TypeNode) : GreenNode(SyntaxKind.GenericArgument, TypeNode.Offset)
{
  public override int FullWidth => TypeNode.FullWidth;
  public override IEnumerable<GreenNode> GetChildren() { yield return TypeNode; }

  public static ProtocolRef Parse(Parser p) => new(TypeExpr.Parse(p));
}

