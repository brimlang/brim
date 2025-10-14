namespace Brim.Parse.Green;

public sealed record ConstraintRef(
  GreenToken? LeadingPlus,
  GreenNode TypeNode) :
GreenNode(SyntaxKind.ConstraintRef, LeadingPlus?.Offset ?? TypeNode.Offset)
{
  public override int FullWidth => TypeNode.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    if (LeadingPlus is not null) yield return LeadingPlus;
    yield return TypeNode;
  }
}

