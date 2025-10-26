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
    GreenToken literal = parser.Current.Kind switch
    {
      RawKind.IntegerLiteral => parser.Expect(RawKind.IntegerLiteral, SyntaxKind.IntToken),
      RawKind.DecimalLiteral => parser.Expect(RawKind.DecimalLiteral, SyntaxKind.DecimalToken),
      RawKind.StringLiteral => parser.Expect(RawKind.StringLiteral, SyntaxKind.StrToken),
      RawKind.RuneLiteral => parser.Expect(RawKind.RuneLiteral, SyntaxKind.RuneToken),
      _ => parser.Expect(RawKind.Error, SyntaxKind.ErrorToken)
    };

    return new LiteralPattern(literal);
  }
}
