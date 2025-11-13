namespace Brim.Parse.Green;

/// <summary>
/// Represents a signed flag entry (+flag or -flag) in a constrained flags pattern.
/// </summary>
public sealed record SignedFlag(
  GreenToken SignToken,
  GreenToken FlagName)
  : GreenNode(SyntaxKind.SignedFlag, SignToken.Offset)
{
  public override int FullWidth => FlagName.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return SignToken;
    yield return FlagName;
  }

  internal static SignedFlag Parse(Parser parser)
  {
    GreenToken sign = parser.Current.TokenKind switch
    {
      TokenKind.Plus => parser.Expect(SyntaxKind.PlusToken),
      TokenKind.Minus => parser.Expect(SyntaxKind.MinusToken),
      _ => parser.Expect(SyntaxKind.ErrorToken)
    };

    GreenToken flagName = parser.Expect(SyntaxKind.IdentifierToken);

    return new SignedFlag(sign, flagName);
  }
}
