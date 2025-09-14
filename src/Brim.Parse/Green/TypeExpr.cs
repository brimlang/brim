namespace Brim.Parse.Green;

/// <summary>
/// Entry for parsing a type expression (no value expressions).
/// </summary>
public static class TypeExpr
{
  public static GreenNode Parse(Parser p)
  {
    // Prediction by first meaningful token
    if (p.MatchRaw(RawKind.PercentLBrace)) return StructShape.Parse(p);
    if (p.MatchRaw(RawKind.PipeLBrace)) return UnionShape.Parse(p);
    if (p.MatchRaw(RawKind.HashLBrace)) return NamedTupleShape.Parse(p);
    if (p.MatchRaw(RawKind.Ampersand)) return FlagsShape.Parse(p);

    // list[T] — reserved for later; treat as identifier for now
    GreenToken head = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenNode node = head;
    if (p.MatchRaw(RawKind.LBracket))
    {
      // either generic args or module path open — prevent consuming '[[ '
      if (!p.MatchRaw(RawKind.LBracketLBracket))
        node = GenericType.ParseAfterName(p, head);
    }
    return node;
  }
}

