namespace Brim.Parse.Green;

public sealed record GenericParameterList(
  CommaList<GenericParameter> ParameterList) :
GreenNode(SyntaxKind.GenericParameterList, ParameterList.Offset)
{
  public override int FullWidth => ParameterList.FullWidth;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return ParameterList;
  }

  public static GenericParameterList? TryParse(Parser p)
  {
    if (!p.Match(TokenKind.LBracket))
      return null; // fast reject

    CommaList<GenericParameter> list = CommaList<GenericParameter>.Parse(
      p,
      SyntaxKind.GenericOpenToken,
      SyntaxKind.GenericCloseToken,
      ParseParameter);

    if (list.Elements.Count == 0)
      p.AddDiagEmptyGenericParam(list.OpenToken);

    return new GenericParameterList(list);
  }

  static GenericParameter ParseParameter(Parser p)
  {
    GreenToken paramIdTok = p.Expect(SyntaxKind.IdentifierToken);
    ConstraintList? constraints = ParseConstraints(p);
    return new(paramIdTok, constraints);
  }

  static ConstraintList? ParseConstraints(Parser p)
  {
    if (!p.Match(TokenKind.Colon))
      return null;

    GreenToken colonTok = p.Expect(SyntaxKind.ColonToken);

    ArrayBuilder<ConstraintRef> refs = [];

    if (!p.Match(TokenKind.RBracket) && !p.Match(TokenKind.Eob) && !p.Match(TokenKind.Comma))
    {
      refs.Add(ParseConstraintRef(p));

      while (!p.Match(TokenKind.RBracket) && !p.Match(TokenKind.Eob) && !p.Match(TokenKind.Comma))
      {
        Parser.StallGuard sg = p.GetStallGuard();
        refs.Add(ParseConstraintRef(p));
        if (sg.Stalled) break;
      }
    }

    if (refs.Count == 0) p.AddDiagInvalidGenericConstraint();
    return new ConstraintList(colonTok, refs);

    static ConstraintRef ParseConstraintRef(Parser p)
    {
      GreenToken? leadingPlus = null;
      if (p.Match(TokenKind.Plus))
        leadingPlus = p.Expect(SyntaxKind.PlusToken);

      // Parse full TypeRef to support qualified names like runtime.Num
      TypeRef typeRef = TypeRef.Parse(p);
      return new ConstraintRef(leadingPlus, typeRef);
    }
  }
}

public sealed record GenericParameter(
  GreenToken Name,
  ConstraintList? Constraints) :
GreenNode(SyntaxKind.GenericParameter, Name.Offset)
{
  public override int FullWidth => (Constraints?.EndOffset ?? Name.EndOffset) - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    if (Constraints is not null) yield return Constraints;
  }
}

public sealed record ConstraintList(
  GreenToken Colon,
  StructuralArray<ConstraintRef> Constraints) : GreenNode(SyntaxKind.ConstraintList, Colon.Offset)
{
  public override int FullWidth => Constraints.Count == 0 ? Colon.FullWidth : Constraints[^1].EndOffset - Colon.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Colon;
    foreach (ConstraintRef c in Constraints) yield return c;
  }
}

public sealed record GenericArgument(
  TypeExpr TypeNode
) : GreenNode(SyntaxKind.GenericArgument, TypeNode.Offset)
{
  public override int FullWidth => TypeNode.FullWidth;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return TypeNode;
  }
}

public sealed record GenericArgumentList(
  CommaList<GenericArgument> ArgumentList
) : GreenNode(SyntaxKind.GenericArgumentList, ArgumentList.Offset)
{
  public override int FullWidth => ArgumentList.FullWidth;
  public override IEnumerable<GreenNode> GetChildren() => ArgumentList.GetChildren();

  public static GenericArgumentList Parse(Parser p)
  {
    CommaList<GenericArgument> list = CommaList<GenericArgument>.Parse(
      p,
      SyntaxKind.GenericOpenToken,
      SyntaxKind.GenericCloseToken,
      static p2 => new GenericArgument(TypeExpr.Parse(p2)));

    if (list.Elements.Count == 0)
      p.AddDiagEmptyGeneric(list.OpenToken);

    return new GenericArgumentList(list);
  }
}
