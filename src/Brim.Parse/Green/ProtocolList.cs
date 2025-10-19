namespace Brim.Parse.Green;

/// <summary>
/// Angle-bracketed comma-separated list: '<' elements* '>'
/// Used for protocol constraints and other angle-bracket delimited lists.
/// </summary>
public sealed record ProtocolList(
  CommaList<TypeExpr> List) :
GreenNode(SyntaxKind.ProtocolList, List.Offset)
{
  public override int FullWidth => List.FullWidth;
  public override IEnumerable<GreenNode> GetChildren() => List.GetChildren();

  public static ProtocolList Parse(Parser p)
  {
    CommaList<TypeExpr> list = CommaList<TypeExpr>.Parse(
        p,
        SyntaxKind.LessToken,
        SyntaxKind.GreaterToken,
        TypeExpr.Parse);

    return new ProtocolList(list);
  }
}

