namespace Brim.Parse.Green;

public sealed record MatchExpr(
  ExprNode Scrutinee,
  GreenToken Arrow,
  MatchArmList Arms)
  : ExprNode(SyntaxKind.MatchExpr, Scrutinee.Offset)
{
  public override int FullWidth => Arms.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Scrutinee;
    yield return Arrow;
    yield return Arms;
  }
}
