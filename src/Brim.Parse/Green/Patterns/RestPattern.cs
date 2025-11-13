namespace Brim.Parse.Green;

/// <summary>
/// Represents a rest pattern (..name or ..) that captures remaining elements in a list pattern.
/// </summary>
public sealed record RestPattern(
  GreenToken RestToken,
  GreenToken? Identifier)
  : GreenNode(SyntaxKind.RestPattern, RestToken.Offset)
{
  public override int FullWidth => (Identifier?.EndOffset ?? RestToken.EndOffset) - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return RestToken;
    if (Identifier is not null)
      yield return Identifier;
  }

  internal static RestPattern Parse(Parser parser)
  {
    GreenToken restToken = parser.Expect(SyntaxKind.StopToken);
    GreenToken? identifier = null;
    if (parser.Match(TokenKind.Identifier))
    {
      identifier = parser.Expect(SyntaxKind.IdentifierToken);
    }

    return new RestPattern(restToken, identifier);
  }
}
