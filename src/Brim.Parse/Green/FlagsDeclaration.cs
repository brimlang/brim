namespace Brim.Parse.Green;

public sealed record FlagMemberDeclaration(
  GreenToken Identifier,
  GreenToken? TrailingComma) :
GreenNode(SyntaxKind.FlagMemberDeclaration, Identifier.Offset)
{
  public override int FullWidth => (TrailingComma?.EndOffset ?? Identifier.EndOffset) - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    if (TrailingComma is not null) yield return TrailingComma;
  }

  public static FlagMemberDeclaration Parse(Parser p)
  {
    GreenToken nameTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken? trailing = null;
    if (p.MatchRaw(RawKind.Comma)) trailing = p.ExpectSyntax(SyntaxKind.CommaToken);
    return new FlagMemberDeclaration(nameTok, trailing);
  }
}

public sealed record FlagsDeclaration(
  DeclarationName Name,
  GreenToken Colon,
  GreenToken Ampersand,
  GreenToken UnderlyingType,
  GreenToken OpenBrace,
  StructuralArray<FlagMemberDeclaration> Members,
  GreenToken CloseBrace,
  GreenToken Terminator) :
GreenNode(SyntaxKind.FlagsDeclaration, Name.Offset),
IParsable<FlagsDeclaration>
{
  public override int FullWidth => Terminator.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    yield return Ampersand;
    yield return UnderlyingType;
    yield return OpenBrace;
    foreach (FlagMemberDeclaration m in Members) yield return m;
    yield return CloseBrace;
    yield return Terminator;
  }

  // EBNF (updated): FlagsDecl ::= Identifier ':' '&' Identifier '{' Identifier (',' Identifier)* (',')? '}' Terminator
  public static FlagsDeclaration Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken amp = p.ExpectSyntax(SyntaxKind.AmpersandToken);
    GreenToken underlying = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.OpenBraceToken);

    ImmutableArray<FlagMemberDeclaration>.Builder members = ImmutableArray.CreateBuilder<FlagMemberDeclaration>();

    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        FlagMemberDeclaration member = FlagMemberDeclaration.Parse(p);
        members.Add(member);
        if (member.TrailingComma is null) break;
        if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob)) break; // trailing
        continue;
      }
    }

    StructuralArray<FlagMemberDeclaration> arr = [.. members];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new FlagsDeclaration(name, colon, amp, underlying, open, arr, close, term);
  }
}
