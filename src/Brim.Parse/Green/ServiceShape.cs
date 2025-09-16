using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record ServiceShape(
  GreenToken HatToken,
  GreenToken OpenToken,
  StructuralArray<ProtocolRef> Protocols,
  GreenToken CloseBrace)
  : GreenNode(SyntaxKind.ServiceShape, OpenToken.Offset)
{
  public override int FullWidth => CloseBrace.EndOffset - OpenToken.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return HatToken;
    yield return OpenToken;
    foreach (ProtocolRef p in Protocols) yield return p;
    yield return CloseBrace;
  }

  public static ServiceShape Parse(Parser p)
  {
    // Expect '^' then '{'
    GreenToken hat = p.ExpectSyntax(SyntaxKind.HatToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.OpenBraceToken);

    StructuralArray<ProtocolRef> list =
      Delimited.ParseCommaSeparatedTypes(
        p,
        static p2 =>
        {
          GreenToken head2 = p2.ExpectSyntax(SyntaxKind.IdentifierToken);
          GreenNode pref2 = head2;
          if (p2.MatchRaw(RawKind.LBracket) && !p2.MatchRaw(RawKind.LBracketLBracket))
            pref2 = GenericType.ParseAfterName(p2, head2);
          return pref2;
        },
        static (n, c) => new ProtocolRef(n, c),
        RawKind.RBrace);
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    return new ServiceShape(hat, open, list, close);
  }
}
