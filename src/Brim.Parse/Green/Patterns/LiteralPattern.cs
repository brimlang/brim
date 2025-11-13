namespace Brim.Parse.Green;

/// <summary>
/// Represents a literal pattern that matches exact literal values (integers, decimals, strings, runes, booleans).
/// </summary>
public sealed record LiteralPattern(GreenToken Literal)
  : PatternNode(SyntaxKind.LiteralPattern, Literal.Offset)
{
  public override int FullWidth => Literal.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Literal;
  }

  internal static new LiteralPattern Parse(Parser parser)
  {
    GreenToken literal = parser.Current.TokenKind switch
    {
      TokenKind.IntegerLiteral => parser.Expect(SyntaxKind.IntToken),
      TokenKind.DecimalLiteral => parser.Expect(SyntaxKind.DecimalToken),
      TokenKind.StringLiteral => parser.Expect(SyntaxKind.StrToken),
      TokenKind.RuneLiteral => parser.Expect(SyntaxKind.RuneToken),
      _ => parser.Expect(SyntaxKind.ErrorToken),
    };

    return new LiteralPattern(literal);
  }
}
