namespace Brim.Parse.Green;

public sealed record TypeDeclaration(
  DeclarationName Name,
  GreenToken TypeBind,
  GreenNode TypeNode,
  GreenToken Terminator)
  : GreenNode(SyntaxKind.TypeDeclaration, Name.Offset)
{
  public override int FullWidth => Terminator.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return TypeBind;
    yield return TypeNode;
    yield return Terminator;
  }

  public static TypeDeclaration Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
    GreenToken bind = p.ExpectSyntax(SyntaxKind.TypeBindToken);
    GreenNode typeExpr = TypeExpr.Parse(p);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new TypeDeclaration(name, bind, typeExpr, term);
  }
}
