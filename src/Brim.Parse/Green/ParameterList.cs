namespace Brim.Parse.Green;

public sealed record ParameterList(
  GreenToken OpenParen,
  StructuralArray<GreenNode> Parameters,
  GreenToken CloseParen)
: GreenNode(SyntaxKind.ParameterList, OpenParen.Offset)
{
  public override int FullWidth => CloseParen.EndOffset - OpenParen.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return OpenParen;

    foreach (GreenNode param in Parameters)
      yield return param;

    yield return CloseParen;
  }
}

