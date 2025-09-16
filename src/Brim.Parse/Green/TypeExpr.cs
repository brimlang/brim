namespace Brim.Parse.Green;

/// <summary>
/// Entry for parsing a type expression (no value expressions).
/// </summary>
public static class TypeExpr
{
  public static GreenNode Parse(Parser p)
  {
    Opt<ParseFunc> shapeParser = MapShapeParser(p.Current);
    GreenNode primary = shapeParser.HasValue
      ? shapeParser.Value(p)
      : Fallback(p);

    return p.Current.Kind switch
    {
      RawKind.Question => new OptionType(primary, p.ExpectSyntax(SyntaxKind.QuestionToken)),
      RawKind.Bang => new ResultType(primary, p.ExpectSyntax(SyntaxKind.BangToken)),
      _ => primary,
    };

    static Opt<ParseFunc> MapShapeParser(TokenView tok) => tok.Kind switch
    {
      RawKind.PercentLBrace => new(StructShape.Parse),
      RawKind.PipeLBrace => new(UnionShape.Parse),
      RawKind.HashLBrace => new(NamedTupleShape.Parse),
      RawKind.Ampersand => new(FlagsShape.Parse),
      RawKind.LParen => new(FunctionShape.Parse),
      RawKind.StopLBrace => new(ProtocolShape.Parse),
      RawKind.Hat => new(ServiceShape.Parse),
      _ => Opt<ParseFunc>.None,
    };

    static GreenNode Fallback(Parser p)
    {
      GreenNode primary;
      GreenToken head = p.ExpectSyntax(SyntaxKind.IdentifierToken);
      primary = head;
      if (p.MatchRaw(RawKind.LBracket))
        primary = GenericType.ParseAfterName(p, head);

      return primary;
    }
  }
}
