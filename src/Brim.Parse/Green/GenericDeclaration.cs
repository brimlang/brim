namespace Brim.Parse.Green;

// Orchestrator for any declaration that starts with Identifier '[' ... ']' ':' <body>
public static class GenericDeclaration
{
  public static GreenNode Parse(Parser p)
  {
    GreenToken idTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GenericParameterList? gpl = GenericParameterListParser.TryParse(p);
    DeclarationName dname = new(idTok, gpl);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);

    if (p.MatchRaw(RawKind.PercentLBrace))
      return ParseGenericStructBody(p, dname, colon);
    if (p.MatchRaw(RawKind.PipeLBrace))
      return ParseGenericUnionBody(p, dname, colon);
    if (p.MatchRaw(RawKind.HashLBrace))
      return ParseGenericNamedTupleBody(p, dname, colon);

    p.AddDiagUnexpectedGenericBody();
    return new GreenToken(SyntaxKind.ErrorToken, p.Current);
  }

  static StructDeclaration ParseGenericStructBody(Parser p, DeclarationName name, GreenToken colon)
  {
    GreenToken openStruct = p.ExpectSyntax(SyntaxKind.StructToken);
    ImmutableArray<FieldDeclaration>.Builder fields = ImmutableArray.CreateBuilder<FieldDeclaration>();
    ImmutableArray<GreenToken>.Builder seps = ImmutableArray.CreateBuilder<GreenToken>();
    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        fields.Add(FieldDeclaration.Parse(p));
        if (p.MatchRaw(RawKind.Comma))
        {
          GreenToken commaTok = p.ExpectSyntax(SyntaxKind.CommaToken);
          seps.Add(commaTok);
          if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob)) break; // trailing
          continue;
        }
        break;
      }
    }

    StructuralArray<FieldDeclaration> arr = [.. fields];
    StructuralArray<GreenToken> sepArr = [.. seps];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new StructDeclaration(name, colon, openStruct, arr, sepArr, close, term);
  }

  static UnionDeclaration ParseGenericUnionBody(Parser p, DeclarationName name, GreenToken colon)
  {
    GreenToken openUnion = p.ExpectSyntax(SyntaxKind.UnionToken);
    ImmutableArray<UnionVariantDeclaration>.Builder vars = ImmutableArray.CreateBuilder<UnionVariantDeclaration>();
    ImmutableArray<GreenToken>.Builder seps = ImmutableArray.CreateBuilder<GreenToken>();
    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        vars.Add(UnionVariantDeclaration.Parse(p));
        if (p.MatchRaw(RawKind.Comma))
        {
          GreenToken commaTok = p.ExpectSyntax(SyntaxKind.CommaToken);
          seps.Add(commaTok);
          if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob)) break; // trailing
          continue;
        }
        break;
      }
    }

    StructuralArray<UnionVariantDeclaration> arr = [.. vars];
    StructuralArray<GreenToken> sepArr = [.. seps];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new UnionDeclaration(name, colon, openUnion, arr, sepArr, close, term);
  }

  static NamedTupleDeclaration ParseGenericNamedTupleBody(Parser p, DeclarationName name, GreenToken colon)
  {
    GreenToken open = p.ExpectSyntax(SyntaxKind.NamedTupleToken);
    ImmutableArray<GreenNode>.Builder elems = ImmutableArray.CreateBuilder<GreenNode>();
    ImmutableArray<GreenToken>.Builder seps = ImmutableArray.CreateBuilder<GreenToken>();
    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        GreenToken typeNameTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
        GreenNode ty = typeNameTok;
        if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
        {
          ty = GenericType.ParseAfterName(p, typeNameTok);
        }
        elems.Add(ty);
        if (p.MatchRaw(RawKind.Comma))
        {
          GreenToken commaTok = p.ExpectSyntax(SyntaxKind.CommaToken);
          seps.Add(commaTok);
          if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob)) break; // trailing
          continue;
        }
        break;
      }
    }

    if (elems.Count == 0)
      p.AddDiagEmptyNamedTupleElementList();

    StructuralArray<GreenNode> arr = [.. elems];
    StructuralArray<GreenToken> sepArr = [.. seps];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new NamedTupleDeclaration(name, colon, open, arr, sepArr, close, term);
  }
}
