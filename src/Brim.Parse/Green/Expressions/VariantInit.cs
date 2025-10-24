namespace Brim.Parse.Green;

public sealed record VariantInit(
  GreenToken Name,
  GreenToken? EqualsToken,
  ExprNode? Value)
  : GreenNode(SyntaxKind.VariantInit, Name.Offset)
{
  public override int FullWidth => Name.FullWidth +
    (EqualsToken?.FullWidth ?? 0) +
    (Value?.FullWidth ?? 0);

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    if (EqualsToken is not null) yield return EqualsToken;
    if (Value is not null) yield return Value;
  }

  public static VariantInit Parse(Parser p)
  {
    GreenToken name = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    if (p.MatchRaw(RawKind.Equal))
    {
      GreenToken equals = p.ExpectSyntax(SyntaxKind.EqualToken);
      ExprNode value = p.ParseExpression();
      return new VariantInit(name, equals, value);
    }
    return new VariantInit(name, null, null);
  }
}
