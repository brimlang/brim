namespace Brim.Parse.Green;

public sealed record NamedTupleDeclaration(
  Identifier Identifier,
  GenericParameterList? GenericParams,
  GreenToken Colon,
  GreenToken OpenToken, // #{ token
  StructuralArray<GreenNode> ElementTypes,
  GreenToken CloseBrace,
  GreenToken Terminator)
  : GreenNode(SyntaxKind.NamedTupleDeclaration, Identifier.Offset)
{
  public override int FullWidth => Terminator.EndOffset - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    if (GenericParams is not null) yield return GenericParams;
    yield return Colon;
    yield return OpenToken;
    foreach (GreenNode t in ElementTypes) yield return t;
    yield return CloseBrace;
    yield return Terminator;
  }

  static GenericParameterList? TryParseGenericParams(Parser p)
  {
    if (!p.MatchRaw(RawKind.LBracket)) return null;
    GreenToken open = p.ExpectSyntax(SyntaxKind.GenericOpenToken);
    ImmutableArray<Identifier>.Builder items = ImmutableArray.CreateBuilder<Identifier>();
    bool first = true;
    bool empty = false;
    if (p.MatchRaw(RawKind.RBracket))
    {
      empty = true; // record emptiness for diag
    }
    else
    {
      while (!p.MatchRaw(RawKind.RBracket) && !p.MatchRaw(RawKind.Eob))
      {
        int before = p.Current.CoreToken.Offset;
        if (!first)
        {
          if (p.MatchRaw(RawKind.Comma)) _ = p.ExpectRaw(RawKind.Comma); else break;
        }
        items.Add(Identifier.Parse(p));
        first = false;
        if (p.Current.CoreToken.Offset == before) break; // no progress
      }
    }
    GreenToken close = p.ExpectSyntax(SyntaxKind.GenericCloseToken);
    if (empty) p.AddDiagEmptyGeneric(open);
    return new GenericParameterList(open, items.ToImmutable(), close);
  }

  // EBNF: NamedTupleDecl ::= Identifier GenericParams? ':' NamedTupleToken TypeRef (',' TypeRef)* '}' Terminator
  // No zero-tuples: must contain at least one TypeRef.
  public static NamedTupleDeclaration Parse(Parser p)
  {
    Identifier id = Identifier.Parse(p);
    GenericParameterList? gp = TryParseGenericParams(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.NamedTupleToken); // #{

    ImmutableArray<GreenNode>.Builder elems = ImmutableArray.CreateBuilder<GreenNode>();

    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      bool first = true;
      while (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
      {
        int before = p.Current.CoreToken.Offset;
        if (!first)
        {
          if (p.MatchRaw(RawKind.Comma)) _ = p.ExpectRaw(RawKind.Comma); else break;
        }
        // For now TypeRef ::= Identifier GenericArgList?
        Identifier typeName = Identifier.Parse(p);
        GreenNode ty = typeName;
        if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
        {
          ty = GenericType.ParseAfterName(p, typeName);
        }
        elems.Add(ty);
        first = false;
        if (p.Current.CoreToken.Offset == before) break; // safety
      }
    }

    // Enforce non-empty named tuple list. If empty fabricate a missing element by emitting unexpected diagnostic.
    if (elems.Count == 0)
    {
      p.AddDiagEmptyNamedTupleElementList();
    }

    StructuralArray<GreenNode> arr = [.. elems];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);

    return new NamedTupleDeclaration(id, gp, colon, open, arr, close, term);
  }
}
