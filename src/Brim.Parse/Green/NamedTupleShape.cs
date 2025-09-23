using Brim.Parse.Collections;

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

    StructuralArray<NamedTupleElement> elems =
      Delimited.ParseCommaSeparatedTypes(
        p,
        static p2 =>
        {
          GreenToken typeNameTok2 = p2.ExpectSyntax(SyntaxKind.IdentifierToken);
          GreenNode ty2 = typeNameTok2;
          if (p2.MatchRaw(RawKind.LBracket) && !p2.MatchRaw(RawKind.LBracketLBracket))
            ty2 = GenericType.ParseAfterName(p2, typeNameTok2);
          return ty2;
        },
        static (n, c) => new NamedTupleElement(n, c),
        RawKind.RBrace);

    if (elems.Count == 0)
      p.AddDiagEmptyNamedTupleElementList();

    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBlockToken);
    return new NamedTupleShape(open, elems, close);
  }
}
