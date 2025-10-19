namespace Brim.Parse.Green;

public sealed record MemberAccessExpr(
  ExprNode Target,
  GreenToken Dot,
  GreenToken Member)
  : ExprNode(SyntaxKind.MemberAccessExpr, Target.Offset)
{
  public override int FullWidth => Member.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Target;
    yield return Dot;
    yield return Member;
  }
}
