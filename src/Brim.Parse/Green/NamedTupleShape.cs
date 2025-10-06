namespace Brim.Parse.Green;

public sealed record NamedTupleShape(
  CommaList<NamedTupleElement> ElementList
) : GreenNode(SyntaxKind.NamedTupleShape, ElementList.Offset)
{
  public override int FullWidth => ElementList.FullWidth;
  public override IEnumerable<GreenNode> GetChildren() => ElementList.GetChildren();

  public static NamedTupleShape Parse(Parser p)
  {
    CommaList<NamedTupleElement> elems = CommaList<NamedTupleElement>.Parse(
      p,
      SyntaxKind.NamedTupleToken,
      SyntaxKind.CloseBlockToken,
      NamedTupleElement.Parse
    );

    if (elems.Elements.Count == 0)
      p.AddDiagEmptyNamedTupleElementList();

    return new(elems);
  }
}
