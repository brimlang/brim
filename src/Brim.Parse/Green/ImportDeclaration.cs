namespace Brim.Parse.Green;

public sealed record ImportDeclaration(
  GreenToken Identifier,
  GreenToken Equal,
  ModuleHeader ModuleHeader,
  GreenToken Terminator) :
GreenNode(SyntaxKind.ImportDeclaration, Identifier.Offset),
IParsable<ImportDeclaration>
{
  public override int FullWidth => ModuleHeader.EndOffset - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    yield return Equal;
    yield return ModuleHeader;
    yield return Terminator;
  }

  // EBNF: ImportDecl ::= Identifier '=' ModuleHeader Terminator
  public static ImportDeclaration Parse(Parser p) => new(
    p.ExpectSyntax(SyntaxKind.IdentifierToken),
    p.ExpectSyntax(SyntaxKind.EqualToken),
    ModuleHeader.Parse(p),
    p.ExpectSyntax(SyntaxKind.TerminatorToken)
  );
}
