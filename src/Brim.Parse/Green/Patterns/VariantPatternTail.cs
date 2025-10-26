namespace Brim.Parse.Green;

/// <summary>
/// Represents the optional payload tail of a variant pattern: ParenListOpt<Pattern>
/// Used in variant patterns like |(Good(v)) or |(Error(msg))
/// </summary>
public sealed record VariantPatternTail(CommaList<PatternNode> Patterns)
  : GreenNode(SyntaxKind.VariantPatternTail, Patterns.Offset)
{
  public override int FullWidth => Patterns.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Patterns;
  }

  internal static VariantPatternTail Parse(Parser parser)
  {
    CommaList<PatternNode> patterns = CommaList<PatternNode>.Parse(
        parser,
        SyntaxKind.OpenParenToken,
        SyntaxKind.CloseParenToken,
        PatternNode.Parse);

    return new VariantPatternTail(patterns);
  }
}
