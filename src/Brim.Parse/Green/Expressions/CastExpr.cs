namespace Brim.Parse.Green;

public sealed record CastExpr(
  ExprNode Target,
  GreenToken CastToken,
  TypeExpr Type)
  : ExprNode(SyntaxKind.CastExpr, Target.Offset)
{
  public override int FullWidth => Type.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Target;
    yield return CastToken;
    yield return Type;
  }
}
