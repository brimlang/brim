namespace Brim.Parse.Green;

public sealed record ErrorConstruct(
  GreenToken Open,
  ExprNode Expr,
  GreenToken CloseBrace)
  : ExprNode(SyntaxKind.ErrorConstruct, Open.Offset)
{
  public override int FullWidth => CloseBrace.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Open;
    yield return Expr;
    yield return CloseBrace;
  }
}
