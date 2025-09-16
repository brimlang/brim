using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record FunctionType(
  GreenToken OpenParen,
  StructuralArray<FunctionParameter> Parameters,
  GreenToken CloseParen,
  GreenNode ReturnType)
  : GreenNode(SyntaxKind.FunctionType, OpenParen.Offset)
{
  public override int FullWidth => ReturnType.EndOffset - OpenParen.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return OpenParen;
    foreach (FunctionParameter p in Parameters) yield return p;
    yield return CloseParen;
    yield return ReturnType;
  }

  public static FunctionType Parse(Parser p)
  {
    GreenToken open = p.ExpectSyntax(SyntaxKind.OpenParenToken);
    StructuralArray<FunctionParameter> plist =
      Delimited.ParseCommaSeparatedTypes(
        p,
        static p2 => TypeExpr.Parse(p2),
        static (n, c) => new FunctionParameter(n, c),
        RawKind.RParen);
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseParenToken);
    GreenNode ret = TypeExpr.Parse(p);
    return new FunctionType(open, plist, close, ret);
  }
}
