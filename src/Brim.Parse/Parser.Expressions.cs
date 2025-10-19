using System;
using Brim.Parse.Green;

namespace Brim.Parse;

public sealed partial class Parser
{
  internal enum OperatorAssociativity : byte
  {
    Left,
    Right,
    None,
  }

  internal readonly record struct BinaryOperatorInfo(
    RawKind Kind,
    byte Precedence,
    OperatorAssociativity Associativity);

  internal ExprNode ParseExpression() => ParseBinaryExpression(0);

  ExprNode ParseBinaryExpression(byte minPrecedence)
  {
    ExprNode left = ParseUnaryExpression();

    while (TryGetBinaryOperatorInfo(Current.Kind, out BinaryOperatorInfo info) && info.Precedence >= minPrecedence)
    {
      SyntaxKind opKind = MapBinaryOperatorKind(info.Kind);
      GreenToken opToken = ExpectSyntax(opKind);
      byte nextPrecedence = info.Associativity switch
      {
        OperatorAssociativity.Left => (byte)(info.Precedence + 1),
        OperatorAssociativity.None => (byte)(info.Precedence + 1),
        _ => info.Precedence,
      };
      ExprNode right = ParseBinaryExpression(nextPrecedence);
      left = new BinaryExpr(left, opToken, right);
    }

    return left;
  }

  ExprNode ParseUnaryExpression()
  {
    if (IsPrefixOperator(Current.Kind))
    {
      SyntaxKind opKind = MapPrefixOperatorKind(Current.Kind);
      GreenToken opToken = ExpectSyntax(opKind);
      ExprNode operand = ParseUnaryExpression();
      return new UnaryExpr(opToken, operand);
    }

    return ParseCallOrAccessExpression();
  }

  ExprNode ParseCallOrAccessExpression()
  {
    ExprNode expr = ParsePrimaryExpression();

    while (true)
    {
      if (MatchRaw(RawKind.LParen))
      {
        ArgumentList args = ParseArgumentList();
        expr = new CallExpr(expr, args);
        continue;
      }

      if (MatchRaw(RawKind.Stop))
      {
        GreenToken dot = ExpectSyntax(SyntaxKind.StopToken);
        GreenToken member = ExpectSyntax(SyntaxKind.IdentifierToken);
        expr = new MemberAccessExpr(expr, dot, member);
        continue;
      }

      if (MatchRaw(RawKind.Question) || MatchRaw(RawKind.Bang))
      {
        SyntaxKind opKind = Current.Kind == RawKind.Question
          ? SyntaxKind.QuestionToken
          : SyntaxKind.BangToken;
        GreenToken op = ExpectSyntax(opKind);
        expr = new PropagationExpr(expr, op);
        continue;
      }

      if (MatchRaw(RawKind.ColonGreater))
      {
        GreenToken cast = ExpectSyntax(SyntaxKind.CastToken);
        TypeExpr type = TypeExpr.Parse(this);
        expr = new CastExpr(expr, cast, type);
        continue;
      }

      break;
    }

    return expr;
  }

  ExprNode ParsePrimaryExpression()
  {
    switch (Current.Kind)
    {
      case RawKind.Identifier:
        return new IdentifierExpr(ExpectSyntax(SyntaxKind.IdentifierToken));
      case RawKind.IntegerLiteral:
        return new LiteralExpr(ExpectSyntax(SyntaxKind.IntToken));
      case RawKind.DecimalLiteral:
        return new LiteralExpr(ExpectSyntax(SyntaxKind.DecimalToken));
      case RawKind.StringLiteral:
        return new LiteralExpr(ExpectSyntax(SyntaxKind.StrToken));
      case RawKind.RuneLiteral:
        return new LiteralExpr(ExpectSyntax(SyntaxKind.RuneToken));
      case RawKind.MinusGreater:
        return ParseLambdaExpression();
      case RawKind.LParen:
        return ParseParenthesizedExpression();
      case RawKind.LBrace:
        return ParseBlockExpression();
      default:
        RawToken unexpected = ExpectRaw(Current.Kind);
        _diags.Add(Diagnostic.Unexpected(unexpected, []));
        GreenToken error = new(SyntaxKind.ErrorToken, unexpected);
        return new LiteralExpr(error);
    }
  }

  ArgumentList ParseArgumentList()
  {
    CommaList<ExprNode> list = CommaList<ExprNode>.Parse(
      this,
      SyntaxKind.OpenParenToken,
      SyntaxKind.CloseParenToken,
      p => p.ParseExpression());

    return new ArgumentList(list);
  }

  ExprNode ParseParenthesizedExpression()
  {
    GreenToken open = ExpectSyntax(SyntaxKind.OpenParenToken);
    ExprNode inner = ParseExpression();
    GreenToken close = ExpectSyntax(SyntaxKind.CloseParenToken);
    return new ParenthesizedExpr(open, inner, close);
  }

  ExprNode ParseLambdaExpression()
  {
    GreenToken sigil = ExpectSyntax(SyntaxKind.LambdaArrowToken);
    LambdaParams parameters = LambdaParams.ParseAfterArrow(this);

    ExprNode body = MatchRaw(RawKind.LBrace)
      ? ParseBlockExpression()
      : ParseExpression();

    return new FunctionLiteral(sigil, parameters, body);
  }

  ExprNode ParseBlockExpression() => BlockExpr.SkipBlock(this);

  static bool IsPrefixOperator(RawKind kind) => kind is RawKind.Minus or RawKind.Bang;

  static SyntaxKind MapPrefixOperatorKind(RawKind kind) => kind switch
  {
    RawKind.Minus => SyntaxKind.MinusToken,
    RawKind.Bang => SyntaxKind.BangToken,
    _ => SyntaxKind.ErrorToken,
  };

  static SyntaxKind MapBinaryOperatorKind(RawKind kind) => kind switch
  {
    RawKind.Star => SyntaxKind.StarToken,
    RawKind.Slash => SyntaxKind.SlashToken,
    RawKind.Percent => SyntaxKind.PercentToken,
    RawKind.Plus => SyntaxKind.PlusToken,
    RawKind.Minus => SyntaxKind.MinusToken,
    RawKind.Less => SyntaxKind.LessToken,
    RawKind.Greater => SyntaxKind.GreaterToken,
    RawKind.LessEqual => SyntaxKind.LessEqualToken,
    RawKind.GreaterEqual => SyntaxKind.GreaterEqualToken,
    RawKind.EqualEqual => SyntaxKind.EqualEqualToken,
    RawKind.BangEqual => SyntaxKind.BangEqualToken,
    RawKind.AmpersandAmpersand => SyntaxKind.AmpersandAmpersandToken,
    RawKind.PipePipe => SyntaxKind.PipePipeToken,
    _ => SyntaxKind.ErrorToken,
  };

  static readonly BinaryOperatorInfo[] BinaryOperatorTable =
  [
    new BinaryOperatorInfo(RawKind.Star, 80, OperatorAssociativity.Left),
    new BinaryOperatorInfo(RawKind.Slash, 80, OperatorAssociativity.Left),
    new BinaryOperatorInfo(RawKind.Percent, 80, OperatorAssociativity.Left),
    new BinaryOperatorInfo(RawKind.Plus, 75, OperatorAssociativity.Left),
    new BinaryOperatorInfo(RawKind.Minus, 75, OperatorAssociativity.Left),
    new BinaryOperatorInfo(RawKind.Less, 70, OperatorAssociativity.None),
    new BinaryOperatorInfo(RawKind.Greater, 70, OperatorAssociativity.None),
    new BinaryOperatorInfo(RawKind.LessEqual, 70, OperatorAssociativity.None),
    new BinaryOperatorInfo(RawKind.GreaterEqual, 70, OperatorAssociativity.None),
    new BinaryOperatorInfo(RawKind.EqualEqual, 65, OperatorAssociativity.None),
    new BinaryOperatorInfo(RawKind.BangEqual, 65, OperatorAssociativity.None),
    new BinaryOperatorInfo(RawKind.AmpersandAmpersand, 50, OperatorAssociativity.Left),
    new BinaryOperatorInfo(RawKind.PipePipe, 45, OperatorAssociativity.Left),
  ];

  static ReadOnlySpan<BinaryOperatorInfo> BinaryOperators => BinaryOperatorTable;

  internal static bool TryGetBinaryOperatorInfo(RawKind kind, out BinaryOperatorInfo info)
  {
    foreach (BinaryOperatorInfo candidate in BinaryOperators)
    {
      if (candidate.Kind == kind)
      {
        info = candidate;
        return true;
      }
    }

    info = default;
    return false;
  }
}
