namespace Brim.Parse.Green;

public sealed record UnionVariantDeclaration(
  GreenToken Identifier,
  UnionVariantDeclaration.VariantTypeExpr? TypeExpr
) :
GreenNode(SyntaxKind.UnionVariantDeclaration, Identifier.Offset),
IParsable<UnionVariantDeclaration>
{
  public override int FullWidth => (TypeExpr?.EndOffset ?? Identifier.EndOffset) - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    if (TypeExpr is not null) yield return TypeExpr;
  }

  public static UnionVariantDeclaration Parse(Parser p)
  {
    GreenToken id = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    VariantTypeExpr? vt = null;
    if (p.MatchRaw(RawKind.Colon))
    {
      vt = VariantTypeExpr.Parse(p);
    }

    return new UnionVariantDeclaration(id, vt);
  }

  public sealed record VariantTypeExpr(
      GreenToken Colon,
      TypeExpr TypeExpr
  ) : GreenNode(SyntaxKind.VariantType, Colon.Offset)
  {
    public override int FullWidth => TypeExpr.EndOffset - Colon.Offset;
    public override IEnumerable<GreenNode> GetChildren()
    {
      yield return Colon;
      yield return TypeExpr;
    }

    public static VariantTypeExpr Parse(Parser p) =>
      new(
        Colon: p.ExpectSyntax(SyntaxKind.ColonToken),
        TypeExpr: TypeExpr.Parse(p)
      );
  }
}
