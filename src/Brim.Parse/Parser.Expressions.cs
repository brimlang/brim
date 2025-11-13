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
    TokenKind Kind,
    byte Precedence,
    OperatorAssociativity Associativity);

  internal ExprNode ParseExpression() => ParseMatchExpression();

  ExprNode ParseMatchExpression()
  {
    ExprNode scrutinee = ParseBinaryExpression(0);
    if (Match(TokenKind.EqualGreater))
    {
      GreenToken arrow = Expect(SyntaxKind.ArrowToken);

      // Check if this is a multi-line match (with braces) or single-line
      if (Match(TokenKind.LBrace))
      {
        // Multi-line match: => { arms }
        MatchBlock block = ParseMatchBlock();
        return new MatchExpr(scrutinee, arrow, block);
      }
      else
      {
        // Single-line match: => arm
        MatchArm arm = ParseMatchArm();
        return new MatchExpr(scrutinee, arrow, arm);
      }
    }

    return scrutinee;
  }

  ExprNode ParseBinaryExpression(byte minPrecedence)
  {
    ExprNode left = ParseUnaryExpression();

    while (TryGetBinaryOperatorInfo(Current.TokenKind, out BinaryOperatorInfo info) && info.Precedence >= minPrecedence)
    {
      SyntaxKind opKind = MapBinaryOperatorKind(info.Kind);
      GreenToken opToken = Expect(opKind);
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
    if (IsPrefixOperator(Current.TokenKind))
    {
      SyntaxKind opKind = MapPrefixOperatorKind(Current.TokenKind);
      GreenToken opToken = Expect(opKind);
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
      if (Match(TokenKind.LParen))
      {
        ArgumentList args = ParseArgumentList();
        expr = new CallExpr(expr, args);
        continue;
      }

      if (Match(TokenKind.Stop))
      {
        GreenToken dot = Expect(SyntaxKind.StopToken);
        GreenToken member = Expect(SyntaxKind.IdentifierToken);
        expr = new MemberAccessExpr(expr, dot, member);
        continue;
      }

      if (Match(TokenKind.Question) || Match(TokenKind.Bang))
      {
        SyntaxKind opKind = Match(TokenKind.Question) ? SyntaxKind.QuestionToken : SyntaxKind.BangToken;
        GreenToken op = Expect(opKind);
        expr = new PropagationExpr(expr, op);
        continue;
      }

      if (Match(TokenKind.ColonGreater))
      {
        GreenToken cast = Expect(SyntaxKind.CastToken);
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
    switch (Current.TokenKind)
    {
      case TokenKind.Identifier:
        return ParseIdentifierOrConstruct();

      case TokenKind.IntegerLiteral:
      case TokenKind.DecimalLiteral:
      case TokenKind.StringLiteral:
      case TokenKind.RuneLiteral:
        SyntaxKind literalKind = Current.TokenKind switch
        {
          TokenKind.IntegerLiteral => SyntaxKind.IntToken,
          TokenKind.DecimalLiteral => SyntaxKind.DecimalToken,
          TokenKind.StringLiteral => SyntaxKind.StrToken,
          TokenKind.RuneLiteral => SyntaxKind.RuneToken,
          _ => SyntaxKind.ErrorToken,
        };
        return new LiteralExpr(Expect(literalKind));

      case TokenKind.Pipe:
      case TokenKind.PipePipeGreater:
        return ParseLambdaExpression();

      case TokenKind.LParen:
        return ParseParenthesizedExpression();

      case TokenKind.LBrace:
        return ParseBlockExpression();

      case TokenKind.QuestionLBrace:
        return ParseOptionConstruct();

      case TokenKind.BangLBrace:
        return ParseResultConstruct();

      case TokenKind.BangBangLBrace:
        return ParseErrorConstruct();

      case TokenKind.AtmarkLBrace:
        return ParseBareServiceConstruct();

      default:
        _diags.Add(Diagnostic.Parse.Unexpected(Current, ReadOnlySpan<TokenKind>.Empty));
        CoreToken curr = Current;
        Advance();
        GreenToken error = SyntaxKind.ErrorToken.MakeGreen(curr);
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
    GreenToken open = Expect(SyntaxKind.OpenParenToken);
    ExprNode inner = ParseExpression();
    GreenToken close = Expect(SyntaxKind.CloseParenToken);
    return new ParenthesizedExpr(open, inner, close);
  }

  ExprNode ParseLambdaExpression()
  {
    if (Match(TokenKind.PipePipeGreater))
    {
      GreenToken open = Expect(SyntaxKind.EmptyLambaToken);
      ExprNode emptyBody = Match(TokenKind.LBrace) ? ParseBlockExpression() : ParseExpression();

      return new ZeroParameterFunctionLiteral(open, emptyBody);
    }

    GreenToken openToken = Expect(SyntaxKind.LambdaOpenToken);
    ArrayBuilder<LambdaParams.Parameter> builder = [];
    int parametersOffset = openToken.EndOffset;

    if (!Match(TokenKind.PipeGreater))
    {
      GreenToken firstIdent = Expect(SyntaxKind.IdentifierToken);
      parametersOffset = firstIdent.Offset;
      builder.Add(new LambdaParams.Parameter(null, firstIdent));

      while (Match(TokenKind.Comma) && !Match(TokenKind.PipeGreater, 1))
      {
        GreenToken comma = Expect(SyntaxKind.CommaToken);
        GreenToken ident = Expect(SyntaxKind.IdentifierToken);
        builder.Add(new LambdaParams.Parameter(comma, ident));
      }
    }

    GreenToken closeToken = Expect(SyntaxKind.LambdaCloseToken);
    LambdaParams parametersList = builder.Count > 0
      ? LambdaParams.From(builder, parametersOffset)
      : LambdaParams.Empty(closeToken.Offset);

    ExprNode body = Match(TokenKind.LBrace) ? ParseBlockExpression() : ParseExpression();
    return new FunctionLiteral(openToken, parametersList, closeToken, body);
  }

  BlockExpr ParseBlockExpression() => BlockExpr.Parse(this);

  MatchArmList ParseMatchArmList()
  {
    ArrayBuilder<MatchArm> arms = [];

    while (Match(TokenKind.Terminator))
      _ = Expect(SyntaxKind.TerminatorToken);

    while (!Match(TokenKind.Eob))
    {
      StallGuard guard = GetStallGuard();
      MatchArm arm = ParseMatchArm();
      arms.Add(arm);

      if (arm.Terminator is null || guard.Stalled)
        break;

      while (Match(TokenKind.Terminator))
        _ = Expect(SyntaxKind.TerminatorToken);
    }

    return new MatchArmList(arms.ToImmutable());
  }

  MatchBlock ParseMatchBlock()
  {
    GreenToken openBrace = Expect(SyntaxKind.OpenBlockToken);

    // Optional leading terminator
    GreenToken? leadingTerminator = null;
    if (Match(TokenKind.Terminator))
      leadingTerminator = Expect(SyntaxKind.TerminatorToken);

    ArrayBuilder<MatchBlock.Element> arms = [];

    // Parse arms until we hit closing brace
    while (!Match(TokenKind.RBrace) && !Match(TokenKind.Eob))
    {
      StallGuard guard = GetStallGuard();

      // Optional leading terminator before this arm (for 2nd+ arms)
      GreenToken? armLeadingTerm = null;
      if (arms.Count > 0 && Match(TokenKind.Terminator))
        armLeadingTerm = Expect(SyntaxKind.TerminatorToken);

      // Parse the arm itself (which includes its own optional terminator)
      MatchArm arm = ParseMatchArm();
      arms.Add(new MatchBlock.Element(armLeadingTerm, arm));

      if (guard.Stalled)
        break;
    }

    // Optional trailing terminator before close
    GreenToken? trailingTerminator = null;
    if (Match(TokenKind.Terminator))
      trailingTerminator = Expect(SyntaxKind.TerminatorToken);

    GreenToken closeBrace = Expect(SyntaxKind.CloseBlockToken);
    return new MatchBlock(openBrace, leadingTerminator, arms.ToImmutable(), trailingTerminator, closeBrace);
  }

  MatchArm ParseMatchArm()
  {
    PatternNode pattern = PatternNode.Parse(this);

    MatchGuard? guard = null;
    if (Match(TokenKind.QuestionQuestion))
    {
      GreenToken guardToken = Expect(SyntaxKind.MatchGuardToken);
      ExprNode condition = ParseBinaryExpression(0);
      guard = new MatchGuard(guardToken, condition);
    }

    GreenToken arrow = Expect(SyntaxKind.ArrowToken);
    ExprNode target = ParseMatchExpression();

    GreenToken? terminator = null;
    if (Match(TokenKind.Terminator))
      terminator = Expect(SyntaxKind.TerminatorToken);

    return new MatchArm(pattern, guard, arrow, target, terminator);
  }

  internal bool LooksLikeAssignment()
  {
    int offset = 0;
    if (Match(TokenKind.Hat, offset))
      offset++;

    if (!Match(TokenKind.Identifier, offset))
      return false;

    offset++;

    while (Match(TokenKind.Stop, offset) && Match(TokenKind.Identifier, offset + 1))
    {
      offset += 2;
    }

    return Match(TokenKind.Equal, offset);
  }

  internal AssignmentStatement ParseAssignmentStatement()
  {
    AssignmentTarget target = AssignmentTarget.Parse(this);
    GreenToken equal = Expect(SyntaxKind.EqualToken);
    ExprNode value = ParseExpression();
    GreenToken terminator = Expect(SyntaxKind.TerminatorToken);
    return new AssignmentStatement(target, equal, value, terminator);
  }

  static bool IsPrefixOperator(TokenKind kind) => kind is TokenKind.Minus or TokenKind.Bang;

  static SyntaxKind MapPrefixOperatorKind(TokenKind kind) => kind switch
  {
    TokenKind.Minus => SyntaxKind.MinusToken,
    TokenKind.Bang => SyntaxKind.BangToken,
    _ => SyntaxKind.ErrorToken,
  };

  static SyntaxKind MapBinaryOperatorKind(TokenKind kind) => kind switch
  {
    TokenKind.Star => SyntaxKind.StarToken,
    TokenKind.Slash => SyntaxKind.SlashToken,
    TokenKind.Percent => SyntaxKind.PercentToken,
    TokenKind.Plus => SyntaxKind.PlusToken,
    TokenKind.Minus => SyntaxKind.MinusToken,
    TokenKind.Less => SyntaxKind.LessToken,
    TokenKind.Greater => SyntaxKind.GreaterToken,
    TokenKind.LessEqual => SyntaxKind.LessEqualToken,
    TokenKind.GreaterEqual => SyntaxKind.GreaterEqualToken,
    TokenKind.EqualEqual => SyntaxKind.EqualEqualToken,
    TokenKind.BangEqual => SyntaxKind.BangEqualToken,
    TokenKind.AmpersandAmpersand => SyntaxKind.AmpersandAmpersandToken,
    TokenKind.PipePipe => SyntaxKind.PipePipeToken,
    _ => SyntaxKind.ErrorToken,
  };

  static readonly BinaryOperatorInfo[] _binaryOperatorTable =
  [
    new BinaryOperatorInfo(TokenKind.Star, 80, OperatorAssociativity.Left),
    new BinaryOperatorInfo(TokenKind.Slash, 80, OperatorAssociativity.Left),
    new BinaryOperatorInfo(TokenKind.Percent, 80, OperatorAssociativity.Left),
    new BinaryOperatorInfo(TokenKind.Plus, 75, OperatorAssociativity.Left),
    new BinaryOperatorInfo(TokenKind.Minus, 75, OperatorAssociativity.Left),
    new BinaryOperatorInfo(TokenKind.Less, 70, OperatorAssociativity.None),
    new BinaryOperatorInfo(TokenKind.Greater, 70, OperatorAssociativity.None),
    new BinaryOperatorInfo(TokenKind.LessEqual, 70, OperatorAssociativity.None),
    new BinaryOperatorInfo(TokenKind.GreaterEqual, 70, OperatorAssociativity.None),
    new BinaryOperatorInfo(TokenKind.EqualEqual, 65, OperatorAssociativity.None),
    new BinaryOperatorInfo(TokenKind.BangEqual, 65, OperatorAssociativity.None),
    new BinaryOperatorInfo(TokenKind.AmpersandAmpersand, 50, OperatorAssociativity.Left),
    new BinaryOperatorInfo(TokenKind.PipePipe, 45, OperatorAssociativity.Left),
  ];

  static ReadOnlySpan<BinaryOperatorInfo> BinaryOperators => _binaryOperatorTable;

  internal static bool TryGetBinaryOperatorInfo(TokenKind kind, out BinaryOperatorInfo info)
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

  ExprNode ParseIdentifierOrConstruct()
  {
    GreenToken ident = Expect(SyntaxKind.IdentifierToken);

    return Current.TokenKind switch
    {
      TokenKind.PercentLBrace => ParseStructConstruct(ident),
      TokenKind.PipeLBrace => ParseUnionConstruct(ident),
      TokenKind.HashLBrace => ParseTupleConstruct(ident),
      TokenKind.AmpersandLBrace => ParseFlagsConstruct(ident),
      TokenKind.AtmarkLBrace => ParseServiceConstruct(ident),
      TokenKind.LBracket => ParseSeqConstruct(ident),
      _ => new IdentifierExpr(ident),
    };
  }

  StructConstruct ParseStructConstruct(GreenToken typeIdent)
  {
    QualifiedIdent qualifiedIdent = new([], typeIdent);
    TypeRef typeRef = new(qualifiedIdent, null);

    CommaList<FieldInit> fields = CommaList<FieldInit>.Parse(
      this,
      SyntaxKind.StructToken,
      SyntaxKind.CloseBlockToken,
      FieldInit.Parse);

    return new StructConstruct(typeRef, fields);
  }

  UnionConstruct ParseUnionConstruct(GreenToken typeIdent)
  {
    QualifiedIdent qualifiedIdent = new([], typeIdent);
    TypeRef typeRef = new(qualifiedIdent, null);

    GreenToken unionOpen = Expect(SyntaxKind.UnionToken);
    VariantInit variant = VariantInit.Parse(this);
    GreenToken closeBrace = Expect(SyntaxKind.CloseBlockToken);

    return new UnionConstruct(typeRef, unionOpen, variant, closeBrace);
  }

  TupleConstruct ParseTupleConstruct(GreenToken typeIdent)
  {
    QualifiedIdent qualifiedIdent = new([], typeIdent);
    TypeRef typeRef = new(qualifiedIdent, null);

    CommaList<ExprNode> elements = CommaList<ExprNode>.Parse(
      this,
      SyntaxKind.NamedTupleToken,
      SyntaxKind.CloseBlockToken,
      p => p.ParseExpression());

    return new TupleConstruct(typeRef, elements);
  }

  FlagsConstruct ParseFlagsConstruct(GreenToken typeIdent)
  {
    QualifiedIdent qualifiedIdent = new([], typeIdent);
    TypeRef typeRef = new(qualifiedIdent, null);

    CommaList<GreenToken> flags = CommaList<GreenToken>.Parse(
      this,
      SyntaxKind.FlagsToken,
      SyntaxKind.CloseBlockToken,
      p => p.Expect(SyntaxKind.IdentifierToken));

    return new FlagsConstruct(typeRef, flags);
  }

  ServiceConstruct ParseServiceConstruct(GreenToken typeIdent)
  {
    QualifiedIdent qualifiedIdent = new([], typeIdent);
    TypeRef typeRef = new(qualifiedIdent, null);

    CommaList<FieldInit> fields = CommaList<FieldInit>.Parse(
      this,
      SyntaxKind.ServiceToken,
      SyntaxKind.CloseBlockToken,
      FieldInit.Parse);

    return new ServiceConstruct(typeRef, fields);
  }

  ServiceConstruct ParseBareServiceConstruct()
  {
    // Bare @{ field = expr, ... } without type prefix
    // Create a missing/empty type reference
    GreenToken missingType = FabricateMissing(SyntaxKind.IdentifierToken);
    QualifiedIdent qualifiedIdent = new([], missingType);
    TypeRef typeRef = new(qualifiedIdent, null);

    CommaList<FieldInit> fields = CommaList<FieldInit>.Parse(
      this,
      SyntaxKind.ServiceToken,
      SyntaxKind.CloseBlockToken,
      FieldInit.Parse);

    return new ServiceConstruct(typeRef, fields);
  }

  SeqConstruct ParseSeqConstruct(GreenToken seqIdent)
  {
    GenericArgumentList? genericArgs = null;
    if (Match(TokenKind.LBracket))
      genericArgs = GenericArgumentList.Parse(this);

    CommaList<ExprNode> elements = CommaList<ExprNode>.Parse(
      this,
      SyntaxKind.OpenBlockToken,
      SyntaxKind.CloseBlockToken,
      p => p.ParseExpression());

    return new SeqConstruct(seqIdent, genericArgs, elements);
  }

  OptionConstruct ParseOptionConstruct()
  {
    GreenToken optionOpen = Expect(SyntaxKind.OptionConstruct);

    ExprNode? value = null;
    if (!Match(TokenKind.RBrace))
      value = ParseExpression();

    GreenToken closeBrace = Expect(SyntaxKind.CloseBlockToken);
    return new OptionConstruct(optionOpen, value, closeBrace);
  }

  ResultConstruct ParseResultConstruct()
  {
    GreenToken resultOpen = Expect(SyntaxKind.ResultConstruct);

    ExprNode value = ParseExpression();
    GreenToken closeBrace = Expect(SyntaxKind.CloseBlockToken);
    return new ResultConstruct(resultOpen, value, closeBrace);
  }

  ErrorConstruct ParseErrorConstruct()
  {
    GreenToken errorOpen = Expect(SyntaxKind.ErrorConstruct);

    ExprNode value = ParseExpression();
    GreenToken closeBrace = Expect(SyntaxKind.CloseBlockToken);
    return new ErrorConstruct(errorOpen, value, closeBrace);
  }
}
