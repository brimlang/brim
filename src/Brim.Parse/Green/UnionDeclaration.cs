namespace Brim.Parse.Green;

public sealed record UnionVariantDeclaration(
  GreenToken Identifier,
  GreenToken Colon,
  GreenNode Type,
  GreenToken? TrailingComma) :
GreenNode(SyntaxKind.UnionVariantDeclaration, Identifier.Offset),
IParsable<UnionVariantDeclaration>
{
  public override int FullWidth => (TrailingComma?.EndOffset ?? Type.EndOffset) - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    yield return Colon;
    yield return Type;
    if (TrailingComma is not null) yield return TrailingComma;
  }

  public static UnionVariantDeclaration Parse(Parser p)
  {
    GreenToken nameTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken typeNameTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenNode ty = typeNameTok;
    if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
    {
      ty = GenericType.ParseAfterName(p, typeNameTok);
    }
    GreenToken? trailing = null;
    if (p.MatchRaw(RawKind.Comma))
      trailing = p.ExpectSyntax(SyntaxKind.CommaToken);
    return new(nameTok, colon, ty, trailing);
  }
}
