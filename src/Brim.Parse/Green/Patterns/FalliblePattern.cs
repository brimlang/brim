namespace Brim.Parse.Green;

/// <summary>
/// Represents a fallible pattern !(p) or !!(p) that matches result type values.
/// !(p) matches Ok(p), !!(p) matches Err(p).
/// </summary>
public sealed record FalliblePattern(
  GreenToken BangToken,
  GreenToken? SecondBangToken,
  GreenToken OpenToken,
  PatternNode? Pattern,
  GreenToken CloseToken)
  : PatternNode(SyntaxKind.FalliblePattern, BangToken.Offset)
{
  public override int FullWidth => CloseToken.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return BangToken;
    if (SecondBangToken is not null) yield return SecondBangToken;
    yield return OpenToken;
    if (Pattern is not null) yield return Pattern;
    yield return CloseToken;
  }

  internal static new FalliblePattern Parse(Parser parser)
  {
    GreenToken firstBang = parser.Expect(SyntaxKind.BangToken);

    GreenToken? secondBang = null;
    if (parser.Match(TokenKind.Bang))
    {
      secondBang = parser.Expect(SyntaxKind.BangToken);
    }

    GreenToken open = parser.Expect(SyntaxKind.OpenParenToken);

    PatternNode? pattern = null;
    // !(p) requires pattern, !!(p) allows optional pattern
    if (!parser.Match(TokenKind.RParen))
    {
      pattern = PatternNode.Parse(parser);
    }

    GreenToken close = parser.Expect(SyntaxKind.CloseParenToken);

    return new FalliblePattern(firstBang, secondBang, open, pattern, close);
  }
}
