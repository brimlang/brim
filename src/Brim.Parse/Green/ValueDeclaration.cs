namespace Brim.Parse.Green;

public sealed record ValueDeclaration(
  GreenToken? Atmark,
  GreenToken Name,
  GreenToken Colon,
  GreenNode TypeNode,
  GreenToken Equal,
  GreenToken Terminator)
  : GreenNode(SyntaxKind.ValueDeclaration, (Atmark ?? Name).Offset)
{
  public bool IsMutable => Atmark is not null;
  public override int FullWidth => Terminator.EndOffset - (Atmark ?? Name).Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    if (Atmark is not null) yield return Atmark;
    yield return Name;
    yield return Colon;
    yield return TypeNode;
    yield return Equal;
    yield return Terminator;
  }

  public static ValueDeclaration Parse(Parser p)
  {
    GreenToken? at = null;
    if (p.MatchRaw(RawKind.Atmark))
      at = new GreenToken(SyntaxKind.AtToken, p.ExpectRaw(RawKind.Atmark));

    GreenToken name = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenNode typeExpr = TypeExpr.Parse(p);
    GreenToken eq = p.ExpectSyntax(SyntaxKind.EqualToken);

    // Consume initializer tokens structurally until a Terminator (structure-only phase)
    while (!p.MatchRaw(RawKind.Terminator) && !p.MatchRaw(RawKind.Eob))
      _ = p.ExpectRaw(p.Current.Kind);

    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new ValueDeclaration(at, name, colon, typeExpr, eq, term);
  }
}

