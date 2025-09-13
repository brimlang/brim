namespace Brim.Parse.Green;

public sealed record FieldDeclaration(
  GreenToken Identifier,
  GreenToken Colon,
  GreenNode TypeAnnotation,
  GreenToken? TrailingComma) :
GreenNode(SyntaxKind.FieldDeclaration, Identifier.Offset),
IParsable<FieldDeclaration>
{
  public override int FullWidth => (TrailingComma?.EndOffset ?? TypeAnnotation.EndOffset) - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    yield return Colon;
    yield return TypeAnnotation;
    if (TrailingComma is not null) yield return TrailingComma;
  }

  public static FieldDeclaration Parse(Parser p)
  {
    GreenToken nameTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken typeNameTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);

    GreenNode type = typeNameTok;
    if (p.MatchRaw(RawKind.LBracket))
    {
      type = GenericType.ParseAfterName(p, typeNameTok);
    }

    GreenToken? trailing = null;
    if (p.MatchRaw(RawKind.Comma))
    {
      trailing = p.ExpectSyntax(SyntaxKind.CommaToken);
    }

    return new FieldDeclaration(nameTok, colon, type, trailing);
  }
}

