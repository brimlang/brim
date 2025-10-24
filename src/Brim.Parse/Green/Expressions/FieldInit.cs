namespace Brim.Parse.Green;

public sealed record FieldInit(
  GreenToken Name,
  GreenToken EqualsToken,
  ExprNode Value)
  : GreenNode(SyntaxKind.FieldInit, Name.Offset)
{
  public override int FullWidth => Name.FullWidth + EqualsToken.FullWidth + Value.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return EqualsToken;
    yield return Value;
  }

  public static FieldInit Parse(Parser p)
  {
    GreenToken name = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken equals = p.ExpectSyntax(SyntaxKind.EqualToken);
    ExprNode value = p.ParseExpression();
    return new FieldInit(name, equals, value);
  }
}
