namespace Brim.Parse.Green;

public sealed record DeclarationName(
  GreenToken Identifier,
  GenericParameterList? GenericParams) :
  GreenNode(SyntaxKind.DeclarationName, Identifier.Offset)
{
  public override int FullWidth => (GenericParams is null ? Identifier.EndOffset : GenericParams.EndOffset) - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    if (GenericParams is not null) yield return GenericParams;
  }

  public static DeclarationName Parse(Parser p)
  {
    GreenToken id = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GenericParameterList? gp = GenericParameterListParser.TryParse(p);
    return new DeclarationName(id, gp);
  }
}

internal static class GenericParameterListParser
{
  public static GenericParameterList? TryParse(Parser p)
  {
    if (!p.MatchRaw(RawKind.LBracket))
      return null; // fast reject

    GreenToken open = p.ExpectSyntax(SyntaxKind.GenericOpenToken);

    bool empty = p.MatchRaw(RawKind.RBracket);
    ImmutableArray<GenericParameter>.Builder items = ImmutableArray.CreateBuilder<GenericParameter>();
    if (!empty)
    {
      bool first = true;
      while (!p.MatchRaw(RawKind.RBracket) && !p.MatchRaw(RawKind.Eob))
      {
        int before = p.Current.CoreToken.Offset;
        if (!first)
        {
          if (p.MatchRaw(RawKind.Comma))
            _ = p.ExpectRaw(RawKind.Comma);
          else
            break;
        }

        GreenToken paramIdTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
        ConstraintList? constraints = ParseConstraints(p);
        items.Add(new GenericParameter(paramIdTok, constraints));
        first = false;

        if (p.Current.CoreToken.Offset == before) break; // safety guard
      }
    }

    GreenToken close = p.ExpectSyntax(SyntaxKind.GenericCloseToken);
    if (empty)
      p.AddDiagEmptyGeneric(open);

    return new GenericParameterList(open, items.ToImmutable(), close);
  }

  static ConstraintList? ParseConstraints(Parser p)
  {
    if (!p.MatchRaw(RawKind.Colon))
      return null;

    GreenToken colonTok = p.ExpectSyntax(SyntaxKind.ColonToken);
    ImmutableArray<GreenNode>.Builder refs = ImmutableArray.CreateBuilder<GreenNode>();

    bool firstRef = true;
    while (!p.MatchRaw(RawKind.RBracket) && !p.MatchRaw(RawKind.Eob))
    {
      int beforeRef = p.Current.CoreToken.Offset;
      if (!firstRef)
      {
        if (p.MatchRaw(RawKind.Plus))
          _ = p.ExpectRaw(RawKind.Plus);
        else
          break;
      }

      GreenToken typeNameTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
      GreenNode refNode = typeNameTok;
      if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
      {
        refNode = GenericType.ParseAfterName(p, typeNameTok);
      }

      refs.Add(refNode);
      firstRef = false;

      if (p.Current.CoreToken.Offset == beforeRef) break;
    }

    StructuralArray<GreenNode> arr = [.. refs];
    if (arr.Count == 0) p.AddDiagInvalidGenericConstraint();
    return new ConstraintList(colonTok, arr);
  }
}
