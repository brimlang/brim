namespace Brim.Parse.Green;

public sealed record ServiceShape(
  GreenToken OpenToken, // '^{'
  StructuralArray<GreenNode> Protocols,
  GreenToken CloseBrace)
  : GreenNode(SyntaxKind.ServiceShape, OpenToken.Offset)
{
  public override int FullWidth => CloseBrace.EndOffset - OpenToken.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return OpenToken;
    foreach (GreenNode p in Protocols) yield return p;
    yield return CloseBrace;
  }

  public static ServiceShape Parse(Parser p)
  {
    // Expect '^' then '{'
    _ = p.ExpectSyntax(SyntaxKind.HatToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.OpenBraceToken);

    ImmutableArray<GreenNode>.Builder protos = ImmutableArray.CreateBuilder<GreenNode>();
    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        int before = p.Current.CoreToken.Offset;
        // ProtocolRef: Ident GenericArgs?
        GreenToken head = p.ExpectSyntax(SyntaxKind.IdentifierToken);
        GreenNode pref = head;
        if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
          pref = GenericType.ParseAfterName(p, head);
        protos.Add(pref);
        if (p.MatchRaw(RawKind.Comma))
        {
          _ = p.ExpectSyntax(SyntaxKind.CommaToken);
          if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob)) break;
          if (p.Current.CoreToken.Offset == before) break;
          continue;
        }
        break;
      }
    }

    StructuralArray<GreenNode> list = [.. protos];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    return new ServiceShape(open, list, close);
  }
}

