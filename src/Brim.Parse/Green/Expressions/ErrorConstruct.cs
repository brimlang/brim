namespace Brim.Parse.Green;

public sealed record ErrorConstruct(
  GreenToken BangBangOpen,
  ExprNode Value,
  GreenToken CloseBrace)
  : ExprNode(SyntaxKind.ErrorConstruct, BangBangOpen.Offset)
{
  public override int FullWidth =>
    BangBangOpen.FullWidth + Value.FullWidth + CloseBrace.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return BangBangOpen;
    yield return Value;
    yield return CloseBrace;
  }
}
