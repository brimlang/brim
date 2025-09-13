namespace Brim.Parse.Green;

public sealed record FieldDeclaration(
  GreenToken Identifier,
  GreenToken Colon,
  GreenNode TypeAnnotation) :
GreenNode(SyntaxKind.FieldDeclaration, Identifier.Offset),
IParsable<FieldDeclaration>
{
  public override int FullWidth => TypeAnnotation.EndOffset - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    yield return Colon;
    yield return TypeAnnotation;
  }

  public static FieldDeclaration Parse(Parser p)
  {
    GreenToken nameTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken typeNameTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);

    GreenNode type = typeNameTok;
    if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
    {
      type = GenericType.ParseAfterName(p, typeNameTok);
    }

    return new FieldDeclaration(nameTok, colon, type);
  }
}

