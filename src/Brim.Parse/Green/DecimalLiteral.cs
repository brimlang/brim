namespace Brim.Parse.Green;

public sealed record DecimalLiteral(
  GreenToken Token)
: GreenNode(SyntaxKind.DecimalToken, Token.Offset)
, IParsable<DecimalLiteral>
{
  public override int FullWidth => Token.FullWidth;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Token;
  }

  public static DecimalLiteral Parse(Parser p)
  {
    GreenToken token = p.ExpectSyntax(SyntaxKind.DecimalToken);
    return new DecimalLiteral(token);
  }
}

