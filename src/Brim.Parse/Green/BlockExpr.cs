namespace Brim.Parse.Green;

public sealed record BlockExpr(
  TerminatorList<GreenNode> StatementList) :
ExprNode(SyntaxKind.BlockExpr, StatementList.Offset)
{
  public override int FullWidth => StatementList.FullWidth;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return StatementList;
  }

  internal static BlockExpr Parse(Parser p)
  {
    TerminatorList<GreenNode> statementList = TerminatorList<GreenNode>.Parse(
      p,
      SyntaxKind.OpenBlockToken,
      SyntaxKind.CloseBlockToken,
      static p => p.LooksLikeAssignment() ? p.ParseAssignmentStatement() : p.ParseExpression());

    return new BlockExpr(statementList);
  }
}
