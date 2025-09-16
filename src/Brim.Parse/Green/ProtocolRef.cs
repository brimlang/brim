namespace Brim.Parse.Green;

public sealed record ProtocolRef(
  GreenNode TypeNode,
  GreenToken? TrailingComma) : GreenNode(SyntaxKind.GenericArgument, TypeNode.Offset) // reuse list coloring
{
  public override int FullWidth => (TrailingComma?.EndOffset ?? TypeNode.EndOffset) - TypeNode.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return TypeNode;
    if (TrailingComma is not null) yield return TrailingComma;
  }
}

