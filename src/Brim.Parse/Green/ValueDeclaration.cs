namespace Brim.Parse.Green;

public sealed record ValueDeclaration(
  GreenToken? Mutator,
  GreenToken Name,
  GreenToken Colon,
  TypeExpr TypeNode,
  GreenToken Equal,
  ExprNode Initializer,
  GreenToken Terminator)
  : GreenNode(SyntaxKind.ValueDeclaration, (Mutator ?? Name).Offset)
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

    GreenToken name = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    TypeExpr typeExpr = TypeExpr.Parse(p);

    // Check if this looks like a function header without body (unsupported)
    if (p.MatchRaw(RawKind.Terminator) && mutator is null)
    {
      // This is likely "name :(params) RetType" without "= expr" or "{ body }"
      // Emit UnsupportedModuleMember diagnostic for function definitions
      p.AddDiagUnsupportedModuleMember(name.Token);

      GreenToken terminator = p.ExpectSyntax(SyntaxKind.TerminatorToken);
      GreenToken errorEq = new(SyntaxKind.ErrorToken, p.Current);
      ExprNode errorInit = new LiteralExpr(errorEq);
      return new ValueDeclaration(mutator, name, colon, typeExpr,
        errorEq, errorInit, terminator);
    }

    GreenToken eq = p.ExpectSyntax(SyntaxKind.EqualToken);

    ExprNode initializer;
    if (p.MatchRaw(RawKind.Terminator) || p.MatchRaw(RawKind.Eob))
    {
      GreenToken missing = p.FabricateMissing(SyntaxKind.IdentifierToken, RawKind.Identifier);
      initializer = new IdentifierExpr(missing);
    }
    else
    {
      initializer = p.ParseExpression();
    }

    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new ValueDeclaration(mutator, name, colon, typeExpr, eq, initializer, term);
  }
}
