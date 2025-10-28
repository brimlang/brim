using Brim.Parse;
using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record BlockExpr(
  GreenToken OpenBrace,
  StructuralArray<GreenNode> Statements,
  ExprNode Result,
  GreenToken CloseBrace) :
ExprNode(SyntaxKind.BlockExpr, OpenBrace.Offset)
{
  public override int FullWidth => CloseBrace.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return OpenBrace;
    foreach (GreenNode stmt in Statements) yield return stmt;
    yield return Result;
    yield return CloseBrace;
  }

  internal static BlockExpr Parse(Parser p)
  {
    GreenToken openBrace = p.ExpectSyntax(SyntaxKind.OpenBraceToken);

    ArrayBuilder<GreenNode> statements = [];

    while (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      if (p.MatchRaw(RawKind.Terminator))
      {
        _ = p.ExpectSyntax(SyntaxKind.TerminatorToken);
        continue;
      }

      if (p.LooksLikeAssignment())
      {
        AssignmentStatement assignment = p.ParseAssignmentStatement();
        statements.Add(assignment);
        continue;
      }

      ExprNode expr = p.ParseExpression();

      if (p.MatchRaw(RawKind.Terminator))
      {
        GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
        
        // If closing brace or EOB follows, this is the final expression with trailing terminator
        if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob))
        {
          // The terminator is optional trailing, result is the expression
          ExprNode result = expr;
          GreenToken closeToken = p.ExpectSyntax(SyntaxKind.CloseBlockToken);
          return new BlockExpr(openBrace, statements.ToImmutable(), result, closeToken);
        }
        
        // Otherwise it's a statement and we continue parsing
        statements.Add(new ExpressionStatement(expr, term));
        continue;
      }

      // No terminator after expression, must be the final expression before }
      ExprNode resultExpr = expr;
      GreenToken closingBrace = p.ExpectSyntax(SyntaxKind.CloseBlockToken);
      return new BlockExpr(openBrace, statements.ToImmutable(), resultExpr, closingBrace);
    }

    ExprNode missing = new LiteralExpr(p.FabricateMissing(SyntaxKind.IdentifierToken, RawKind.Identifier));
    GreenToken closeBrace = p.ExpectSyntax(SyntaxKind.CloseBlockToken);
    return new BlockExpr(openBrace, statements.ToImmutable(), missing, closeBrace);
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
    LiteralExpr placeholder = new(new GreenToken(SyntaxKind.ErrorToken, new RawToken(RawKind.Error, closeBrace.Token.Offset, 0, closeBrace.Token.Line, closeBrace.Token.Column)));
    return new BlockExpr(openBrace, StructuralArray<GreenNode>.Empty, placeholder, closeBrace);
  }
}
