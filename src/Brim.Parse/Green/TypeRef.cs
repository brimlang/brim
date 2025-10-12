using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record TypeRef(
  GreenToken Name,
  StructuralArray<GreenToken> QualifierParts,
  GenericArgumentList? GenericArgs
) : GreenNode(SyntaxKind.TypeRef, Name.Offset)
{
  public override int FullWidth => (GenericArgs?.EndOffset ??
    (QualifierParts.Count > 0 ? QualifierParts[^1].EndOffset : Name.EndOffset)) - Offset;

  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    foreach (GreenToken part in QualifierParts)
      yield return part;
    if (GenericArgs is not null) yield return GenericArgs;
  }

  public static TypeRef Parse(Parser p)
  {
    GreenToken name = p.ExpectSyntax(SyntaxKind.IdentifierToken);

    // Parse optional dotted qualifiers (e.g., runtime.Num)
    ArrayBuilder<GreenToken> qualifiers = [];
    while (p.MatchRaw(RawKind.Stop))
    {
      GreenToken dot = p.ExpectSyntax(SyntaxKind.StopToken);
      GreenToken part = p.ExpectSyntax(SyntaxKind.IdentifierToken);
      qualifiers.Add(dot);
      qualifiers.Add(part);
    }

    GenericArgumentList? args = null;
    if (p.MatchRaw(RawKind.LBracket))
      args = GenericArgumentList.Parse(p);

    return new TypeRef(name, qualifiers, args);
  }

  public static TypeRef ParseAfterName(Parser p, GreenToken name)
  {
    GenericArgumentList args = GenericArgumentList.Parse(p);
    return new TypeRef(name, [], args);
  }
}
