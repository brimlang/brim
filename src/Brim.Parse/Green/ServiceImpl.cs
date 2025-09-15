namespace Brim.Parse.Green;

public sealed record ServiceImpl(
  GreenNode ServiceRef,
  GreenToken ReceiverIdent,
  GreenToken OpenBrace,
  StructuralArray<ServiceStateField> StateFields,
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
    yield return CloseBrace;
  }

  public static ServiceImpl Parse(Parser p)
  {
    // Name [TypeArgs]? '<' ident '>' '{' StateBlock '}'
    GreenToken head = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenNode sref = head;
    if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
      sref = GenericType.ParseAfterName(p, head);

    _ = p.ExpectRaw(RawKind.Less);
    GreenToken rid = p.ExpectSyntax(SyntaxKind.IdentifierToken); // '_' allowed as identifier
    _ = p.ExpectRaw(RawKind.Greater);
    GreenToken ob = p.ExpectSyntax(SyntaxKind.OpenBraceToken);

    StructuralArray<ServiceStateField> fields = ParseStateBlock(p);

    // Skip remaining members until matching '}' for now (no expression parser yet)
    int depth = 1;
    while (!p.MatchRaw(RawKind.Eob) && depth > 0)
    {
      if (p.MatchRaw(RawKind.LBrace)) { _ = p.ExpectSyntax(SyntaxKind.OpenBraceToken); depth++; continue; }
      if (p.MatchRaw(RawKind.RBrace)) { depth--; if (depth == 0) break; _ = p.ExpectSyntax(SyntaxKind.CloseBraceToken); continue; }
      _ = p.ExpectRaw(p.Current.Kind); // consume
    }

    GreenToken cb = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    return new ServiceImpl(sref, rid, ob, fields, cb);
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
