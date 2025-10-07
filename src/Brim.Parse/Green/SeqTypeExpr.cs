namespace Brim.Parse.Green;

public sealed record SeqTypeExpr(
  GreenToken SeqKeyword,
  GreenToken LBracket,
  TypeExpr ElementType,
  GreenToken RBracket
) : GreenNode(SyntaxKind.SeqTypeExpr, SeqKeyword.Offset)
{
  public override int FullWidth => RBracket.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return SeqKeyword;
    yield return LBracket;
    foreach (GreenNode child in ElementType.GetChildren())
      yield return child;
    yield return RBracket;
  }

  public static SeqTypeExpr Parse(Parser p)
  {
    GreenToken seq = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken lbracket = p.ExpectSyntax(SyntaxKind.GenericOpenToken);
    TypeExpr elem = TypeExpr.Parse(p);
    GreenToken rbracket = p.ExpectSyntax(SyntaxKind.GenericCloseToken);
    return new SeqTypeExpr(seq, lbracket, elem, rbracket);
  }
}
