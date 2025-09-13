namespace Brim.Parse.Green;

public sealed record UnionVariantDeclaration(
  GreenToken Identifier,
  GreenToken Colon,
  GreenNode Type) :
GreenNode(SyntaxKind.UnionVariantDeclaration, Identifier.Offset),
IParsable<UnionVariantDeclaration>
{
  public override int FullWidth => Type.EndOffset - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    yield return Colon;
    yield return Type;
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

    return new(nameTok, colon, ty);
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

    foreach (UnionVariantDeclaration v in Variants)
      yield return v;

    yield return Close;
    yield return Terminator;
  }

  public static UnionDeclaration Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.UnionToken);
    ImmutableArray<UnionVariantDeclaration>.Builder vars = ImmutableArray.CreateBuilder<UnionVariantDeclaration>();
    while (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      int before = p.Current.CoreToken.Offset;
      vars.Add(UnionVariantDeclaration.Parse(p));
      if (p.MatchRaw(RawKind.Comma))
        _ = p.ExpectRaw(RawKind.Comma);

      if (p.Current.CoreToken.Offset == before)
      {
        // No progress; break to avoid infinite loop.
        break;
      }
    }

    StructuralArray<UnionVariantDeclaration> arr = [.. vars];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new(name, colon, open, arr, close, term);
  }
}
