namespace Brim.Parse.Green;

public sealed record MethodSignature(
  DeclarationName Name,
  GreenToken Colon,
  CommaList<FunctionParam> Parameters,
  TypeExpr ReturnType
) : GreenNode(SyntaxKind.MethodSignature, Name.Offset),
  IParsable<MethodSignature>
{
  public override int FullWidth => ReturnType.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    yield return Parameters;
    yield return ReturnType;
  }

  public static MethodSignature Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    CommaList<FunctionParam> parameters = CommaList<FunctionParam>.Parse(
      p,
      SyntaxKind.OpenParenToken,
      SyntaxKind.CloseParenToken,
      FunctionParam.Parse);
    TypeExpr returnType = TypeExpr.Parse(p);
    return new(name, colon, parameters, returnType);
  }
}
