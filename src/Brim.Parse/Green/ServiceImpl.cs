namespace Brim.Parse.Green;

public sealed record ServiceImpl(
  GreenNode ServiceRef,
  GreenToken ReceiverIdent,
  GreenToken OpenBrace,
  StructuralArray<ServiceStateField> StateFields,
  StructuralArray<GreenNode> Members,
  GreenToken CloseBrace)
  : GreenNode(SyntaxKind.Block, ServiceRef.Offset)
{
  public override int FullWidth => CloseBrace.EndOffset - ServiceRef.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return ServiceRef;
    yield return ReceiverIdent;
    yield return OpenBrace;
    foreach (ServiceStateField f in StateFields) yield return f;
    foreach (GreenNode m in Members) yield return m;
    yield return CloseBrace;
  }

  public static ServiceImpl Parse(Parser p)
  {
    // '<' ServiceRef ',' recv '>' '{' StateBlock Member* '}'
    _ = p.ExpectRaw(RawKind.Less);
    GreenToken head = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenNode sref = head;
    if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
      sref = GenericType.ParseAfterName(p, head);
    GreenToken comma = p.ExpectSyntax(SyntaxKind.CommaToken);
    GreenToken rid = p.ExpectSyntax(SyntaxKind.IdentifierToken); // allow '_'
    _ = p.ExpectRaw(RawKind.Greater);
    GreenToken ob = p.ExpectSyntax(SyntaxKind.OpenBraceToken);

    StructuralArray<ServiceStateField> fields = ParseStateBlock(p);

    // Parse members structurally (headers only), skip bodies
    ImmutableArray<GreenNode>.Builder memb = ImmutableArray.CreateBuilder<GreenNode>();
    while (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      if (p.MatchRaw(RawKind.Terminator)) { _ = p.ExpectRaw(RawKind.Terminator); continue; }
      if (p.MatchRaw(RawKind.LBrace)) { _ = p.ExpectSyntax(SyntaxKind.OpenBraceToken); SkipBlock(p); continue; }
      if (p.MatchRaw(RawKind.Less)) { break; }

      if (p.MatchRaw(RawKind.Tilde))
      {
        memb.Add(ParseDtorHeaderAndSkipBody(p));
        continue;
      }

      if (p.MatchRaw(RawKind.Hat))
      {
        memb.Add(ParseCtorHeaderAndSkipBody(p));
        continue;
      }

      if (p.MatchRaw(RawKind.Identifier))
      {
        memb.Add(ParseMethodHeaderAndSkipBody(p));
        continue;
      }

      // Fallback: consume token to avoid stalling
      _ = p.ExpectRaw(p.Current.Kind);
    }

    // Skip remaining members until matching '}' for now (no expression parser yet)
    int depth = 1;
    while (!p.MatchRaw(RawKind.Eob) && depth > 0)
    {
      if (p.MatchRaw(RawKind.LBrace)) { _ = p.ExpectSyntax(SyntaxKind.OpenBraceToken); depth++; continue; }
      if (p.MatchRaw(RawKind.RBrace)) { depth--; if (depth == 0) break; _ = p.ExpectSyntax(SyntaxKind.CloseBraceToken); continue; }
      _ = p.ExpectRaw(p.Current.Kind); // consume
    }

    GreenToken cb = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    return new ServiceImpl(sref, rid, ob, fields, [.. memb], cb);
  }

  static GreenNode ParseCtorHeaderAndSkipBody(Parser p)
  {
    GreenToken hat = new(SyntaxKind.HatToken, p.ExpectRaw(RawKind.Hat));
    GreenToken op = p.ExpectSyntax(SyntaxKind.OpenParenToken);
    StructuralArray<ServiceParam> @params = ParseParamDeclList(p);
    GreenToken cp = p.ExpectSyntax(SyntaxKind.CloseParenToken);
    GreenToken bodyOpen = p.ExpectSyntax(SyntaxKind.OpenBraceToken);
    SkipBlock(p);
    return new ServiceCtorHeader(hat, op, @params, cp, bodyOpen);
  }

  static GreenNode ParseMethodHeaderAndSkipBody(Parser p)
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

  static GreenNode ParseDtorHeaderAndSkipBody(Parser p)
  {
    _ = p.ExpectRaw(RawKind.Tilde);
    GreenToken op = p.ExpectSyntax(SyntaxKind.OpenParenToken);
    GreenToken cp = p.ExpectSyntax(SyntaxKind.CloseParenToken);
    GreenToken bodyOpen = p.ExpectSyntax(SyntaxKind.OpenBraceToken);
    SkipBlock(p);
    return new ServiceDtorHeader(op, cp, bodyOpen);
  }

  static StructuralArray<ServiceParam> ParseParamDeclList(Parser p)
  {
    if (p.MatchRaw(RawKind.RParen)) return [];
    ImmutableArray<ServiceParam>.Builder list = ImmutableArray.CreateBuilder<ServiceParam>();
    while (true)
    {
      int before = p.Current.CoreToken.Offset;
      GreenToken pname = p.ExpectSyntax(SyntaxKind.IdentifierToken);
      GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
      GreenNode ptype = TypeExpr.Parse(p);
      list.Add(new ServiceParam(pname, colon, ptype));
      if (p.MatchRaw(RawKind.Comma))
      {
        _ = p.ExpectSyntax(SyntaxKind.CommaToken);
        if (p.MatchRaw(RawKind.RParen) || p.MatchRaw(RawKind.Eob)) break;
        if (p.Current.CoreToken.Offset == before) break;
        continue;
      }
      break;
    }
    return [.. list];
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

  static StructuralArray<ServiceStateField> ParseStateBlock(Parser p)
  {
    // '<' FieldDecl (',' FieldDecl)* (',')? '>' Terminator or empty '<>' Terminator
    // skip standalone syntax (terminators/comments)
    while (p.MatchRaw(RawKind.Terminator) || p.MatchRaw(RawKind.CommentTrivia))
      _ = p.ExpectRaw(p.Current.Kind);

    _ = p.ExpectRaw(RawKind.Less);
    if (p.MatchRaw(RawKind.Greater))
    {
      _ = p.ExpectRaw(RawKind.Greater);
      _ = p.ExpectSyntax(SyntaxKind.TerminatorToken);
      return [];
    }

    ImmutableArray<ServiceStateField>.Builder list = ImmutableArray.CreateBuilder<ServiceStateField>();
    while (true)
    {
      int before = p.Current.CoreToken.Offset;
      GreenToken fname = p.ExpectSyntax(SyntaxKind.IdentifierToken);
      GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
      GreenNode ftype = TypeExpr.Parse(p);
      list.Add(new ServiceStateField(fname, colon, ftype));
      if (p.MatchRaw(RawKind.Comma))
      {
        _ = p.ExpectSyntax(SyntaxKind.CommaToken);
        if (p.MatchRaw(RawKind.Greater) || p.MatchRaw(RawKind.Eob)) break;
        if (p.Current.CoreToken.Offset == before) break;
        continue;
      }
      break;
    }
    _ = p.ExpectRaw(RawKind.Greater);
    _ = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return [.. list];
  }
}

public sealed record ServiceStateField(
  GreenToken Name,
  GreenToken Colon,
  GreenNode Type)
  : GreenNode(SyntaxKind.FieldDeclaration, Name.Offset)
{
  public override int FullWidth => Type.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name; yield return Colon; yield return Type;
  }
}
