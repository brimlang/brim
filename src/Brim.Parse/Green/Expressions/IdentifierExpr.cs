namespace Brim.Parse.Green;

public sealed record IdentifierExpr(GreenToken Identifier) : ExprNode(SyntaxKind.IdentifierExpr, Identifier.Offset)
{
  public override int FullWidth => Identifier.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
  }
}
