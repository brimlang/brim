namespace Brim.Parse.Green;

public sealed record LiteralExpr(GreenToken Token) : ExprNode(SyntaxKind.LiteralExpr, Token.Offset)
{
  public override int FullWidth => Token.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Token;
  }
}
