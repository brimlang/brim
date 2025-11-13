namespace Brim.Parse.Green;

public sealed record IntegerLiteral(
  GreenToken Token)
: GreenNode(SyntaxKind.IntToken, Token.Offset)
, IParsable<IntegerLiteral>
{
  public override int FullWidth => Token.FullWidth;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Token;
  }

  public static IntegerLiteral Parse(Parser p)
  {
    GreenToken token = p.Expect(SyntaxKind.IntToken);
    return new IntegerLiteral(token);
  }
}

