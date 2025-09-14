namespace Brim.Parse.Green;

public sealed record ProtocolDeclaration(
  DeclarationName Name,
  GreenToken Colon,
  GreenToken Dot,
  GreenToken OpenBrace,
  StructuralArray<MethodSignature> Methods,
  GreenToken CloseBrace,
  GreenToken Terminator)
  : GreenNode(SyntaxKind.ProtocolDeclaration, Name.Offset)
{
  public override int FullWidth => Terminator.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    yield return Dot;
    yield return OpenBrace;
    foreach (MethodSignature m in Methods) yield return m;
    yield return CloseBrace;
    yield return Terminator;
  }

  // Header with method signatures only: Name GP ':.' '{' MethodSigList? '}' Terminator
  public static ProtocolDeclaration Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    // '.' is a RawKind.Stop; map to StopToken for display
    GreenToken dot = new(SyntaxKind.StopToken, p.ExpectRaw(RawKind.Stop));
    GreenToken open = p.ExpectSyntax(SyntaxKind.OpenBraceToken);

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
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new ProtocolDeclaration(name, colon, dot, open, list, close, term);
  }
}

public sealed record ServiceDeclaration(
  DeclarationName Name,
  GreenToken Colon,
  GreenToken Hat,
  GreenToken Receiver,
  GreenToken OpenBrace,
  GreenToken CloseBrace,
  StructuralArray<GreenNode> Implements,
  GreenToken Terminator)
  : GreenNode(SyntaxKind.ServiceDeclaration, Name.Offset)
{
  public override int FullWidth => Terminator.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    yield return Hat;
    yield return Receiver;
    yield return OpenBrace;
    yield return CloseBrace;
    foreach (GreenNode impl in Implements) yield return impl;
    yield return Terminator;
  }

  // Minimal header-only parse: Name GP ':^' Ident '{' '}' (':' ImplementsList)? Terminator
  public static ServiceDeclaration Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken hat = new(SyntaxKind.HatToken, p.ExpectRaw(RawKind.Hat));
    GreenToken recv = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.OpenBraceToken);
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    ImmutableArray<GreenNode>.Builder impls = ImmutableArray.CreateBuilder<GreenNode>();
    if (p.MatchRaw(RawKind.Colon))
    {
      _ = p.ExpectSyntax(SyntaxKind.ColonToken);
      // ImplementsList: ProtocolRef ('+' ProtocolRef)*
      while (true)
      {
        int before = p.Current.CoreToken.Offset;
        // ProtocolRef: Identifier GenericArgs?
        GreenToken head = p.ExpectSyntax(SyntaxKind.IdentifierToken);
        GreenNode tref = head;
        if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
          tref = GenericType.ParseAfterName(p, head);
        impls.Add(tref);
        if (p.MatchRaw(RawKind.Plus))
        {
          _ = p.ExpectRaw(RawKind.Plus);
          continue;
        }
        if (p.Current.CoreToken.Offset == before) break;
        break;
      }
    }
    StructuralArray<GreenNode> implArr = [.. impls];
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new ServiceDeclaration(name, colon, hat, recv, open, close, implArr, term);
  }
}

public sealed record MethodSignature(
  DeclarationName Name,
  GreenToken Colon,
  GreenToken OpenParen,
  StructuralArray<GreenNode> Parameters,
  GreenToken CloseParen,
  GreenNode ReturnType,
  GreenToken? TrailingComma)
  : GreenNode(SyntaxKind.MethodSignature, Name.Offset)
{
  public override int FullWidth => (TrailingComma?.EndOffset ?? ReturnType.EndOffset) - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    yield return OpenParen;
    foreach (GreenNode p in Parameters) yield return p;
    yield return CloseParen;
    yield return ReturnType;
    if (TrailingComma is not null) yield return TrailingComma;
  }

  public static MethodSignature Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.OpenParenToken);
    ImmutableArray<GreenNode>.Builder @params = ImmutableArray.CreateBuilder<GreenNode>();
    bool empty = p.MatchRaw(RawKind.RParen);
    if (!empty)
    {
      while (true)
      {
        int before = p.Current.CoreToken.Offset;
        GreenNode ty = TypeExpr.Parse(p);
        @params.Add(ty);
        if (p.MatchRaw(RawKind.Comma))
        {
          _ = p.ExpectSyntax(SyntaxKind.CommaToken);
          if (p.MatchRaw(RawKind.RParen) || p.MatchRaw(RawKind.Eob))
          {
            break;
          }
        }
        else
        {
          break;
        }
        if (p.Current.CoreToken.Offset == before) break;
      }
    }
    StructuralArray<GreenNode> plist = [.. @params];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseParenToken);
    GreenNode ret = TypeExpr.Parse(p);
    GreenToken? trailing = null;
    if (p.MatchRaw(RawKind.Comma)) trailing = p.ExpectSyntax(SyntaxKind.CommaToken);
    return new MethodSignature(name, colon, open, plist, close, ret, trailing);
  }
}
