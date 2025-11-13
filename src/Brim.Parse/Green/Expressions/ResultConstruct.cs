namespace Brim.Parse.Green;

public sealed record ResultConstruct(
  GreenToken Open,
  ExprNode Expr,
  GreenToken Close)
  : ExprNode(SyntaxKind.ResultConstruct, Open.Offset)
{
  public override int FullWidth => Close.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Open;
    yield return Expr;
    yield return Close;
  }
}
