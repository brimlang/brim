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
  StructuralArray<GreenToken> VariantSeparators,
  GreenToken Close,
  GreenToken Terminator) : GreenNode(SyntaxKind.UnionDeclaration, Name.Offset)
{
  public override int FullWidth => Terminator.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    yield return UnionOpen;
    for (int i = 0; i < Variants.Count; i++)
    {
      yield return Variants[i];
      if (i < VariantSeparators.Count)
        yield return VariantSeparators[i];
    }
    yield return Close;
    yield return Terminator;
  }

  public static UnionDeclaration Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.UnionToken);
    ImmutableArray<UnionVariantDeclaration>.Builder vars = ImmutableArray.CreateBuilder<UnionVariantDeclaration>();
    ImmutableArray<GreenToken>.Builder seps = ImmutableArray.CreateBuilder<GreenToken>();

    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        int before = p.Current.CoreToken.Offset;
        vars.Add(UnionVariantDeclaration.Parse(p));
        if (p.MatchRaw(RawKind.Comma))
        {
          GreenToken commaTok = p.ExpectSyntax(SyntaxKind.CommaToken);
          seps.Add(commaTok);
          if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob))
            break; // trailing comma
          continue;
        }
        break;
      }
    }

    StructuralArray<UnionVariantDeclaration> arr = [.. vars];
    StructuralArray<GreenToken> sepArr = [.. seps];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new(name, colon, open, arr, sepArr, close, term);
  }
}
