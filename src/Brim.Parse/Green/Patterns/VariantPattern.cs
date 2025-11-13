namespace Brim.Parse.Green;

/// <summary>
/// Represents a variant pattern |(Variant) or |(Variant(p)) that matches union variant values.
/// </summary>
public sealed record VariantPattern(
  GreenToken OpenToken,
  GreenToken VariantName,
  VariantPatternTail? Tail,
  GreenToken CloseToken)
  : PatternNode(SyntaxKind.VariantPattern, OpenToken.Offset)
{
  public override int FullWidth => CloseToken.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return OpenToken;
    yield return VariantName;
    if (Tail is not null)
      yield return Tail;
    yield return CloseToken;
  }

  internal static new VariantPattern Parse(Parser parser)
  {
    GreenToken open = parser.Expect(SyntaxKind.UnionToken);
    GreenToken variantName = parser.Expect(SyntaxKind.IdentifierToken);

    VariantPatternTail? tail = null;
    if (parser.Match(TokenKind.LParen))
    {
      tail = VariantPatternTail.Parse(parser);
    }

    GreenToken close = parser.Expect(SyntaxKind.CloseParenToken);

    return new VariantPattern(open, variantName, tail, close);
  }
}
