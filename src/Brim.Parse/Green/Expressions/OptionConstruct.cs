namespace Brim.Parse.Green;

public sealed record OptionConstruct(
  GreenToken QuestionOpen,
  ExprNode? Value,
  GreenToken CloseBrace)
  : ExprNode(SyntaxKind.OptionConstruct, QuestionOpen.Offset)
{
  public override int FullWidth =>
    QuestionOpen.FullWidth + (Value?.FullWidth ?? 0) + CloseBrace.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return QuestionOpen;
    if (Value is not null) yield return Value;
    yield return CloseBrace;
  }
}
