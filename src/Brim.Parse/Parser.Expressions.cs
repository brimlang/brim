using Brim.Parse.Collections;
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

  internal ExprNode ParseExpression() => ParseMatchExpression();

  ExprNode ParseMatchExpression()
  {
    ExprNode scrutinee = ParseBinaryExpression(0);
    if (MatchRaw(RawKind.EqualGreater))
    {
      GreenToken arrow = ExpectSyntax(SyntaxKind.ArrowToken);
      MatchArmList arms = ParseMatchArmList();
      return new MatchExpr(scrutinee, arrow, arms);
    }

    return scrutinee;
  }

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
        SyntaxKind opKind = MatchRaw(RawKind.Question) ? SyntaxKind.QuestionToken : SyntaxKind.BangToken;
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
      case RawKind.DecimalLiteral:
      case RawKind.StringLiteral:
      case RawKind.RuneLiteral:
        SyntaxKind literalKind = Current.Kind switch
        {
          RawKind.IntegerLiteral => SyntaxKind.IntToken,
          RawKind.DecimalLiteral => SyntaxKind.DecimalToken,
          RawKind.StringLiteral => SyntaxKind.StrToken,
          RawKind.RuneLiteral => SyntaxKind.RuneToken,
          _ => SyntaxKind.ErrorToken,
        };
        return new LiteralExpr(ExpectSyntax(literalKind));

      case RawKind.Pipe:
      case RawKind.PipePipeGreater:
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

  ParenthesizedExpr ParseParenthesizedExpression()
  {
    GreenToken open = ExpectSyntax(SyntaxKind.OpenParenToken);
    ExprNode inner = ParseExpression();
    GreenToken close = ExpectSyntax(SyntaxKind.CloseParenToken);
    return new ParenthesizedExpr(open, inner, close);
  }

  FunctionLiteral ParseLambdaExpression()
  {
    if (MatchRaw(RawKind.PipePipeGreater))
    {
      RawToken combined = ExpectRaw(RawKind.PipePipeGreater);
      RawToken openRaw = new(RawKind.Pipe, combined.Offset, 1, combined.Line, combined.Column);
      RawToken closeRaw = new(RawKind.PipeGreater, combined.Offset + 1, combined.Length - 1, combined.Line, combined.Column + 1);

      GreenToken open = new(SyntaxKind.LambdaOpenToken, openRaw);
      GreenToken close = new(SyntaxKind.LambdaCloseToken, closeRaw);
      LambdaParams parameters = LambdaParams.Empty(open.Offset + open.FullWidth);

      ExprNode emptyBody = MatchRaw(RawKind.LBrace) ? ParseBlockExpression() : ParseExpression();
      return new FunctionLiteral(open, parameters, close, emptyBody);
    }

    GreenToken openToken = ExpectSyntax(SyntaxKind.LambdaOpenToken);
    ArrayBuilder<LambdaParams.Parameter> builder = [];
    int parametersOffset = openToken.EndOffset;

    if (!MatchRaw(RawKind.PipeGreater))
    {
      GreenToken firstIdent = ExpectSyntax(SyntaxKind.IdentifierToken);
      parametersOffset = firstIdent.Offset;
      builder.Add(new LambdaParams.Parameter(null, firstIdent));

      while (MatchRaw(RawKind.Comma) && !MatchRaw(RawKind.PipeGreater, 1))
      {
        GreenToken comma = ExpectSyntax(SyntaxKind.CommaToken);
        GreenToken ident = ExpectSyntax(SyntaxKind.IdentifierToken);
        builder.Add(new LambdaParams.Parameter(comma, ident));
      }
    }

    GreenToken closeToken = ExpectSyntax(SyntaxKind.LambdaCloseToken);
    LambdaParams parametersList = builder.Count > 0
      ? LambdaParams.From(builder, parametersOffset)
      : LambdaParams.Empty(closeToken.Offset);

    ExprNode body = MatchRaw(RawKind.LBrace) ? ParseBlockExpression() : ParseExpression();
    return new FunctionLiteral(openToken, parametersList, closeToken, body);
  }

  BlockExpr ParseBlockExpression() => BlockExpr.Parse(this);

  MatchArmList ParseMatchArmList()
  {
    ArrayBuilder<MatchArm> arms = [];

    while (MatchRaw(RawKind.Terminator))
      _ = ExpectSyntax(SyntaxKind.TerminatorToken);

    while (!MatchRaw(RawKind.Eob))
    {
      Parser.StallGuard guard = GetStallGuard();
      MatchArm arm = ParseMatchArm();
      arms.Add(arm);

      if (arm.Terminator is null || guard.Stalled)
        break;

      while (MatchRaw(RawKind.Terminator))
        _ = ExpectSyntax(SyntaxKind.TerminatorToken);
    }

    return new MatchArmList(arms.ToImmutable());
  }

  MatchArm ParseMatchArm()
  {
    ExprNode pattern = ParseBinaryExpression(0);

    MatchGuard? guard = null;
    if (MatchRaw(RawKind.QuestionQuestion))
    {
      GreenToken guardToken = ExpectSyntax(SyntaxKind.MatchGuardToken);
      ExprNode condition = ParseBinaryExpression(0);
      guard = new MatchGuard(guardToken, condition);
    }

    GreenToken arrow = ExpectSyntax(SyntaxKind.ArrowToken);
    ExprNode target = ParseMatchExpression();

    GreenToken? terminator = null;
    if (MatchRaw(RawKind.Terminator))
      terminator = ExpectSyntax(SyntaxKind.TerminatorToken);

    return new MatchArm(pattern, guard, arrow, target, terminator);
  }

  internal bool LooksLikeAssignment()
  {
    int offset = 0;
    if (MatchRaw(RawKind.Hat, offset))
      offset++;

    if (!MatchRaw(RawKind.Identifier, offset))
      return false;

    offset++;

    while (MatchRaw(RawKind.Stop, offset) && MatchRaw(RawKind.Identifier, offset + 1))
    {
      offset += 2;
    }

    return MatchRaw(RawKind.Equal, offset);
  }

  internal AssignmentStatement ParseAssignmentStatement()
  {
    AssignmentTarget target = AssignmentTarget.Parse(this);
    GreenToken equal = ExpectSyntax(SyntaxKind.EqualToken);
    ExprNode value = ParseExpression();
    GreenToken terminator = ExpectSyntax(SyntaxKind.TerminatorToken);
    return new AssignmentStatement(target, equal, value, terminator);
  }

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

  static readonly BinaryOperatorInfo[] _binaryOperatorTable =
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

  static ReadOnlySpan<BinaryOperatorInfo> BinaryOperators => _binaryOperatorTable;

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
