namespace Brim.Parse.Green;

public sealed record DeclarationName(
  GreenToken Identifier,
  GenericParameterList? GenericParams) :
GreenNode(SyntaxKind.DeclarationName, Identifier.Offset),
IParsable<DeclarationName>
{
  public override int FullWidth => (GenericParams is null ? Identifier.EndOffset : GenericParams.EndOffset) - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    if (GenericParams is not null) yield return GenericParams;
  }

  public static DeclarationName Parse(Parser p)
  {
    GreenToken id = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GenericParameterList? gp = GenericParameterListParser.TryParse(p);
    return new DeclarationName(id, gp);
  }
}
