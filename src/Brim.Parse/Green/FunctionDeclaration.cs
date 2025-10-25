namespace Brim.Parse.Green;

public sealed record FunctionParam(
  GreenToken Name,
  GreenToken Colon,
  TypeExpr Type)
  : GreenNode(SyntaxKind.FunctionParameter, Name.Offset)
{
  public override int FullWidth => Type.EndOffset - Name.Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    yield return Type;
  }

  public static FunctionParam Parse(Parser p)
  {
    GreenToken name = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    TypeExpr type = TypeExpr.Parse(p);
    return new FunctionParam(name, colon, type);
  }
}

public sealed record FunctionDeclaration(
  DeclarationName Name,
  GreenToken Colon,
  CommaList<FunctionParam> Parameters,
  TypeExpr ReturnType,
  BlockExpr Body,
  GreenToken Terminator)
  : GreenNode(SyntaxKind.FunctionDeclaration, Name.Offset)
{
  public override int FullWidth => Terminator.EndOffset - Name.Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    yield return Parameters;
    yield return ReturnType;
    yield return Body;
    yield return Terminator;
  }

  public static FunctionDeclaration ParseAfterName(Parser p, DeclarationName name)
  {
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);

    CommaList<FunctionParam> parameters = CommaList<FunctionParam>.Parse(
      p,
      SyntaxKind.OpenParenToken,
      SyntaxKind.CloseParenToken,
      FunctionParam.Parse);

    TypeExpr returnType = TypeExpr.Parse(p);
    BlockExpr body = BlockExpr.Parse(p);
    GreenToken terminator = p.ExpectSyntax(SyntaxKind.TerminatorToken);

    return new FunctionDeclaration(
      name,
      colon,
      parameters,
      returnType,
      body,
      terminator);
  }
}
