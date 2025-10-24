namespace Brim.Parse.Green;

public sealed record ResultConstruct(
  GreenToken BangOpen,
  ExprNode Value,
  GreenToken CloseBrace)
  : ExprNode(SyntaxKind.ResultConstruct, BangOpen.Offset)
{
  public override int FullWidth =>
    BangOpen.FullWidth + Value.FullWidth + CloseBrace.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return BangOpen;
    yield return Value;
    yield return CloseBrace;
  }
}
