namespace Brim.Parse.Green;

public sealed record MethodSignature(
  DeclarationName Name,
  GreenToken Colon,
  FunctionShape MethodShape
) : GreenNode(SyntaxKind.MethodSignature, Name.Offset),
  IParsable<MethodSignature>
{
  public override int FullWidth => MethodShape.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    foreach (GreenNode p in MethodShape.GetChildren()) yield return p;
  }

  public static MethodSignature Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    FunctionShape shape = FunctionShape.Parse(p);
    return new(name, colon, shape);
  }
}
