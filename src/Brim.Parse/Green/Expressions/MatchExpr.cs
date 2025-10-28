namespace Brim.Parse.Green;

public sealed record MatchExpr(
  ExprNode Scrutinee,
  GreenToken Arrow,
  GreenNode Body)
  : ExprNode(SyntaxKind.MatchExpr, Scrutinee.Offset)
{
  public override int FullWidth => Body.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Scrutinee;
    yield return Arrow;
    yield return Body;
  }
}
