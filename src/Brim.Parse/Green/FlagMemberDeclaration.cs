namespace Brim.Parse.Green;

public sealed record FlagMemberDeclaration(
  GreenToken Identifier,
  GreenToken? TrailingComma) :
GreenNode(SyntaxKind.FlagMemberDeclaration, Identifier.Offset)
{
  public override int FullWidth => (TrailingComma?.EndOffset ?? Identifier.EndOffset) - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    if (TrailingComma is not null) yield return TrailingComma;
  }

  public static FlagMemberDeclaration Parse(Parser p)
  {
    GreenToken nameTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken? trailing = null;
    if (p.MatchRaw(RawKind.Comma)) trailing = p.ExpectSyntax(SyntaxKind.CommaToken);
    return new FlagMemberDeclaration(nameTok, trailing);
  }
}
