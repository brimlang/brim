namespace Brim.Parse.Green;

public sealed record FlagsConstruct(
  TypeRef Type,
  CommaList<GreenToken> Flags)
  : ExprNode(SyntaxKind.FlagsConstruct, Type.Offset)
{
  public override int FullWidth => Type.FullWidth + Flags.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Type;
    yield return Flags;
  }
}
