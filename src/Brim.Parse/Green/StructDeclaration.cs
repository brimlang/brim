namespace Brim.Parse.Green;

public sealed record StructDeclaration(
  DeclarationName Name,
  GreenToken Colon,
  GreenToken StructOpen,
  StructuralArray<FieldDeclaration> Fields,
  GreenToken Close,
  GreenToken Terminator) :
GreenNode(SyntaxKind.StructDeclaration, Name.Offset),
IParsable<StructDeclaration>
{
  public override int FullWidth => Terminator.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    yield return StructOpen;
    foreach (FieldDeclaration field in Fields) yield return field;
    yield return Close;
    yield return Terminator;
  }

  // EBNF: StructDecl ::= Identifier GenericParams? ':' StructToken FieldDecl*(',' FieldDecl)* '}' Terminator
  public static StructDeclaration Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
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
        name,
        colon,
        open,
        fieldArray,
        close,
        term);
  }
}
