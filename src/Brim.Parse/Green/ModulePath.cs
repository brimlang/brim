namespace Brim.Parse.Green;

public sealed record class ModulePath(
  StructuralArray<GreenToken> Parts)
: GreenNode(SyntaxKind.ModulePath, Parts[0].Offset)
, IParsable<ModulePath>
{
  public override int FullWidth => Parts.Sum(static p => p.FullWidth);
  public override IEnumerable<GreenNode> GetChildren()
  {
    foreach (GreenToken part in Parts)
      yield return part;
  }

  public static ModulePath Parse(Parser p)
  {
    ImmutableArray<GreenToken>.Builder parts = ImmutableArray.CreateBuilder<GreenToken>();

    parts.Add(p.Expect(SyntaxKind.IdentifierToken));
    while (p.Match(TokenKind.ColonColon))
    {
      parts.Add(p.Expect(SyntaxKind.ModulePathSepToken));
      parts.Add(p.Expect(SyntaxKind.IdentifierToken));
    }

    return new ModulePath(parts);
  }
}

