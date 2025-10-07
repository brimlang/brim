namespace Brim.Parse.Green;

public sealed record FieldDeclaration(
  GreenToken Identifier,
  GreenToken Colon,
  TypeExpr TypeAnnotation,
  GreenToken? TrailingComma) :
GreenNode(SyntaxKind.FieldDeclaration, Identifier.Offset),
IParsable<FieldDeclaration>
{
  public override int FullWidth => (TrailingComma?.EndOffset ?? TypeAnnotation.EndOffset) - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    yield return Colon;
    foreach (GreenNode child in TypeAnnotation.GetChildren())
      yield return child;
    if (TrailingComma is not null) yield return TrailingComma;
  }

  public static FieldDeclaration Parse(Parser p)
  {
    GreenToken nameTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    TypeExpr type = TypeExpr.Parse(p);

    GreenToken? trailing = null;
    if (p.MatchRaw(RawKind.Comma))
    {
      trailing = p.ExpectSyntax(SyntaxKind.CommaToken);
    }

    return new FieldDeclaration(nameTok, colon, type, trailing);
  }
}

