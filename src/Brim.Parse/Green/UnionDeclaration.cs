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

public sealed record UnionDeclaration(
  DeclarationName Name,
  GreenToken Colon,
  GreenToken UnionOpen,
  StructuralArray<UnionVariantDeclaration> Variants,
  GreenToken Close,
  GreenToken Terminator) : GreenNode(SyntaxKind.UnionDeclaration, Name.Offset)
{
  public override int FullWidth => Terminator.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    yield return UnionOpen;
    foreach (UnionVariantDeclaration v in Variants) yield return v;
    yield return Close;
    yield return Terminator;
  }

  public static UnionDeclaration Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.UnionToken);
    ImmutableArray<UnionVariantDeclaration>.Builder vars = ImmutableArray.CreateBuilder<UnionVariantDeclaration>();

    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        UnionVariantDeclaration variant = UnionVariantDeclaration.Parse(p);
        vars.Add(variant);
        if (variant.TrailingComma is null) break; // no comma => end
        if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob)) break; // trailing comma on last
        continue; // another variant expected
      }
    }

    StructuralArray<UnionVariantDeclaration> arr = [.. vars];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new(name, colon, open, arr, close, term);
  }
}
