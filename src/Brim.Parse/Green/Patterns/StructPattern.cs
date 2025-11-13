namespace Brim.Parse.Green;

/// <summary>
/// Represents a struct pattern %(field = p, ...) that matches struct values by field name.
/// </summary>
public sealed record StructPattern(CommaList<FieldPattern> FieldPatterns)
  : PatternNode(SyntaxKind.StructPattern, FieldPatterns.Offset)
{
  public override int FullWidth => FieldPatterns.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return FieldPatterns;
  }

  internal static new StructPattern Parse(Parser parser)
  {
    CommaList<FieldPattern> fieldPatterns = CommaList<FieldPattern>.Parse(
        parser,
        SyntaxKind.StructToken,
        SyntaxKind.CloseParenToken,
        FieldPattern.Parse);

    return new StructPattern(fieldPatterns);
  }
}
