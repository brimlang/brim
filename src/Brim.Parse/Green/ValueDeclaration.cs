namespace Brim.Parse.Green;

public sealed record ValueDeclaration(
  GreenToken? Mutator,
  DeclarationName Name,
  GreenToken Colon,
  TypeExpr TypeNode,
  GreenToken Equal,
  ExprNode Initializer,
  GreenToken Terminator)
  : GreenNode(SyntaxKind.ValueDeclaration, Mutator?.Offset ?? Name.Offset)
{
  public bool IsMutable => Mutator is not null;
  public override int FullWidth => Terminator.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    if (Mutator is not null) yield return Mutator;
    yield return Name;
    yield return Colon;
    foreach (GreenNode child in TypeNode.GetChildren())
      yield return child;
    yield return Equal;
    yield return Initializer;
    yield return Terminator;
  }

  public static ValueDeclaration Parse(Parser p)
  {
    GreenToken? mutator = null;
    if (p.MatchRaw(RawKind.Hat))
      mutator = new GreenToken(SyntaxKind.HatToken, p.ExpectRaw(RawKind.Hat));

    DeclarationName name = DeclarationName.Parse(p);
    return ParseAfterName(p, mutator, name);
  }

  internal static ValueDeclaration ParseAfterName(Parser p, GreenToken? mutator, DeclarationName name)
  {
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    TypeExpr typeExpr = TypeExpr.Parse(p);

    if (p.MatchRaw(RawKind.Terminator) && mutator is null)
    {
      p.AddDiagUnsupportedModuleMember(name.Identifier.Token);

      GreenToken terminator = p.ExpectSyntax(SyntaxKind.TerminatorToken);
      GreenToken errorEq = new(SyntaxKind.ErrorToken, p.Current);
      ExprNode errorInit = new LiteralExpr(errorEq);
      return new ValueDeclaration(mutator, name, colon, typeExpr, errorEq, errorInit, terminator);
    }

    GreenToken eq = p.ExpectSyntax(SyntaxKind.EqualToken);

    ExprNode initializer = p.MatchRaw(RawKind.Terminator) || p.MatchRaw(RawKind.Eob)
      ? new IdentifierExpr(p.FabricateMissing(SyntaxKind.IdentifierToken, RawKind.Identifier))
      : p.ParseExpression();

    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new ValueDeclaration(mutator, name, colon, typeExpr, eq, initializer, term);
  }
}
