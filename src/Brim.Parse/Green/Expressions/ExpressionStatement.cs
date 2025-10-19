namespace Brim.Parse.Green;

public sealed record ExpressionStatement(
  ExprNode Expression,
  GreenToken Terminator)
  : GreenNode(SyntaxKind.ExpressionStatement, Expression.Offset)
{
  public override int FullWidth => Terminator.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Expression;
    yield return Terminator;
  }
}
