namespace Brim.Parse.Green;

public sealed record MatchGuard(
  GreenToken GuardToken,
  ExprNode Condition)
  : GreenNode(SyntaxKind.MatchGuard, GuardToken.Offset)
{
  public override int FullWidth => Condition.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return GuardToken;
    yield return Condition;
  }
}
