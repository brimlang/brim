namespace Brim.Parse.Green;

public sealed record PropagationExpr(
  ExprNode Target,
  GreenToken Operator)
  : ExprNode(SyntaxKind.PropagationExpr, Target.Offset)
{
  public override int FullWidth => Operator.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Target;
    yield return Operator;
  }
}
