namespace Brim.Parse.Green;

public sealed record BinaryExpr(
  ExprNode Left,
  GreenToken Operator,
  ExprNode Right)
  : ExprNode(SyntaxKind.BinaryExpr, Left.Offset)
{
  public override int FullWidth => Right.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Left;
    yield return Operator;
    yield return Right;
  }
}
