namespace Brim.Parse.Green;

public sealed record ImplementsRef(
  GreenNode TypeNode,
  GreenToken? TrailingPlus) : GreenNode(SyntaxKind.ImplementsRef, TypeNode.Offset)
{
  public override int FullWidth => (TrailingPlus?.EndOffset ?? TypeNode.EndOffset) - TypeNode.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return TypeNode;
    if (TrailingPlus is not null) yield return TrailingPlus;
  }
}

