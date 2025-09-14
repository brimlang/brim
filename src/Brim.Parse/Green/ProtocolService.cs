namespace Brim.Parse.Green;

public sealed record ProtocolDeclaration(
  DeclarationName Name,
  GreenToken Colon,
  GreenToken Dot,
  GreenToken OpenBrace,
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
    yield return CloseBrace;
    yield return Terminator;
  }

  // Minimal header-only parse: Name GP ':.' '{' '}' Terminator
  public static ProtocolDeclaration Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    // '.' is a RawKind.Stop; map to StopToken for display
    GreenToken dot = new(SyntaxKind.StopToken, p.ExpectRaw(RawKind.Stop));
    GreenToken open = p.ExpectSyntax(SyntaxKind.OpenBraceToken);
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new ProtocolDeclaration(name, colon, dot, open, close, term);
  }
}

public sealed record ServiceDeclaration(
  DeclarationName Name,
  GreenToken Colon,
  GreenToken Hat,
  GreenToken Receiver,
  GreenToken OpenBrace,
  GreenToken CloseBrace,
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
    yield return Terminator;
  }

  // Minimal header-only parse: Name GP ':^' Ident '{' '}' Terminator
  public static ServiceDeclaration Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken hat = new(SyntaxKind.HatToken, p.ExpectRaw(RawKind.Hat));
    GreenToken recv = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.OpenBraceToken);
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new ServiceDeclaration(name, colon, hat, recv, open, close, term);
  }
}

