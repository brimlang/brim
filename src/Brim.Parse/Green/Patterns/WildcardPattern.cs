namespace Brim.Parse.Green;

/// <summary>
/// Represents a wildcard pattern (_) that matches any value without binding.
/// </summary>
public sealed record WildcardPattern(GreenToken Underscore)
  : PatternNode(SyntaxKind.WildcardPattern, Underscore.Offset)
{
  public override int FullWidth => Underscore.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Underscore;
  }

  internal static new WildcardPattern Parse(Parser parser)
  {
    GreenToken underscore = parser.Expect(SyntaxKind.IdentifierToken);
    return new WildcardPattern(underscore);
  }

  internal static PatternNode ParseError(Parser parser)
  {
    GreenToken error = parser.Expect(SyntaxKind.ErrorToken);
    return new WildcardPattern(error);
  }
}
