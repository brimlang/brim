namespace Brim.Parse.Green;

/// <summary>
/// Represents a list/sequence pattern (p1, p2, ..rest) that matches sequence elements positionally.
/// </summary>
public sealed record ListPattern(
  GreenToken OpenToken,
  ListElements? Elements,
  GreenToken CloseToken)
  : PatternNode(SyntaxKind.ListPattern, OpenToken.Offset)
{
  public override int FullWidth => CloseToken.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return OpenToken;
    if (Elements is not null)
      yield return Elements;
    yield return CloseToken;
  }

  internal static new ListPattern Parse(Parser parser)
  {
    GreenToken open = parser.Expect(RawKind.LParen, SyntaxKind.OpenParenToken);

    ListElements? elements = null;
    if (!parser.Match(RawKind.RParen))
    {
      elements = ListElements.Parse(parser);
    }

    GreenToken close = parser.Expect(RawKind.RParen, SyntaxKind.CloseParenToken);

    return new ListPattern(open, elements, close);
  }
}
