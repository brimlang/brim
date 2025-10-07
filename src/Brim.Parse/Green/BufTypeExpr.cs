namespace Brim.Parse.Green;

public sealed record BufTypeExpr(
  GreenToken BufKeyword,
  GreenToken LBracket,
  TypeExpr ElementType,
  GreenToken? Star,
  GreenToken? Size,
  GreenToken RBracket
) : GreenNode(SyntaxKind.BufTypeExpr, BufKeyword.Offset)
{
  public override int FullWidth => RBracket.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return BufKeyword;
    yield return LBracket;
    yield return ElementType;
    if (Star is not null) yield return Star;
    if (Size is not null) yield return Size;
    yield return RBracket;
  }

  public static BufTypeExpr Parse(Parser p)
  {
    GreenToken buf = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken lbracket = p.ExpectSyntax(SyntaxKind.GenericOpenToken);
    TypeExpr elem = TypeExpr.Parse(p);
    GreenToken? star = null;
    GreenToken? size = null;
    if (p.MatchRaw(RawKind.Star))
    {
      star = p.ExpectSyntax(SyntaxKind.StarToken);
      size = p.ExpectSyntax(SyntaxKind.IntToken);
    }
    GreenToken rbracket = p.ExpectSyntax(SyntaxKind.GenericCloseToken);
    return new BufTypeExpr(buf, lbracket, elem, star, size, rbracket);
  }
}
