namespace Brim.Parse.Green;

public sealed record CallExpr(
  ExprNode Target,
  ArgumentList Arguments)
  : ExprNode(SyntaxKind.CallExpr, Target.Offset)
{
  public override int FullWidth => Arguments.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Target;
    yield return Arguments;
  }
}
