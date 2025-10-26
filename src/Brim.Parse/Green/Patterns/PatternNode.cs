namespace Brim.Parse.Green;

/// <summary>
/// Base class for all pattern nodes in the syntax tree.
/// </summary>
public abstract record PatternNode(SyntaxKind Kind, int Offset) : GreenNode(Kind, Offset)
{
  public static PatternNode Parse(Parser parser)
  {
    // Check for underscore identifier (wildcard pattern)
    // Note: wildcards will be distinguished from bindings during semantic analysis
    // For now, both parse as identifiers and pattern matching determines usage

    // Handle ? and ! sequences that need lookahead
    if (parser.Current.Kind == RawKind.Question && parser.MatchRaw(RawKind.LParen, 1))
    {
      return OptionalPattern.Parse(parser);
    }

    if (parser.Current.Kind == RawKind.Bang && parser.MatchRaw(RawKind.LParen, 1))
    {
      return FalliblePattern.Parse(parser);
    }

    if (parser.Current.Kind == RawKind.Bang && parser.MatchRaw(RawKind.Bang, 1) && parser.MatchRaw(RawKind.LParen, 2))
    {
      return FalliblePattern.Parse(parser);
    }

    return parser.Current.Kind switch
    {
      RawKind.Identifier => BindingPattern.Parse(parser),
      RawKind.IntegerLiteral or RawKind.DecimalLiteral or RawKind.StringLiteral or RawKind.RuneLiteral => LiteralPattern.Parse(parser),
      RawKind.HashLParen => TuplePattern.Parse(parser),
      RawKind.PercentLParen => StructPattern.Parse(parser),
      RawKind.PipeLParen => VariantPattern.Parse(parser),
      RawKind.AmpersandLParen => FlagsPattern.Parse(parser),
      RawKind.AtmarkLParen => ServicePattern.Parse(parser),
      RawKind.LParen => ListPattern.Parse(parser),
      _ => BindingPattern.ParseError(parser)
    };
  }
}
