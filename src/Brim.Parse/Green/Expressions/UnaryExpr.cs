namespace Brim.Parse.Green;

public sealed record UnaryExpr(
  GreenToken Operator,
  ExprNode Operand)
  : ExprNode(SyntaxKind.UnaryExpr, Operator.Offset)
{
  public override int FullWidth => Operand.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Operator;
    yield return Operand;
  }
}
