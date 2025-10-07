namespace Brim.Parse.Green;

public sealed record FunctionShape(
    CommaList<FunctionParameter> ParameterList,
    TypeExpr ReturnType)
  : GreenNode(SyntaxKind.FunctionShape, ParameterList.OpenToken.Offset)
{
  public override int FullWidth => ReturnType.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    foreach (GreenNode child in ParameterList.GetChildren())
      yield return child;
    foreach (GreenNode child in ReturnType.GetChildren())
      yield return child;
  }

  public static FunctionShape Parse(Parser p)
  {
    CommaList<FunctionParameter> parameters = CommaList<FunctionParameter>.Parse(
      p,
      SyntaxKind.OpenParenToken,
      SyntaxKind.CloseParenToken,
      FunctionParameter.Parse);

    TypeExpr returnType = TypeExpr.Parse(p);

    return new FunctionShape(parameters, returnType);
  }
}
