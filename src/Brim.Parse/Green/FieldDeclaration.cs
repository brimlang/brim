namespace Brim.Parse.Green;

public sealed record FieldDeclaration(
  Identifier Identifier,
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
    Identifier name = Identifier.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    Identifier typeName = Identifier.Parse(p);
    GreenNode type = typeName;
    if (p.Match(RawTokenKind.LBracket) && !p.Match(RawTokenKind.LBracketLBracket))
    {
      type = GenericType.ParseAfterName(p, typeName);
    }
    return new FieldDeclaration(name, colon, type);
  }
}

