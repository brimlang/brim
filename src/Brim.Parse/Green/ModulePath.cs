using Brim.Parse.Collections;

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

    parts.Add(p.ExpectSyntax(SyntaxKind.IdentifierToken));
    while (p.MatchRaw(RawKind.ColonColon))
    {
      parts.Add(p.ExpectSyntax(SyntaxKind.ModulePathSepToken));
      parts.Add(p.ExpectSyntax(SyntaxKind.IdentifierToken));
    }

    return new ModulePath(parts);
  }
}

