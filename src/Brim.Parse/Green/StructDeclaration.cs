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
    foreach (FieldDeclaration f in Fields) yield return f;
    yield return Close;
    yield return Terminator;
  }

  // EBNF (updated): StructDecl ::= Identifier GenericParams? ':' StructToken FieldDecl (',' FieldDecl)* (',')? '}' Terminator
  public static StructDeclaration Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.StructToken);

    ImmutableArray<FieldDeclaration>.Builder fields = ImmutableArray.CreateBuilder<FieldDeclaration>();

    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        FieldDeclaration field = FieldDeclaration.Parse(p);
        fields.Add(field);
        if (field.TrailingComma is null) break; // no comma means end
        if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob)) break; // trailing comma
        continue; // expect another field
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
