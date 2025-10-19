using Brim.Parse;

namespace Brim.Parse.Green;

public sealed record LambdaParams(
  GreenToken? SingleParameter,
  CommaList<GreenToken>? ParameterList)
  : GreenNode(SyntaxKind.LambdaParams, SingleParameter is not null ? SingleParameter.Offset : ParameterList!.Offset)
{
  public override int FullWidth => SingleParameter?.FullWidth ?? ParameterList!.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    if (SingleParameter is not null)
    {
      yield return SingleParameter;
      yield break;
    }

    if (ParameterList is not null)
      yield return ParameterList;
  }

  public static LambdaParams ParseAfterArrow(Parser p)
  {
    if (p.MatchRaw(RawKind.LParen))
    {
      CommaList<GreenToken> list = CommaList<GreenToken>.Parse(
        p,
        SyntaxKind.OpenParenToken,
        SyntaxKind.CloseParenToken,
        ParseParameterElement);

      return new LambdaParams(null, list);
    }

    GreenToken single = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    return new LambdaParams(single, null);
  }

  static GreenToken ParseParameterElement(Parser parser) => parser.ExpectSyntax(SyntaxKind.IdentifierToken);
}
