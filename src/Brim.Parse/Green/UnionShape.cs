using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record UnionShape(
  GreenToken Open,
  StructuralArray<UnionVariantDeclaration> Variants,
  GreenToken Close)
  : GreenNode(SyntaxKind.UnionShape, Open.Offset)
{
  public override int FullWidth => Close.EndOffset - Open.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Open;
    foreach (UnionVariantDeclaration v in Variants) yield return v;
    yield return Close;
  }

  public static UnionShape Parse(Parser p)
  {
    GreenToken open = p.ExpectSyntax(SyntaxKind.UnionToken);
    ImmutableArray<UnionVariantDeclaration>.Builder vars = ImmutableArray.CreateBuilder<UnionVariantDeclaration>();

    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        int before = p.Current.CoreToken.Offset;
        UnionVariantDeclaration variant = UnionVariantDeclaration.Parse(p);
        vars.Add(variant);
        if (variant.TrailingComma is null) break;
        if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob)) break;
        if (p.Current.CoreToken.Offset == before) break; // progress guard
      }
    }

    StructuralArray<UnionVariantDeclaration> arr = [.. vars];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    return new UnionShape(open, arr, close);
  }
}

