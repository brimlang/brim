namespace Brim.Parse.Green;

public sealed record NamedTupleElement(
  GreenNode TypeNode,
  GreenToken? TrailingComma) : GreenNode(SyntaxKind.NamedTupleElement, TypeNode.Offset)
{
  public override int FullWidth => (TrailingComma?.EndOffset ?? TypeNode.EndOffset) - TypeNode.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return TypeNode;
    if (TrailingComma is not null) yield return TrailingComma;
  }
}
