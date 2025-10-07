namespace Brim.Parse.Green;

public sealed record BufTypeExpr(
  GreenToken BufKeyword,
  GreenToken LBracket,
  TypeExpr ElementType,
  GreenToken? Semicolon,
  GreenToken? Size,
  GreenToken RBracket
) : GreenNode(SyntaxKind.BufTypeExpr, BufKeyword.Offset)
{
  public override int FullWidth => RBracket.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return BufKeyword;
    yield return LBracket;
    foreach (GreenNode child in ElementType.GetChildren())
      yield return child;
    if (Semicolon is not null) yield return Semicolon;
    if (Size is not null) yield return Size;
    yield return RBracket;
  }

  public static BufTypeExpr Parse(Parser p)
  {
    GreenToken buf = p.ExpectSyntax(SyntaxKind.BufKeywordToken);
    GreenToken lbracket = p.ExpectSyntax(SyntaxKind.GenericOpenToken);
    TypeExpr elem = TypeExpr.Parse(p);
    GreenToken? semicolon = null;
    GreenToken? size = null;
    if (p.MatchRaw(RawKind.Terminator))
    {
      semicolon = p.ExpectSyntax(SyntaxKind.TerminatorToken);
      size = p.ExpectSyntax(SyntaxKind.IntToken);
    }
    GreenToken rbracket = p.ExpectSyntax(SyntaxKind.GenericCloseToken);
    return new BufTypeExpr(buf, lbracket, elem, semicolon, size, rbracket);
  }
}
