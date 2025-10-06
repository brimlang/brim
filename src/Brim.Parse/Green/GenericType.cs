namespace Brim.Parse.Green;

public sealed record GenericType(
  GreenToken Name,
  GenericArgumentList Arguments) :
GreenNode(SyntaxKind.GenericType, Name.Offset)
{
  public override int FullWidth => Arguments.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Arguments;
  }

  public static GenericType ParseAfterName(Parser p, GreenToken name)
  {
    GenericArgumentList args = GenericArgumentList.Parse(p);
    return new GenericType(name, args);
  }
}
