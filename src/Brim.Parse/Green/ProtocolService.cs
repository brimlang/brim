using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record ServiceDeclaration(
  DeclarationName Name,
  GreenToken Colon,
  GreenToken Hat,
  GreenToken Receiver,
  GreenToken OpenBrace,
  GreenToken CloseBrace,
  GreenToken? ImplementsColon,
  StructuralArray<ImplementsRef> Implements,
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
    if (ImplementsColon is not null) yield return ImplementsColon;
    foreach (ImplementsRef impl in Implements) yield return impl;
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
    ImmutableArray<ImplementsRef>.Builder impls = ImmutableArray.CreateBuilder<ImplementsRef>();
    GreenToken? implsColon = null;
    if (p.MatchRaw(RawKind.Colon))
    {
      implsColon = p.ExpectSyntax(SyntaxKind.ColonToken);
      // ImplementsList: ProtocolRef ('+' ProtocolRef)*
      while (true)
      {
        int before = p.Current.CoreToken.Offset;
        // ProtocolRef: Identifier GenericArgs?
        GreenToken head = p.ExpectSyntax(SyntaxKind.IdentifierToken);
        GreenNode tref = head;
        if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
          tref = GenericType.ParseAfterName(p, head);
        GreenToken? trailingPlus = null;
        if (p.MatchRaw(RawKind.Plus)) trailingPlus = new GreenToken(SyntaxKind.PlusToken, p.ExpectRaw(RawKind.Plus));
        impls.Add(new ImplementsRef(tref, trailingPlus));
        if (trailingPlus is null) break;
        if (p.Current.CoreToken.Offset == before) break;
      }
    }
    StructuralArray<ImplementsRef> implArr = [.. impls];
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new ServiceDeclaration(name, colon, hat, recv, open, close, implsColon, implArr, term);
  }
}

public sealed record MethodSignature(
  DeclarationName Name,
  GreenToken Colon,
  GreenToken OpenParen,
  StructuralArray<FunctionParameter> Parameters,
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
    StructuralArray<FunctionParameter> plist =
      Delimited.ParseCommaSeparatedTypes(
        p,
        static p2 => TypeExpr.Parse(p2),
        static (n, c) => new FunctionParameter(n, c),
        RawKind.RParen);
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseParenToken);
    GreenNode ret = TypeExpr.Parse(p);
    GreenToken? listTrailing = null;
    if (p.MatchRaw(RawKind.Comma)) listTrailing = p.ExpectSyntax(SyntaxKind.CommaToken);
    return new MethodSignature(name, colon, open, plist, close, ret, listTrailing);
  }
}
