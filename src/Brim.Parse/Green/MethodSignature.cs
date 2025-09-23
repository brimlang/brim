using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record MethodSignature(
  DeclarationName Name,
  GreenToken Colon,
  GreenToken OpenParen,
  StructuralArray<FunctionParameter> Parameters,
  GreenToken CloseParen,
  GreenNode ReturnType,
  GreenToken? TrailingComma)
  : GreenNode(SyntaxKind.MethodSignature, Name.Offset)
{
  public override int FullWidth => (TrailingComma?.EndOffset ?? ReturnType.EndOffset) - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    yield return OpenParen;
    foreach (GreenNode p in Parameters) yield return p;
    yield return CloseParen;
    yield return ReturnType;
    if (TrailingComma is not null) yield return TrailingComma;
  }

  public static MethodSignature Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.OpenParenToken);

    StructuralArray<FunctionParameter> plist =
      Delimited.ParseCommaSeparatedTypes(
        p,
        static p2 => TypeExpr.Parse(p2),
        static (n, c) => new FunctionParameter(n, c),
        RawKind.RParen);

    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseParenToken);
    GreenNode ret = TypeExpr.Parse(p);

    GreenToken? listTrailing = null;
    if (p.MatchRaw(RawKind.Comma))
      listTrailing = p.ExpectSyntax(SyntaxKind.CommaToken);

    return new MethodSignature(name, colon, open, plist, close, ret, listTrailing);
  }
}

