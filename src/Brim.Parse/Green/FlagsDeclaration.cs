namespace Brim.Parse.Green;

public sealed record FlagMemberDeclaration(
  Identifier Identifier) :
GreenNode(SyntaxKind.FlagMemberDeclaration, Identifier.Offset)
{
  public override int FullWidth => Identifier.FullWidth;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
  }

  public static FlagMemberDeclaration Parse(Parser p)
  {
    Identifier name = Identifier.Parse(p);
    return new FlagMemberDeclaration(name);
  }
}

public sealed record FlagsDeclaration(
  Identifier Identifier,
  GreenToken Colon,
  GreenToken Ampersand,
  Identifier UnderlyingType,
  GreenToken OpenBrace,
  StructuralArray<FlagMemberDeclaration> Members,
  GreenToken CloseBrace,
  GreenToken Terminator) :
GreenNode(SyntaxKind.FlagsDeclaration, Identifier.Offset),
IParsable<FlagsDeclaration>
{
  public override int FullWidth => Terminator.EndOffset - Identifier.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
    yield return Colon;
    yield return Ampersand;
    yield return UnderlyingType;
    yield return OpenBrace;
    foreach (FlagMemberDeclaration m in Members) yield return m;
    yield return CloseBrace;
    yield return Terminator;
  }

  // EBNF: FlagsDecl ::= Identifier ':' '&' Identifier '{' Identifier (',' Identifier)* '}' Terminator
  public static FlagsDeclaration Parse(Parser p)
  {
    Identifier id = Identifier.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken amp = p.ExpectSyntax(SyntaxKind.AmpersandToken);
    Identifier underlying = Identifier.Parse(p);
    GreenToken open = p.ExpectSyntax(SyntaxKind.OpenBraceToken);

    ImmutableArray<FlagMemberDeclaration>.Builder members = ImmutableArray.CreateBuilder<FlagMemberDeclaration>();
    while (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      int before = p.Current.CoreToken.Offset;
      members.Add(FlagMemberDeclaration.Parse(p));
      if (p.MatchRaw(RawKind.Comma)) _ = p.ExpectRaw(RawKind.Comma);
      if (p.Current.CoreToken.Offset == before)
      {
        break; // no progress
      }
    }

    StructuralArray<FlagMemberDeclaration> arr = [.. members];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new FlagsDeclaration(id, colon, amp, underlying, open, arr, close, term);
  }
}
