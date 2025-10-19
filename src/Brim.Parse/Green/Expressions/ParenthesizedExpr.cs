namespace Brim.Parse.Green;

public sealed record ParenthesizedExpr(
  GreenToken OpenParen,
  ExprNode Expression,
  GreenToken CloseParen)
  : ExprNode(SyntaxKind.ParenthesizedExpr, OpenParen.Offset)
{
  public override int FullWidth => CloseParen.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return OpenParen;
    yield return Expression;
    yield return CloseParen;
  }
}
