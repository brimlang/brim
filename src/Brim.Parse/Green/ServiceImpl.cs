namespace Brim.Parse.Green;

public sealed record ServiceImpl(
  GreenToken Sigil,
  GreenNode ServiceRef,
  GreenToken ReceiverOpen,
  GreenToken ReceiverIdent,
  GreenToken ReceiverClose,
  GreenToken? CtorOpenParen,
  StructuralArray<ServiceParam> CtorParams,
  GreenToken? CtorCloseParen,
  GreenToken OpenBrace,
  StructuralArray<ServiceFieldInit> InitDecls,
  StructuralArray<GreenNode> Members,
  GreenToken CloseBrace)
  : GreenNode(SyntaxKind.BlockExpr, Sigil.Offset)
{
  public override int FullWidth => CloseBrace.EndOffset - Sigil.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Sigil;
    yield return ServiceRef;
    yield return ReceiverOpen;
    yield return ReceiverIdent;
    yield return ReceiverClose;
    if (CtorOpenParen is not null) yield return CtorOpenParen;
    foreach (ServiceParam p in CtorParams) yield return p;
    if (CtorCloseParen is not null) yield return CtorCloseParen;
    yield return OpenBrace;
    foreach (ServiceFieldInit f in InitDecls) yield return f;
    foreach (GreenNode m in Members) yield return m;
    yield return CloseBrace;
  }

  public static ServiceImpl Parse(Parser p)
  {
    // '@' ServiceRef '<' recv '>' CtorParamsOpt '{' InitDecl* Member* '}'
    GreenToken sigil = p.Expect(SyntaxKind.ServiceImplToken);
    GreenToken head = p.Expect(SyntaxKind.IdentifierToken);
    GreenNode sref = head;
    if (p.Match(TokenKind.LBracket))
      sref = TypeRef.WithGenericArgs(p, head);

    GreenToken recvOpen = p.Expect(SyntaxKind.LessToken);
    GreenToken rid = p.Expect(SyntaxKind.IdentifierToken); // allow '_'
    GreenToken recvClose = p.Expect(SyntaxKind.GreaterToken);

    GreenToken? ctorOpen = null;
    StructuralArray<ServiceParam> ctorParams = [];
    GreenToken? ctorClose = null;
    if (p.Match(TokenKind.LParen))
    {
      ctorOpen = p.Expect(SyntaxKind.OpenParenToken);
      ctorParams = ParseParamDeclList(p);
      ctorClose = p.Expect(SyntaxKind.CloseParenToken);
    }

    GreenToken ob = p.Expect(SyntaxKind.OpenBlockToken);

    // Init declarations: ('^'? Ident ':' Type '=' ... Terminator)+
    ImmutableArray<ServiceFieldInit>.Builder inits = ImmutableArray.CreateBuilder<ServiceFieldInit>();
    while (true)
    {
      // Skip terminators
      // TODO: Preserve tokens in tree
      StructuralArray<GreenToken> _ = p.CollectSyntaxKind(SyntaxKind.TerminatorToken);

      // Optional attributes would be handled here (future)

      bool looksLikeInit = p.Match(TokenKind.Hat) && p.Match(TokenKind.Identifier, 1) && p.Match(TokenKind.Colon, 2)
        || (p.Match(TokenKind.Identifier) && p.Match(TokenKind.Colon, 1));
      if (!looksLikeInit) break;

      GreenToken? mutator = null;
      if (p.Match(TokenKind.Hat))
        mutator = p.Expect(SyntaxKind.MutableToken);

      GreenToken fname = p.Expect(SyntaxKind.IdentifierToken);
      GreenToken colon = p.Expect(SyntaxKind.ColonToken);
      TypeExpr ftype = TypeExpr.Parse(p);
      GreenToken eq = p.Expect(SyntaxKind.EqualToken);
      ExprNode initializer;
      if (p.Match(TokenKind.Terminator) || p.Match(TokenKind.Eob) || p.Match(TokenKind.RBrace))
      {
        GreenToken missing = p.FabricateMissing(SyntaxKind.IdentifierToken);
        initializer = new IdentifierExpr(missing);
      }
      else
      {
        initializer = p.ParseExpression();
      }
      GreenToken term = p.Expect(SyntaxKind.TerminatorToken);
      inits.Add(new ServiceFieldInit(mutator, fname, colon, ftype, eq, initializer, term));
    }

    // Optional destructor immediately after init section
    ArrayBuilder<GreenNode> memb = [];
    if (p.Match(TokenKind.Tilde))
      memb.Add(ParseDtorHeaderAndSkipBody(p));

    // Members: methods only; skip bodies
    while (!p.Match(TokenKind.RBrace))
    {
      Parser.StallGuard guard = p.GetStallGuard();

      if (p.Match(TokenKind.Terminator)) { _ = p.Expect(SyntaxKind.TerminatorToken); continue; }
      if (p.Match(TokenKind.LBrace)) { _ = p.Expect(SyntaxKind.OpenBlockToken); SkipBlock(p); continue; }

      if (p.Match(TokenKind.Identifier))
      {
        memb.Add(ParseMethodHeaderAndSkipBody(p));
        continue;
      }

      // Unexpected token - consume and continue
      memb.Add(p.UnexpectedTokenAsError());

      // Stall guard to prevent infinite loop
      if (guard.Stalled) break;
    }

    GreenToken cb = p.Expect(SyntaxKind.CloseBlockToken);
    return new ServiceImpl(
      Sigil: sigil,
      ServiceRef: sref,
      ReceiverOpen: recvOpen,
      ReceiverIdent: rid,
      ReceiverClose: recvClose,
      CtorOpenParen: ctorOpen,
      CtorParams: ctorParams,
      CtorCloseParen: ctorClose,
      OpenBrace: ob,
      InitDecls: [.. inits],
      Members: memb,
      CloseBrace: cb);
  }

  static ServiceMethodHeader ParseMethodHeaderAndSkipBody(Parser p)
  {
    GreenToken name = p.Expect(SyntaxKind.IdentifierToken);
    GreenToken op = p.Expect(SyntaxKind.OpenParenToken);
    StructuralArray<ServiceParam> @params = ParseParamDeclList(p);
    GreenToken cp = p.Expect(SyntaxKind.CloseParenToken);
    TypeExpr ret = TypeExpr.Parse(p);
    GreenToken bodyOpen = p.Expect(SyntaxKind.OpenBlockToken);
    SkipBlock(p);
    return new ServiceMethodHeader(name, op, @params, cp, ret, bodyOpen);
  }

  static ServiceDtorHeader ParseDtorHeaderAndSkipBody(Parser p)
  {
    GreenToken tilde = p.Expect(SyntaxKind.TildeToken);
    GreenToken op = p.Expect(SyntaxKind.OpenParenToken);
    GreenToken cp = p.Expect(SyntaxKind.CloseParenToken);
    TypeExpr ret = TypeExpr.Parse(p);
    GreenToken bodyOpen = p.Expect(SyntaxKind.OpenBlockToken);
    SkipBlock(p);
    return new ServiceDtorHeader(tilde, op, cp, ret, bodyOpen);
  }

  static StructuralArray<ServiceParam> ParseParamDeclList(Parser p)
  {
    if (p.Match(TokenKind.RParen)) return [];
    ImmutableArray<ServiceParam>.Builder list = ImmutableArray.CreateBuilder<ServiceParam>();
    while (true)
    {
      Parser.StallGuard sg = p.GetStallGuard();
      GreenToken pname = p.Expect(SyntaxKind.IdentifierToken);
      GreenToken colon = p.Expect(SyntaxKind.ColonToken);
      TypeExpr ptype = TypeExpr.Parse(p);

      GreenToken? trailing = null;
      if (p.Match(TokenKind.Comma))
        trailing = p.Expect(SyntaxKind.CommaToken);

      list.Add(new ServiceParam(pname, colon, ptype, trailing));

      if (trailing is null) break;
      if (p.Match(TokenKind.RParen) || p.Match(TokenKind.Eob)) break;
      if (sg.Stalled) break;
    }

    return list;
  }

  static void SkipBlock(Parser p)
  {
    int depth = 1;
    while (!p.Match(TokenKind.Eob) && depth > 0)
    {
      if (p.Match(TokenKind.LBrace)) { _ = p.Expect(SyntaxKind.OpenBlockToken); depth++; continue; }
      if (p.Match(TokenKind.RBrace)) { depth--; if (depth == 0) break; _ = p.Expect(SyntaxKind.CloseBlockToken); continue; }
    }
    _ = p.Expect(SyntaxKind.CloseBlockToken);
  }

  // Removed old state block parsing in favor of init decls
}

public sealed record ServiceFieldInit(
  GreenToken? Mutator,
  GreenToken Name,
  GreenToken Colon,
  TypeExpr Type,
  GreenToken Equal,
  ExprNode Initializer,
  GreenToken Terminator)
  : GreenNode(SyntaxKind.FieldDeclaration, (Mutator ?? Name).Offset)
{
  public override int FullWidth => Terminator.EndOffset - (Mutator ?? Name).Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    if (Mutator is not null) yield return Mutator;
    yield return Name;
    yield return Colon;
    foreach (GreenNode child in Type.GetChildren())
      yield return child;
    yield return Equal;
    yield return Initializer;
    yield return Terminator;
  }
}
