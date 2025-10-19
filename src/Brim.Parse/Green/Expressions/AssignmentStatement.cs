namespace Brim.Parse.Green;

public sealed record AssignmentStatement(
  AssignmentTarget Target,
  GreenToken EqualToken,
  ExprNode Expression,
  GreenToken Terminator)
  : GreenNode(SyntaxKind.AssignmentStatement, Target.Offset)
{
  public override int FullWidth => Terminator.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Target;
    yield return EqualToken;
    yield return Expression;
    yield return Terminator;
  }
}
