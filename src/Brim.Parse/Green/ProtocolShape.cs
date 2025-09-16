using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record ProtocolShape(
  GreenToken OpenToken, // '.{'
  StructuralArray<MethodSignature> Methods,
  GreenToken CloseBrace)
  : GreenNode(SyntaxKind.ProtocolShape, OpenToken.Offset)
{
  public override int FullWidth => CloseBrace.EndOffset - OpenToken.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return OpenToken;
    foreach (MethodSignature m in Methods) yield return m;
    yield return CloseBrace;
  }

  public static ProtocolShape Parse(Parser p)
  {
    GreenToken open = new(SyntaxKind.ProtocolToken, p.ExpectRaw(RawKind.StopLBrace));
    ImmutableArray<MethodSignature>.Builder methods = ImmutableArray.CreateBuilder<MethodSignature>();

    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        int before = p.Current.CoreToken.Offset;
        MethodSignature m = MethodSignature.Parse(p);
        methods.Add(m);
        if (m.TrailingComma is null) break;
        if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob)) break;
        if (p.Current.CoreToken.Offset == before) break;
      }
    }

    StructuralArray<MethodSignature> list = [.. methods];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    return new ProtocolShape(open, list, close);
  }
}

