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
    yield return TypeNode;
    yield return Equal;
    yield return Initializer;
    yield return Terminator;
  }

  public static ValueDeclaration Parse(Parser p)
  {
    GreenToken? mutator = null;
    if (p.Match(TokenKind.Hat))
      mutator = p.Expect(SyntaxKind.MutableToken);

    DeclarationName name = DeclarationName.Parse(p);
    return ParseAfterName(p, mutator, name);
  }

  internal static ValueDeclaration ParseAfterName(Parser p, GreenToken? mutator, DeclarationName name)
  {
    GreenToken colon = p.Expect(SyntaxKind.ColonToken);
    TypeExpr typeExpr = TypeExpr.Parse(p);

    if (p.Match(TokenKind.Terminator) && mutator is null)
    {
      p.AddDiagUnsupportedModuleMember(name.Identifier.CoreToken);

      GreenToken terminator = p.Expect(SyntaxKind.TerminatorToken);
      GreenToken errorEq = SyntaxKind.ErrorToken.MakeGreen(p.Current);
      ExprNode errorInit = new LiteralExpr(errorEq);
      return new ValueDeclaration(mutator, name, colon, typeExpr, errorEq, errorInit, terminator);
    }

    GreenToken eq = p.Expect(SyntaxKind.EqualToken);

    ExprNode initializer = p.Match(TokenKind.Terminator) || p.Match(TokenKind.Eob)
      ? new IdentifierExpr(p.FabricateMissing(SyntaxKind.IdentifierToken))
      : p.ParseExpression();

    GreenToken term = p.Expect(SyntaxKind.TerminatorToken);
    return new ValueDeclaration(mutator, name, colon, typeExpr, eq, initializer, term);
  }
}
