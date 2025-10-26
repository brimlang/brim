namespace Brim.Parse.Green;

/// <summary>
/// Represents an entry in a flags pattern - either a signed flag (+flag/-flag) or a bare identifier.
/// </summary>
public sealed record FlagsPatternEntry(GreenNode Entry)
  : GreenNode(SyntaxKind.FlagsPatternEntry, Entry.Offset)
{
  public override int FullWidth => Entry.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Entry;
  }

  internal static FlagsPatternEntry Parse(Parser parser)
  {
    GreenNode entry;

    if (parser.Match(RawKind.Plus) || parser.Match(RawKind.Minus))
    {
      // Signed flag: +flag or -flag
      entry = SignedFlag.Parse(parser);
    }
    else
    {
      // Bare identifier
      entry = parser.Expect(RawKind.Identifier, SyntaxKind.IdentifierToken);
    }

    return new FlagsPatternEntry(entry);
  }
}
