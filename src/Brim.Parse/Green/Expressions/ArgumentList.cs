namespace Brim.Parse.Green;

public sealed record ArgumentList(CommaList<ExprNode> Items) : ExprNode(SyntaxKind.ArgumentList, Items.Offset)
{
  public override int FullWidth => Items.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Items;
  }
}
