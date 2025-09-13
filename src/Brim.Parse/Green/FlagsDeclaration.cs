namespace Brim.Parse.Green;

public sealed record FlagMemberDeclaration(
  GreenToken Identifier) :
GreenNode(SyntaxKind.FlagMemberDeclaration, Identifier.Offset)
{
  public override int FullWidth => Identifier.FullWidth;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Identifier;
  }

  public static FlagMemberDeclaration Parse(Parser p)
  {
    GreenToken nameTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    return new FlagMemberDeclaration(nameTok);
  }
}

public sealed record FlagsDeclaration(
  DeclarationName Name,
  GreenToken Colon,
  GreenToken Ampersand,
  GreenToken UnderlyingType,
  GreenToken OpenBrace,
  StructuralArray<FlagMemberDeclaration> Members,
  StructuralArray<GreenToken> MemberSeparators,
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
    for (int i = 0; i < Members.Count; i++)
    {
      yield return Members[i];
      if (i < MemberSeparators.Count)
        yield return MemberSeparators[i];
    }
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
    ImmutableArray<GreenToken>.Builder seps = ImmutableArray.CreateBuilder<GreenToken>();

    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        int before = p.Current.CoreToken.Offset;
        members.Add(FlagMemberDeclaration.Parse(p));
        if (p.MatchRaw(RawKind.Comma))
        {
          GreenToken commaTok = p.ExpectSyntax(SyntaxKind.CommaToken);
          seps.Add(commaTok);
          if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob)) break; // trailing comma
          continue;
        }
        break;
      }
    }

    StructuralArray<FlagMemberDeclaration> arr = [.. members];
    StructuralArray<GreenToken> sepArr = [.. seps];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new FlagsDeclaration(name, colon, amp, underlying, open, arr, sepArr, close, term);
  }
}
