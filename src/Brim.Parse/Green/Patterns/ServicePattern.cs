namespace Brim.Parse.Green;

/// <summary>
/// Represents a service pattern @(name :Protocol, ...) that matches service instances
/// and binds protocol-typed handles.
/// </summary>
public sealed record ServicePattern(CommaList<ServicePatternEntry> Entries)
  : PatternNode(SyntaxKind.ServicePattern, Entries.Offset)
{
  public override int FullWidth => Entries.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Entries;
  }

  internal static new ServicePattern Parse(Parser parser)
  {
    CommaList<ServicePatternEntry> entries = CommaList<ServicePatternEntry>.Parse(
        parser,
        RawKind.AtmarkLParen,
        SyntaxKind.ServiceToken,
        RawKind.RParen,
        SyntaxKind.CloseParenToken,
        ServicePatternEntry.Parse);

    return new ServicePattern(entries);
  }
}
