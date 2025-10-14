using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record QualifiedIdent(
    StructuralArray<QualifiedIdent.Qualifier> Qualifiers,
    GreenToken Name) :
GreenNode(SyntaxKind.QualifiedIdentifier, Qualifiers.Count > 0 ? Qualifiers[0].Offset : Name.Offset)
{
  public override int FullWidth => Name.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    foreach (Qualifier q in Qualifiers)
      yield return q;
    yield return Name;
  }

  public static QualifiedIdent Parse(Parser p)
  {
    ArrayBuilder<Qualifier> qualifiers = [];
    while (p.MatchRaw(RawKind.Identifier) && p.MatchRaw(RawKind.Stop, 1))
    {
      qualifiers.Add(Qualifier.Parse(p));
    }

    GreenToken name = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    return new QualifiedIdent(qualifiers, name);
  }

  public sealed record Qualifier(
      GreenToken Name,
      GreenToken Dot
  ) : GreenNode(SyntaxKind.Qualifier, Name.Offset)
  {
    public override int FullWidth => Dot.EndOffset - Offset;
    public override IEnumerable<GreenNode> GetChildren()
    {
      yield return Name;
      yield return Dot;
    }

    public static Qualifier Parse(Parser p)
    {
      GreenToken id = p.ExpectSyntax(SyntaxKind.IdentifierToken);
      GreenToken dot = p.ExpectSyntax(SyntaxKind.StopToken);
      return new Qualifier(id, dot);
    }
  }
}

