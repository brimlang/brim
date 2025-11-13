namespace Brim.Parse.Green;

public sealed record DeclarationName(
  GreenToken Identifier,
  GenericParameterList? GenericParams) :
GreenNode(SyntaxKind.DeclarationName, Identifier.Offset),
IParsable<DeclarationName>
{
  public override int FullWidth => (GenericParams?.EndOffset ?? Identifier.EndOffset) - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    if (GenericParams is not null) yield return GenericParams;
  }

  public static DeclarationName Parse(Parser p)
  {
    GreenToken id = p.Expect(SyntaxKind.IdentifierToken);
    GenericParameterList? gp = GenericParameterList.TryParse(p);
    return new DeclarationName(id, gp);
  }
}
