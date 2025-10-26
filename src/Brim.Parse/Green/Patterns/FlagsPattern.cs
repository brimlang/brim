namespace Brim.Parse.Green;

/// <summary>
/// Represents a flags pattern &(flag1, flag2, ...) that matches flag set values.
/// Supports exact matching &(read, write) or constrained matching &(+read, -exec).
/// </summary>
public sealed record FlagsPattern(CommaList<FlagsPatternEntry> Entries)
  : PatternNode(SyntaxKind.FlagsPattern, Entries.Offset)
{
    public override int FullWidth => Entries.FullWidth;

    public override IEnumerable<GreenNode> GetChildren()
    {
        yield return Entries;
    }

    internal static new FlagsPattern Parse(Parser parser)
    {
        CommaList<FlagsPatternEntry> entries = CommaList<FlagsPatternEntry>.Parse(
            parser,
            SyntaxKind.FlagsToken,
            SyntaxKind.CloseParenToken,
            FlagsPatternEntry.Parse);

        return new FlagsPattern(entries);
    }
}
