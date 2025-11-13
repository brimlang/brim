namespace Brim.Parse.Green;

/// <summary>
/// Represents a field pattern (field = pattern) within a struct pattern.
/// </summary>
public sealed record FieldPattern(
  GreenToken FieldName,
  GreenToken BindToken,
  PatternNode Pattern)
  : GreenNode(SyntaxKind.FieldPattern, FieldName.Offset)
{
  public override int FullWidth => Pattern.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return FieldName;
    yield return BindToken;
    yield return Pattern;
  }

  internal static FieldPattern Parse(Parser parser)
  {
    GreenToken fieldName = parser.Expect(SyntaxKind.IdentifierToken);
    GreenToken bindToken = parser.Expect(SyntaxKind.EqualToken);
    PatternNode pattern = PatternNode.Parse(parser);

    return new FieldPattern(fieldName, bindToken, pattern);
  }
}
