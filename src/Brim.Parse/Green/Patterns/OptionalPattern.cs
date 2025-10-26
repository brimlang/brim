namespace Brim.Parse.Green;

/// <summary>
/// Represents an optional pattern ?(p) or ?() that matches option type values.
/// ?(p) matches Some(p), ?() matches None.
/// </summary>
public sealed record OptionalPattern(
  GreenToken QuestionToken,
  GreenToken OpenToken,
  PatternNode? Pattern,
  GreenToken CloseToken)
  : PatternNode(SyntaxKind.OptionalPattern, QuestionToken.Offset)
{
    public override int FullWidth => CloseToken.EndOffset - Offset;

    public override IEnumerable<GreenNode> GetChildren()
    {
        yield return QuestionToken;
        yield return OpenToken;
        if (Pattern is not null)
            yield return Pattern;
        yield return CloseToken;
    }

    internal static new OptionalPattern Parse(Parser parser)
    {
        GreenToken question = parser.Expect(RawKind.Question, SyntaxKind.QuestionToken);
        GreenToken open = parser.Expect(RawKind.LParen, SyntaxKind.OpenParenToken);

        PatternNode? pattern = null;
        if (!parser.Match(RawKind.RParen))
        {
            pattern = PatternNode.Parse(parser);
        }

        GreenToken close = parser.Expect(RawKind.RParen, SyntaxKind.CloseParenToken);

        return new OptionalPattern(question, open, pattern, close);
    }
}
