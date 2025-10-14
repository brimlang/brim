namespace Brim.Parse.Green;

public sealed record StructShape(
  CommaList<FieldDeclaration> FieldList) :
GreenNode(SyntaxKind.StructShape, FieldList.Offset)
{
  public override int FullWidth => FieldList.FullWidth;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return FieldList;
  }

  public static StructShape Parse(Parser p)
  {
    CommaList<FieldDeclaration> fields = CommaList<FieldDeclaration>.Parse(
        p,
        SyntaxKind.StructToken,
        SyntaxKind.CloseBlockToken,
        FieldDeclaration.Parse);

    return new StructShape(fields);
  }
}

