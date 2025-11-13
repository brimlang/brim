namespace Brim.Parse.Green;

public sealed record MatchBlock(
  GreenToken OpenBrace,
  GreenToken? LeadingTerminator,
  StructuralArray<MatchBlock.Element> Arms,
  GreenToken? TrailingTerminator,
  GreenToken CloseBrace)
  : GreenNode(SyntaxKind.MatchBlock, OpenBrace.Offset)
{
  public override int FullWidth => CloseBrace.EndOffset - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return OpenBrace;
    if (LeadingTerminator is not null) yield return LeadingTerminator;
    foreach (Element arm in Arms)
      yield return arm;
    if (TrailingTerminator is not null) yield return TrailingTerminator;
    yield return CloseBrace;
  }

  public sealed record Element(
    GreenToken? LeadingTerminator,
    MatchArm Arm)
    : GreenNode(SyntaxKind.ListElement, Arm.Offset)
  {
    public override int FullWidth => Arm.EndOffset - Offset;
    public override IEnumerable<GreenNode> GetChildren()
    {
      if (LeadingTerminator is not null) yield return LeadingTerminator;
      yield return Arm;
    }
  }
}
