namespace Brim.Parse.Green;

public sealed record ImportDeclaration(
  GreenToken Identifier,
  GreenToken BindToken,
  ModulePath Path,
  GreenToken Terminator) :
GreenNode(SyntaxKind.ImportDeclaration, Identifier.Offset),
IParsable<ImportDeclaration>
{
  public override int FullWidth => Terminator.EndOffset - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    yield return BindToken;
    yield return Path;
    yield return Terminator;
  }

  // EBNF: ImportDecl ::= Identifier '::=' ModulePath Terminator
  public static ImportDeclaration Parse(Parser p) => new(
    p.Expect(SyntaxKind.IdentifierToken),
    p.Expect(SyntaxKind.ModuleBindToken),
    ModulePath.Parse(p),
    p.Expect(SyntaxKind.TerminatorToken)
  );
}
