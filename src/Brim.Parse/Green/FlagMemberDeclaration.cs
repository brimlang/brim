namespace Brim.Parse.Green;

public sealed record FlagMemberDeclaration(
  GreenToken Identifier) :
  GreenNode(SyntaxKind.FlagMemberDeclaration, Identifier.Offset)
{
  public override int FullWidth => Identifier.FullWidth;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
  }

  public static FlagMemberDeclaration Parse(Parser p) => new(p.ExpectSyntax(SyntaxKind.IdentifierToken));
}
