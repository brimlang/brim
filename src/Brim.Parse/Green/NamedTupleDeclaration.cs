namespace Brim.Parse.Green;

public sealed record NamedTupleDeclaration(
  DeclarationName Name,
  GreenToken Colon,
  GreenToken OpenToken, // #{ token
  StructuralArray<GreenNode> ElementTypes,
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
    foreach (GreenNode t in ElementTypes) yield return t;
    yield return CloseBrace;
    yield return Terminator;
  }

  // EBNF: NamedTupleDecl ::= Identifier GenericParams? ':' NamedTupleToken TypeRef (',' TypeRef)* '}' Terminator
  // No zero-tuples: must contain at least one TypeRef.
  public static NamedTupleDeclaration Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
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
        GreenToken typeNameTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
        GreenNode ty = typeNameTok;
        if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
        {
          ty = GenericType.ParseAfterName(p, typeNameTok);
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

    return new NamedTupleDeclaration(name, colon, open, arr, close, term);
  }
}
