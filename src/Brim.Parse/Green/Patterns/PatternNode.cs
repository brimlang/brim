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
    if (parser.Current.TokenKind == TokenKind.Question && parser.Match(TokenKind.LParen, 1))
    {
      return OptionalPattern.Parse(parser);
    }

    if (parser.Current.TokenKind == TokenKind.Bang && parser.Match(TokenKind.LParen, 1))
    {
      return FalliblePattern.Parse(parser);
    }

    if (parser.Current.TokenKind == TokenKind.Bang && parser.Match(TokenKind.Bang, 1) && parser.Match(TokenKind.LParen, 2))
    {
      return FalliblePattern.Parse(parser);
    }

    return parser.Current.TokenKind switch
    {
      TokenKind.Identifier => BindingPattern.Parse(parser),
      TokenKind.IntegerLiteral or TokenKind.DecimalLiteral or TokenKind.StringLiteral or TokenKind.RuneLiteral => LiteralPattern.Parse(parser),
      TokenKind.HashLParen => TuplePattern.Parse(parser),
      TokenKind.PercentLParen => StructPattern.Parse(parser),
      TokenKind.PipeLParen => VariantPattern.Parse(parser),
      TokenKind.AmpersandLParen => FlagsPattern.Parse(parser),
      TokenKind.AtmarkLParen => ServicePattern.Parse(parser),
      TokenKind.LParen => ListPattern.Parse(parser),
      _ => BindingPattern.ParseError(parser)
    };
  }
}
