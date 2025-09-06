namespace Brim.Parse.Green;

public sealed record StructDeclaration(
  Identifier Identifier,
  GenericParameterList? GenericParams,
  GreenToken Equal,
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
    yield return Equal;
    yield return StructOpen;
    foreach (FieldDeclaration field in Fields) yield return field;
    yield return Close;
    yield return Terminator;
  }

  static GenericParameterList? TryParseGenericParams(Parser p)
  {
    if (!p.Match(RawTokenKind.LBracket)) return null; // must not be module path
    if (p.Match(RawTokenKind.LBracketLBracket)) return null; // module path head
    GreenToken open = p.ExpectSyntax(SyntaxKind.GenericOpenToken);
    ImmutableArray<Identifier>.Builder items = ImmutableArray.CreateBuilder<Identifier>();
    bool first = true;
    while (!p.Match(RawTokenKind.RBracket) && !p.Match(RawTokenKind.Eob))
    {
      if (!first)
      {
        if (p.Match(RawTokenKind.Comma)) _ = p.Expect(RawTokenKind.Comma); else break;
      }
      items.Add(Identifier.Parse(p));
      first = false;
    }
    GreenToken close = p.ExpectSyntax(SyntaxKind.GenericCloseToken);
    return new GenericParameterList(open, items.ToImmutable(), close);
  }

  // EBNF: StructDecl ::= Identifier GenericParams? '=' StructToken FieldDecl*("," FieldDecl)* '}' Terminator
  public static StructDeclaration Parse(Parser p)
  {
    Identifier id = Identifier.Parse(p);
    GenericParameterList? gp = TryParseGenericParams(p);
    GreenToken eq = p.ExpectSyntax(SyntaxKind.EqualToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.StructToken);

    ImmutableArray<FieldDeclaration>.Builder fields = ImmutableArray.CreateBuilder<FieldDeclaration>();
    while (!p.Match(RawTokenKind.RBrace) && !p.Match(RawTokenKind.Eob))
    {
      int before = p.Current.CoreToken.Offset;
      fields.Add(FieldDeclaration.Parse(p));
      if (p.Match(RawTokenKind.Comma))
        _ = p.Expect(RawTokenKind.Comma);
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
        eq,
        open,
        fieldArray,
        close,
        term);
  }
}

