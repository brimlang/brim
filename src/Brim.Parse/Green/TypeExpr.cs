namespace Brim.Parse.Green;

/// <summary>
/// Entry for parsing a type expression (no value expressions).
/// </summary>
public static class TypeExpr
{
  public static GreenNode Parse(Parser p)
  {
    // Primary by first token
    GreenNode primary;
    if (p.MatchRaw(RawKind.PercentLBrace))
    {
      primary = StructShape.Parse(p);
    }
    else if (p.MatchRaw(RawKind.PipeLBrace))
    {
      primary = UnionShape.Parse(p);
    }
    else if (p.MatchRaw(RawKind.HashLBrace))
    {
      primary = NamedTupleShape.Parse(p);
    }
    else if (p.MatchRaw(RawKind.Ampersand))
    {
      primary = FlagsShape.Parse(p);
    }
    else if (p.MatchRaw(RawKind.LParen))
    {
      primary = FunctionType.Parse(p);
    }
    else
    {
      GreenToken head = p.ExpectSyntax(SyntaxKind.IdentifierToken);
      primary = head;
      if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
        primary = GenericType.ParseAfterName(p, head);
    }

    // Single postfix operator: '?' or '!'
    if (p.MatchRaw(RawKind.Question))
    {
      GreenToken q = p.ExpectSyntax(SyntaxKind.QuestionToken);
      return new OptionType(primary, q);
    }
    if (p.MatchRaw(RawKind.Bang))
    {
      GreenToken b = p.ExpectSyntax(SyntaxKind.BangToken);
      return new ResultType(primary, b);
    }

    return primary;
  }
}
