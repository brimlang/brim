using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record ValueDeclaration(
  GreenToken? Mutator,
  GreenToken Name,
  GreenToken Colon,
  TypeExpr TypeNode,
  GreenToken Equal,
  StructuralArray<GreenNode> Initializer,
  GreenToken Terminator)
  : GreenNode(SyntaxKind.ValueDeclaration, (Mutator ?? Name).Offset)
{
  public bool IsMutable => Mutator is not null;
  public override int FullWidth => Terminator.EndOffset - (Mutator ?? Name).Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    if (Mutator is not null) yield return Mutator;
    yield return Name;
    yield return Colon;
    foreach (GreenNode child in TypeNode.GetChildren())
      yield return child;
    yield return Equal;
    foreach (GreenNode n in Initializer) yield return n;
    yield return Terminator;
  }

  public static ValueDeclaration Parse(Parser p)
  {
    GreenToken? mutator = null;
    if (p.MatchRaw(RawKind.Hat))
      mutator = new GreenToken(SyntaxKind.HatToken, p.ExpectRaw(RawKind.Hat));

    GreenToken name = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    TypeExpr typeExpr = TypeExpr.Parse(p);
    GreenToken eq = p.ExpectSyntax(SyntaxKind.EqualToken);

    ImmutableArray<GreenNode>.Builder init = ImmutableArray.CreateBuilder<GreenNode>();
    // Consume initializer tokens structurally until a Terminator (structure-only phase)
    while (!p.MatchRaw(RawKind.Terminator) && !p.MatchRaw(RawKind.Eob))
      init.Add(new GreenToken(SyntaxKind.Undefined, p.ExpectRaw(p.Current.CoreToken.Kind)));

    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new ValueDeclaration(mutator, name, colon, typeExpr, eq, init, term);
  }
}
