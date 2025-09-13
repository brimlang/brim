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
    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        FieldDeclaration field = FieldDeclaration.Parse(p);
        fields.Add(field);
        if (field.TrailingComma is null) break;
        if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob)) break; // trailing comma
        continue;
      }
    }

    StructuralArray<FieldDeclaration> arr = [.. fields];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new StructDeclaration(name, colon, openStruct, arr, close, term);
  }

  static UnionDeclaration ParseGenericUnionBody(Parser p, DeclarationName name, GreenToken colon)
  {
    GreenToken openUnion = p.ExpectSyntax(SyntaxKind.UnionToken);
    ImmutableArray<UnionVariantDeclaration>.Builder vars = ImmutableArray.CreateBuilder<UnionVariantDeclaration>();
    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        UnionVariantDeclaration variant = UnionVariantDeclaration.Parse(p);
        vars.Add(variant);
        if (variant.TrailingComma is null) break;
        if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob)) break; // trailing
        continue;
      }
    }

    StructuralArray<UnionVariantDeclaration> arr = [.. vars];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new UnionDeclaration(name, colon, openUnion, arr, close, term);
  }

  static NamedTupleDeclaration ParseGenericNamedTupleBody(Parser p, DeclarationName name, GreenToken colon)
  {
    GreenToken open = p.ExpectSyntax(SyntaxKind.NamedTupleToken);
    ImmutableArray<NamedTupleElement>.Builder elems = ImmutableArray.CreateBuilder<NamedTupleElement>();
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
        GreenToken? trailing = null;
        if (p.MatchRaw(RawKind.Comma)) trailing = p.ExpectSyntax(SyntaxKind.CommaToken);
        elems.Add(new NamedTupleElement(ty, trailing));
        if (trailing is null) break;
        if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob)) break; // trailing
        continue;
      }
    }

    if (elems.Count == 0)
      p.AddDiagEmptyNamedTupleElementList();

    StructuralArray<NamedTupleElement> arr = [.. elems];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new NamedTupleDeclaration(name, colon, open, arr, close, term);
  }
}
