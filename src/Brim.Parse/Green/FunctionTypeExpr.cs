namespace Brim.Parse.Green;

public sealed record FunctionTypeExpr(
  FunctionShape Shape
) : GreenNode(SyntaxKind.FunctionTypeExpr, Shape.Offset)
{
  public override int FullWidth => Shape.FullWidth;
  public override IEnumerable<GreenNode> GetChildren() => Shape.GetChildren();
}
