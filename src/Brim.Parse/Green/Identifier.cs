namespace Brim.Parse.Green;

public sealed record Identifier(
  GreenToken Token)
: GreenNode(SyntaxKind.IdentifierToken, Token.Offset)
, IParsable<Identifier>
{
  public override int FullWidth => Token.FullWidth;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Token;
  }

  public static Identifier Parse(Parser p)
  {
    GreenToken token = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    return new Identifier(token);
  }
}

