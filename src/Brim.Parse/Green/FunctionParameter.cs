namespace Brim.Parse.Green;

public sealed record FunctionParameter(
  GreenNode TypeNode) :
GreenNode(SyntaxKind.FunctionParameter, TypeNode.Offset)
{
  public override int FullWidth => TypeNode.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return TypeNode;
  }

  public static FunctionParameter Parse(Parser p) =>
    new(TypeNode: TypeExpr.Parse(p));
}
