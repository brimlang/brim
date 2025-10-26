namespace Brim.Parse.Green;

/// <summary>
/// Represents a tuple pattern #(p1, p2, ...) that matches tuple values positionally.
/// </summary>
public sealed record TuplePattern(CommaList<PatternNode> Patterns)
  : PatternNode(SyntaxKind.TuplePattern, Patterns.Offset)
{
    public override int FullWidth => Patterns.FullWidth;

    public override IEnumerable<GreenNode> GetChildren()
    {
        yield return Patterns;
    }

    internal static new TuplePattern Parse(Parser parser)
    {
        CommaList<PatternNode> patterns = CommaList<PatternNode>.Parse(
            parser,
            SyntaxKind.NamedTupleToken,
            SyntaxKind.CloseParenToken,
            PatternNode.Parse);

        return new TuplePattern(patterns);
    }
}
