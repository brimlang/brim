namespace Brim.Parse.Green;

public sealed record UnionShape(
  CommaList<UnionVariantDeclaration> VariantList) :
GreenNode(SyntaxKind.UnionShape, VariantList.Offset)
{
  public override int FullWidth => VariantList.FullWidth;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return VariantList;
  }

  public static UnionShape Parse(Parser p)
  {
    CommaList<UnionVariantDeclaration> variants = CommaList<UnionVariantDeclaration>.Parse(
        p,
        SyntaxKind.UnionToken,
        SyntaxKind.CloseBlockToken,
        UnionVariantDeclaration.Parse);

    return new(variants);
  }
}

