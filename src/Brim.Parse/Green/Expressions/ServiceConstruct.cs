namespace Brim.Parse.Green;

public sealed record ServiceConstruct(
  TypeRef Type,
  CommaList<FieldInit> Fields)
  : ExprNode(SyntaxKind.ServiceConstruct, Type.Offset)
{
  public override int FullWidth => Type.FullWidth + Fields.FullWidth;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Type;
    yield return Fields;
  }
}
