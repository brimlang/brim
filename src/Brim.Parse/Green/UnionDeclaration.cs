namespace Brim.Parse.Green;

public sealed record UnionVariantDeclaration(
  Identifier Identifier,
  GreenToken Colon,
  GreenNode Type) : GreenNode(SyntaxKind.UnionVariantDeclaration, Identifier.Offset)
{
  public override int FullWidth => Type.EndOffset - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier; yield return Colon; yield return Type;
  }

  public static UnionVariantDeclaration Parse(Parser p)
  {
    Identifier name = Identifier.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    Identifier typeName = Identifier.Parse(p);
    GreenNode ty = typeName;
    if (p.Match(RawTokenKind.LBracket) && !p.Match(RawTokenKind.LBracketLBracket))
    {
      ty = GenericType.ParseAfterName(p, typeName);
    }
    return new(name, colon, ty);
  }
}

public sealed record UnionDeclaration(
  Identifier Identifier,
  GenericParameterList? GenericParams,
  GreenToken Equal,
  GreenToken UnionOpen,
  StructuralArray<UnionVariantDeclaration> Variants,
  GreenToken Close,
  GreenToken Terminator) : GreenNode(SyntaxKind.UnionDeclaration, Identifier.Offset)
{
  public override int FullWidth => Terminator.EndOffset - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    if (GenericParams is not null) yield return GenericParams;
    yield return Equal; yield return UnionOpen;
    foreach (UnionVariantDeclaration v in Variants) yield return v;
    yield return Close; yield return Terminator;
  }

  public static UnionDeclaration Parse(Parser p)
  {
    Identifier id = Identifier.Parse(p);
    GreenToken eq = p.ExpectSyntax(SyntaxKind.EqualToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.UnionToken);
    ImmutableArray<UnionVariantDeclaration>.Builder vars = ImmutableArray.CreateBuilder<UnionVariantDeclaration>();
    while (!p.Match(RawTokenKind.RBrace) && !p.Match(RawTokenKind.Eob))
    {
      int before = p.Current.CoreToken.Offset;
      vars.Add(UnionVariantDeclaration.Parse(p));
      if (p.Match(RawTokenKind.Comma)) _ = p.Expect(RawTokenKind.Comma);
      if (p.Current.CoreToken.Offset == before)
      {
        // No progress; break to avoid infinite loop.
        break;
      }
    }
    StructuralArray<UnionVariantDeclaration> arr = [.. vars];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new(id, null, eq, open, arr, close, term);
  }
}
