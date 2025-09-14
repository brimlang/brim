namespace Brim.Parse.Green;

public sealed record NamedTupleElement(
  GreenNode TypeNode,
  GreenToken? TrailingComma) : GreenNode(SyntaxKind.NamedTupleElement, TypeNode.Offset)
{
  public override int FullWidth => (TrailingComma?.EndOffset ?? TypeNode.EndOffset) - TypeNode.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return TypeNode;
    if (TrailingComma is not null) yield return TrailingComma;
  }
}

public sealed record NamedTupleDeclaration(
  DeclarationName Name,
  GreenToken TypeBind,
  GreenToken OpenToken, // #{ token
  StructuralArray<NamedTupleElement> Elements,
  GreenToken CloseBrace,
  GreenToken Terminator)
  : GreenNode(SyntaxKind.NamedTupleDeclaration, Name.Offset)
{
  public override int FullWidth => Terminator.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return TypeBind;
    yield return OpenToken;
    foreach (NamedTupleElement e in Elements) yield return e;
    yield return CloseBrace;
    yield return Terminator;
  }

  // EBNF (updated): NamedTupleDecl ::= Identifier GenericParams? ':=' NamedTupleToken TypeRef (',' TypeRef)* (',')? '}' Terminator
  // No zero-tuples: must contain at least one TypeRef.
  public static NamedTupleDeclaration Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.TypeBindToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.NamedTupleToken); // #{

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
        if (p.MatchRaw(RawKind.Comma))
        {
          trailing = p.ExpectSyntax(SyntaxKind.CommaToken);
        }
        elems.Add(new NamedTupleElement(ty, trailing));
        if (trailing is null) break; // no comma -> end
        if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob)) break; // trailing comma
        continue; // expect another element
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
