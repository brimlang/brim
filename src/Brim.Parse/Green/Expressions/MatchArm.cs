namespace Brim.Parse.Green;

public sealed record MatchArm(
  GreenNode Pattern,
  MatchGuard? Guard,
  GreenToken Arrow,
  ExprNode Target,
  GreenToken? Terminator)
  : GreenNode(SyntaxKind.MatchArm, Pattern.Offset)
{
  public override int FullWidth {
    get
    {
      int end = Terminator?.EndOffset ?? Target.EndOffset;
      return end - Offset;
    }
  }

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Pattern;
    if (Guard is not null)
      yield return Guard;
    yield return Arrow;
    yield return Target;
    if (Terminator is not null)
      yield return Terminator;
  }
}
