namespace Brim.Parse.Green;

public sealed record NamedTupleShape(
  GreenToken OpenToken,
  StructuralArray<NamedTupleElement> Elements,
  GreenToken CloseBrace)
  : GreenNode(SyntaxKind.NamedTupleShape, OpenToken.Offset)
{
  public override int FullWidth => CloseBrace.EndOffset - OpenToken.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return OpenToken;
    foreach (NamedTupleElement e in Elements) yield return e;
    yield return CloseBrace;
  }

  public static NamedTupleShape Parse(Parser p)
  {
    GreenToken open = p.ExpectSyntax(SyntaxKind.NamedTupleToken);

    ImmutableArray<NamedTupleElement>.Builder elems = ImmutableArray.CreateBuilder<NamedTupleElement>();
    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        GreenToken typeNameTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
        GreenNode ty = typeNameTok;
        if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
        {
          ty = GenericType.ParseAfterName(p, typeNameTok);
        }
        GreenToken? trailing = null;
        if (p.MatchRaw(RawKind.Comma)) trailing = p.ExpectSyntax(SyntaxKind.CommaToken);
        elems.Add(new NamedTupleElement(ty, trailing));
        if (trailing is null) break;
        if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob)) break; // trailing
      }
    }

    if (elems.Count == 0)
      p.AddDiagEmptyNamedTupleElementList();

    StructuralArray<NamedTupleElement> arr = [.. elems];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    return new NamedTupleShape(open, arr, close);
  }
}

