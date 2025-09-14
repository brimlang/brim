namespace Brim.Parse.Green;

public sealed record OptionType(
  GreenNode Inner,
  GreenToken Suffix)
  : GreenNode(SyntaxKind.GenericType, Inner.Offset) // reuse type coloring/category
{
  public override int FullWidth => Suffix.EndOffset - Inner.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Inner;
    yield return Suffix;
  }
}

public sealed record ResultType(
  GreenNode Inner,
  GreenToken Suffix)
  : GreenNode(SyntaxKind.GenericType, Inner.Offset)
{
  public override int FullWidth => Suffix.EndOffset - Inner.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Inner;
    yield return Suffix;
  }
}

