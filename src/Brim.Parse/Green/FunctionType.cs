namespace Brim.Parse.Green;

public sealed record FunctionType(
  GreenToken OpenParen,
  StructuralArray<GreenNode> Parameters,
  GreenToken CloseParen,
  GreenNode ReturnType)
  : GreenNode(SyntaxKind.FunctionType, OpenParen.Offset)
{
  public override int FullWidth => ReturnType.EndOffset - OpenParen.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return OpenParen;
    foreach (GreenNode p in Parameters) yield return p;
    yield return CloseParen;
    yield return ReturnType;
  }

  public static FunctionType Parse(Parser p)
  {
    GreenToken open = p.ExpectSyntax(SyntaxKind.OpenParenToken);
    ImmutableArray<GreenNode>.Builder @params = ImmutableArray.CreateBuilder<GreenNode>();
    bool empty = p.MatchRaw(RawKind.RParen);
    if (!empty)
    {
      while (true)
      {
        int before = p.Current.CoreToken.Offset;
        GreenNode paramTy = TypeExpr.Parse(p);
        @params.Add(paramTy);
        if (p.MatchRaw(RawKind.Comma))
        {
          _ = p.ExpectSyntax(SyntaxKind.CommaToken);
          // trailing comma allowed; if next is close or EOB, stop
          if (p.MatchRaw(RawKind.RParen) || p.MatchRaw(RawKind.Eob))
            break;
          // else continue
        }
        else
        {
          break;
        }
        if (p.Current.CoreToken.Offset == before) break;
      }
    }
    StructuralArray<GreenNode> plist = [.. @params];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseParenToken);
    GreenNode ret = TypeExpr.Parse(p);
    return new FunctionType(open, plist, close, ret);
  }
}

