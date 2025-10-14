using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record TypeRef(
  QualifiedIdent QualifiedIdent,
  GenericArgumentList? GenericArgs) :
GreenNode(SyntaxKind.TypeRef, QualifiedIdent.Offset)
{
  public GreenToken Name => QualifiedIdent.Name;
  public StructuralArray<QualifiedIdent.Qualifier> QualifierParts => QualifiedIdent.Qualifiers;

  public override int FullWidth => (GenericArgs?.EndOffset ?? QualifiedIdent.EndOffset) - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return QualifiedIdent;
    if (GenericArgs is not null) yield return GenericArgs;
  }

  public static TypeRef Parse(Parser p)
  {
    QualifiedIdent qualifiedIdent = QualifiedIdent.Parse(p);

    GenericArgumentList? args = null;
    if (p.MatchRaw(RawKind.LBracket))
      args = GenericArgumentList.Parse(p);

    return new TypeRef(qualifiedIdent, args);
  }

  public static TypeRef ParseAfterName(Parser p, GreenToken name)
  {
    GenericArgumentList args = GenericArgumentList.Parse(p);
    QualifiedIdent qualifiedIdent = new([], name);
    return new TypeRef(qualifiedIdent, args);
  }
}
