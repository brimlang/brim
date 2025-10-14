namespace Brim.Parse.Green;

public sealed record FieldDeclaration(
  GreenToken Identifier,
  GreenToken Colon,
  TypeExpr TypeExpr) :
GreenNode(SyntaxKind.FieldDeclaration, Identifier.Offset),
IParsable<FieldDeclaration>
{
  public override int FullWidth => TypeExpr.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    yield return Colon;
    yield return TypeExpr;
  }

  public static FieldDeclaration Parse(Parser p)
  {
    GreenToken nameTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    TypeExpr type = TypeExpr.Parse(p);

    return new FieldDeclaration(nameTok, colon, type);
  }
}

