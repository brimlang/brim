using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record ServiceImpl(
  GreenToken Hat,
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
  : GreenNode(SyntaxKind.Block, Hat.Offset)
{
  public override int FullWidth => CloseBrace.EndOffset - Hat.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Hat;
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
    // '^' ServiceRef '<' recv '>' CtorParamsOpt '{' InitDecl* Member* '}'
    GreenToken hat = new(SyntaxKind.HatToken, p.ExpectRaw(RawKind.Hat));
    GreenToken head = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenNode sref = head;
    if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
      sref = GenericType.ParseAfterName(p, head);

    GreenToken recvOpen = p.ExpectSyntax(SyntaxKind.LessToken);
    GreenToken rid = p.ExpectSyntax(SyntaxKind.IdentifierToken); // allow '_'
    GreenToken recvClose = p.ExpectSyntax(SyntaxKind.GreaterToken);

    GreenToken? ctorOpen = null;
    StructuralArray<ServiceParam> ctorParams = [];
    GreenToken? ctorClose = null;
    if (p.MatchRaw(RawKind.LParen))
    {
      ctorOpen = p.ExpectSyntax(SyntaxKind.OpenParenToken);
      ctorParams = ParseParamDeclList(p);
      ctorClose = p.ExpectSyntax(SyntaxKind.CloseParenToken);
    }

    GreenToken ob = p.ExpectSyntax(SyntaxKind.OpenBraceToken);

    // Init declarations: (('@')? Ident ':' Type '=' ... Terminator)+
    ImmutableArray<ServiceFieldInit>.Builder inits = ImmutableArray.CreateBuilder<ServiceFieldInit>();
    while (true)
    {
      // Skip standalone syntax
      while (p.MatchRaw(RawKind.Terminator) || p.MatchRaw(RawKind.CommentTrivia))
        _ = p.ExpectRaw(p.Current.Kind);

      // Optional attributes would be handled here (future)

      bool looksLikeInit = p.MatchRaw(RawKind.Atmark) && p.MatchRaw(RawKind.Identifier, 1) && p.MatchRaw(RawKind.Colon, 2)
        || (p.MatchRaw(RawKind.Identifier) && p.MatchRaw(RawKind.Colon, 1));
      if (!looksLikeInit) break;

      GreenToken? at = null;
      if (p.MatchRaw(RawKind.Atmark))
        at = new GreenToken(SyntaxKind.AtToken, p.ExpectRaw(RawKind.Atmark));

      GreenToken fname = p.ExpectSyntax(SyntaxKind.IdentifierToken);
      GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
      GreenNode ftype = TypeExpr.Parse(p);
      GreenToken eq = p.ExpectSyntax(SyntaxKind.EqualToken);

      // Consume initializer structurally until Terminator
      while (!p.MatchRaw(RawKind.Terminator) && !p.MatchRaw(RawKind.Eob) && !p.MatchRaw(RawKind.RBrace))
        _ = p.ExpectRaw(p.Current.Kind);

      GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
      inits.Add(new ServiceFieldInit(at, fname, colon, ftype, eq, term));
    }

    // Optional destructor immediately after init section
    ImmutableArray<GreenNode>.Builder memb = ImmutableArray.CreateBuilder<GreenNode>();
    if (p.MatchRaw(RawKind.Tilde))
      memb.Add(ParseDtorHeaderAndSkipBody(p));

    // Members: methods only; skip bodies
    while (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      if (p.MatchRaw(RawKind.Terminator)) { _ = p.ExpectRaw(RawKind.Terminator); continue; }
      if (p.MatchRaw(RawKind.LBrace)) { _ = p.ExpectSyntax(SyntaxKind.OpenBraceToken); SkipBlock(p); continue; }

      if (p.MatchRaw(RawKind.Identifier))
      {
        memb.Add(ParseMethodHeaderAndSkipBody(p));
        continue;
      }

      // Fallback
      _ = p.ExpectRaw(p.Current.Kind);
    }

    GreenToken cb = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    return new ServiceImpl(hat, sref, recvOpen, rid, recvClose, ctorOpen, ctorParams, ctorClose, ob, [.. inits], memb.ToImmutable(), cb);
  }

  static ServiceMethodHeader ParseMethodHeaderAndSkipBody(Parser p)
  {
    GreenToken name = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken op = p.ExpectSyntax(SyntaxKind.OpenParenToken);
    StructuralArray<ServiceParam> @params = ParseParamDeclList(p);
    GreenToken cp = p.ExpectSyntax(SyntaxKind.CloseParenToken);
    GreenNode ret = TypeExpr.Parse(p);
    GreenToken bodyOpen = p.ExpectSyntax(SyntaxKind.OpenBraceToken);
    SkipBlock(p);
    return new ServiceMethodHeader(name, op, @params, cp, ret, bodyOpen);
  }

  static ServiceDtorHeader ParseDtorHeaderAndSkipBody(Parser p)
  {
    GreenToken tilde = new(SyntaxKind.TildeToken, p.ExpectRaw(RawKind.Tilde));
    GreenToken op = p.ExpectSyntax(SyntaxKind.OpenParenToken);
    GreenToken cp = p.ExpectSyntax(SyntaxKind.CloseParenToken);
    GreenNode ret = TypeExpr.Parse(p);
    GreenToken bodyOpen = p.ExpectSyntax(SyntaxKind.OpenBraceToken);
    SkipBlock(p);
    return new ServiceDtorHeader(tilde, op, cp, ret, bodyOpen);
  }

  static StructuralArray<ServiceParam> ParseParamDeclList(Parser p)
  {
    if (p.MatchRaw(RawKind.RParen)) return [];
    ImmutableArray<ServiceParam>.Builder list = ImmutableArray.CreateBuilder<ServiceParam>();
    while (true)
    {
      Parser.StallGuard sg = p.GetStallGuard();
      GreenToken pname = p.ExpectSyntax(SyntaxKind.IdentifierToken);
      GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
      GreenNode ptype = TypeExpr.Parse(p);

      GreenToken? trailing = null;
      if (p.MatchRaw(RawKind.Comma))
        trailing = p.ExpectSyntax(SyntaxKind.CommaToken);

      list.Add(new ServiceParam(pname, colon, ptype, trailing));

      if (trailing is null) break;
      if (p.MatchRaw(RawKind.RParen) || p.MatchRaw(RawKind.Eob)) break;
      if (sg.Stalled) break;
    }

    return list;
  }

  static void SkipBlock(Parser p)
  {
    int depth = 1;
    while (!p.MatchRaw(RawKind.Eob) && depth > 0)
    {
      if (p.MatchRaw(RawKind.LBrace)) { _ = p.ExpectSyntax(SyntaxKind.OpenBraceToken); depth++; continue; }
      if (p.MatchRaw(RawKind.RBrace)) { depth--; if (depth == 0) break; _ = p.ExpectSyntax(SyntaxKind.CloseBraceToken); continue; }
      _ = p.ExpectRaw(p.Current.Kind);
    }
    _ = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
  }

  // Removed old state block parsing in favor of init decls
}

public sealed record ServiceFieldInit(
  GreenToken? Atmark,
  GreenToken Name,
  GreenToken Colon,
  GreenNode Type,
  GreenToken Equal,
  GreenToken Terminator)
  : GreenNode(SyntaxKind.FieldDeclaration, (Atmark ?? Name).Offset)
{
  public override int FullWidth => Terminator.EndOffset - (Atmark ?? Name).Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    if (Atmark is not null) yield return Atmark;
    yield return Name;
    yield return Colon;
    yield return Type;
    yield return Equal;
    yield return Terminator;
  }
}
