namespace Brim.Parse.Green;

public sealed record NamedTupleDeclaration(
  DeclarationName Name,
  GreenToken Colon,
  GreenToken OpenToken, // #{ token
  StructuralArray<GreenNode> ElementTypes,
  StructuralArray<GreenToken> ElementSeparators,
  GreenToken CloseBrace,
  GreenToken Terminator)
  : GreenNode(SyntaxKind.NamedTupleDeclaration, Name.Offset)
{
  public override int FullWidth => Terminator.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    yield return OpenToken;
    for (int i = 0; i < ElementTypes.Count; i++)
    {
      yield return ElementTypes[i];
      if (i < ElementSeparators.Count)
        yield return ElementSeparators[i];
    }
    yield return CloseBrace;
    yield return Terminator;
  }

  // EBNF (updated): NamedTupleDecl ::= Identifier GenericParams? ':' NamedTupleToken TypeRef (',' TypeRef)* (',')? '}' Terminator
  // No zero-tuples: must contain at least one TypeRef.
  public static NamedTupleDeclaration Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.NamedTupleToken); // #{

    ImmutableArray<GreenNode>.Builder elems = ImmutableArray.CreateBuilder<GreenNode>();
    ImmutableArray<GreenToken>.Builder seps = ImmutableArray.CreateBuilder<GreenToken>();

    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        int before = p.Current.CoreToken.Offset;
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
          if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob))
            break; // trailing comma
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
