using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record BlockExpr(
  GreenToken OpenBrace,
  StructuralArray<GreenNode> Statements,
  GreenToken CloseBrace) :
ExprNode(SyntaxKind.BlockExpr, OpenBrace.Offset)
{
  public override int FullWidth => CloseBrace.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return OpenBrace;
    foreach (GreenNode stmt in Statements) yield return stmt;
    yield return CloseBrace;
  }

  internal static BlockExpr SkipBlock(Parser p)
  {
    GreenToken openBrace = p.ExpectSyntax(SyntaxKind.OpenBraceToken);

    ArrayBuilder<GreenNode> tokens = [];

    int depth = 1;
    while (!p.MatchRaw(RawKind.Eob) && depth > 0)
    {
      // Check for opening braces (including compound tokens)
      if (p.MatchRaw(RawKind.LBrace) ||
          p.MatchRaw(RawKind.AtmarkLBrace) ||
          p.MatchRaw(RawKind.StarLBrace) ||
          p.MatchRaw(RawKind.PipeLBrace) ||
          p.MatchRaw(RawKind.HashLBrace) ||
          p.MatchRaw(RawKind.PercentLBrace) ||
          p.MatchRaw(RawKind.StopLBrace) ||
          p.MatchRaw(RawKind.QuestionLBrace) ||
          p.MatchRaw(RawKind.BangLBrace) ||
          p.MatchRaw(RawKind.BangBangLBrace) ||
          p.MatchRaw(RawKind.AmpersandLBrace))
      {
        _ = p.ExpectRaw(p.Current.Kind);
        depth++;
        continue;
      }

      if (p.MatchRaw(RawKind.RBrace))
      {
        depth--;
        if (depth == 0) break;
        _ = p.ExpectSyntax(SyntaxKind.CloseBlockToken);
        continue;
      }

      tokens.Add(new GreenToken(SyntaxKind.Undefined, p.ExpectRaw(p.Current.Kind)));
    }

    GreenToken closeBrace = p.ExpectSyntax(SyntaxKind.CloseBlockToken);
    return new BlockExpr(openBrace, tokens, closeBrace);
  }
}
