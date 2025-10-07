namespace Brim.Parse.Green;

public sealed record FunctionParameter(
  TypeExpr TypeNode) :
GreenNode(SyntaxKind.FunctionParameter, TypeNode.Offset)
{
  public override int FullWidth => TypeNode.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    foreach (GreenNode child in TypeNode.GetChildren())
      yield return child;
  }

  public static FunctionParameter Parse(Parser p) =>
    new(TypeNode: TypeExpr.Parse(p));
}
