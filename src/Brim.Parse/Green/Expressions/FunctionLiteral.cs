namespace Brim.Parse.Green;

public sealed record FunctionLiteral(
  GreenToken Open,
  LambdaParams Parameters,
  GreenToken Close,
  ExprNode Body)
  : ExprNode(SyntaxKind.FunctionLiteral, Open.Offset)
{
  public override int FullWidth => Body.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Open;
    yield return Parameters;
    yield return Close;
    yield return Body;
  }
}
