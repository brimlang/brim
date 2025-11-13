namespace Brim.Parse.Green;

public sealed record OptionConstruct(
  GreenToken Open,
  ExprNode? Expr,
  GreenToken Close)
  : ExprNode(SyntaxKind.OptionConstruct, Open.Offset)
{
  public override int FullWidth => Close.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Open;
    if (Expr is not null) yield return Expr;
    yield return Close;
  }
}
