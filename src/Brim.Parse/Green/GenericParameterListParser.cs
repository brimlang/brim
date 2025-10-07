using Brim.Parse.Collections;

namespace Brim.Parse.Green;

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
      while (true)
      {
        GreenToken paramIdTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
        ConstraintList? constraints = ParseConstraints(p);
        GreenToken? trailing = null;
        if (p.MatchRaw(RawKind.Comma))
          trailing = p.ExpectSyntax(SyntaxKind.CommaToken);
        items.Add(new GenericParameter(paramIdTok, constraints, trailing));
        if (trailing is null) break; // no comma => end
        if (p.MatchRaw(RawKind.RBracket) || p.MatchRaw(RawKind.Eob)) break; // trailing comma
        continue;
      }
    }

    GreenToken close = p.ExpectSyntax(SyntaxKind.GenericCloseToken);
    if (empty)
      p.AddDiagEmptyGenericParam(open);

    return new GenericParameterList(open, items.ToImmutable(), close);
  }

  static ConstraintList? ParseConstraints(Parser p)
  {
    if (!p.MatchRaw(RawKind.Colon))
      return null;

    GreenToken colonTok = p.ExpectSyntax(SyntaxKind.ColonToken);
    ImmutableArray<ConstraintRef>.Builder refs = ImmutableArray.CreateBuilder<ConstraintRef>();
    while (!p.MatchRaw(RawKind.RBracket) && !p.MatchRaw(RawKind.Eob))
    {
      int beforeRef = p.Current.CoreToken.Offset;
      GreenToken typeNameTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
      GreenNode refNode = typeNameTok;
      if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
        refNode = TypeRef.ParseAfterName(p, typeNameTok);

      GreenToken? trailingPlus = null;
      if (p.MatchRaw(RawKind.Plus)) trailingPlus = new GreenToken(SyntaxKind.PlusToken, p.ExpectRaw(RawKind.Plus));
      refs.Add(new ConstraintRef(refNode, trailingPlus));

      if (trailingPlus is null) break;
      if (p.Current.CoreToken.Offset == beforeRef) break;
    }

    StructuralArray<ConstraintRef> arr = [.. refs];
    if (arr.Count == 0) p.AddDiagInvalidGenericConstraint();
    return new ConstraintList(colonTok, arr);
  }
}

