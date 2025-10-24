namespace Brim.Parse.Green;

public sealed record SeqConstruct(
  GreenToken SeqKeyword,
  GenericArgumentList? GenericArgs,
  CommaList<ExprNode> Elements)
  : ExprNode(SyntaxKind.SeqConstruct, SeqKeyword.Offset)
{
  public override int FullWidth =>
    SeqKeyword.FullWidth + (GenericArgs?.FullWidth ?? 0) + Elements.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return SeqKeyword;
    if (GenericArgs is not null) yield return GenericArgs;
    yield return Elements;
  }
}
