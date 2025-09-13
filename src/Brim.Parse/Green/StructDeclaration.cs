namespace Brim.Parse.Green;

public sealed record StructDeclaration(
  Identifier Identifier,
  GenericParameterList? GenericParams,
  GreenToken Colon,
  GreenToken StructOpen,
  StructuralArray<FieldDeclaration> Fields,
  GreenToken Close,
  GreenToken Terminator) :
GreenNode(SyntaxKind.StructDeclaration, Identifier.Offset),
IParsable<StructDeclaration>
{
  public override int FullWidth => Terminator.EndOffset - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    if (GenericParams is not null) yield return GenericParams;
    yield return Colon;
    yield return StructOpen;
    foreach (FieldDeclaration field in Fields) yield return field;
    yield return Close;
    yield return Terminator;
  }

  static GenericParameterList? TryParseGenericParams(Parser p)
  {
    if (!p.MatchRaw(RawKind.LBracket)) return null; // must not be module path
    GreenToken open = p.ExpectSyntax(SyntaxKind.GenericOpenToken);

    ImmutableArray<Identifier>.Builder items = ImmutableArray.CreateBuilder<Identifier>();
    bool first = true;
    while (!p.MatchRaw(RawKind.RBracket) && !p.MatchRaw(RawKind.Eob))
    {
      if (!first)
      {
        if (p.MatchRaw(RawKind.Comma)) _ = p.ExpectRaw(RawKind.Comma); else break;
      }
      items.Add(Identifier.Parse(p));
      first = false;
    }

    GreenToken close = p.ExpectSyntax(SyntaxKind.GenericCloseToken);
    return new GenericParameterList(open, items.ToImmutable(), close);
  }

  // EBNF: StructDecl ::= Identifier GenericParams? ':' StructToken FieldDecl*("," FieldDecl)* '}' Terminator
  public static StructDeclaration Parse(Parser p)
  {
    Identifier id = Identifier.Parse(p);
    GenericParameterList? gp = TryParseGenericParams(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.StructToken);

    ImmutableArray<FieldDeclaration>.Builder fields = ImmutableArray.CreateBuilder<FieldDeclaration>();
    while (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      int before = p.Current.CoreToken.Offset;
      fields.Add(FieldDeclaration.Parse(p));
      if (p.MatchRaw(RawKind.Comma))
        _ = p.ExpectRaw(RawKind.Comma);
      if (p.Current.CoreToken.Offset == before)
      {
        // Parsing this field made no progress (likely fabricated tokens). Break to prevent infinite loop.
        break;
      }
    }

    StructuralArray<FieldDeclaration> fieldArray = [.. fields];

    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);

    return new(
        id,
        gp,
        colon,
        open,
        fieldArray,
        close,
        term);
  }
}

