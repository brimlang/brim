namespace Brim.Parse.Green;

public sealed record FunctionLiteral(
  GreenToken Arrow,
  LambdaParams Parameters,
  ExprNode Body)
  : ExprNode(SyntaxKind.FunctionLiteral, Arrow.Offset)
{
  public override int FullWidth => Body.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Arrow;
    yield return Parameters;
    yield return Body;
  }
}
