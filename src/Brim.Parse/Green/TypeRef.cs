namespace Brim.Parse.Green;

public sealed record TypeRef(
  GreenToken Name,
  GenericArgumentList? GenericArgs
) : GreenNode(SyntaxKind.TypeRef, Name.Offset)
{
  public override int FullWidth => (GenericArgs?.EndOffset ?? Name.EndOffset) - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    if (GenericArgs is not null) yield return GenericArgs;
  }

  public static TypeRef Parse(Parser p)
  {
    GreenToken name = p.ExpectSyntax(SyntaxKind.IdentifierToken);

    GenericArgumentList? args = null;
    if (p.MatchRaw(RawKind.LBracket))
      args = GenericArgumentList.Parse(p);
    return new TypeRef(name, args);
  }

  public static TypeRef ParseAfterName(Parser p, GreenToken name)
  {
    GenericArgumentList args = GenericArgumentList.Parse(p);
    return new TypeRef(name, args);
  }
}
