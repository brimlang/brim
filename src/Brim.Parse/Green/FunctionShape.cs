namespace Brim.Parse.Green;

public sealed record FunctionShape(
    CommaList<FunctionParameter> ParameterList,
    GreenNode ReturnType)
  : GreenNode(SyntaxKind.FunctionShape, ParameterList.OpenToken.Offset)
{
  public override int FullWidth => ReturnType.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    foreach (GreenNode child in ParameterList.GetChildren())
      yield return child;
    yield return ReturnType;
  }

  public static FunctionShape Parse(Parser p)
  {
    CommaList<FunctionParameter> parameters = CommaList<FunctionParameter>.Parse(
      p,
      SyntaxKind.OpenParenToken,
      SyntaxKind.CloseParenToken,
      FunctionParameter.Parse);

    GreenNode returnType = TypeExpr.Parse(p);

    return new FunctionShape(parameters, returnType);
  }
}
