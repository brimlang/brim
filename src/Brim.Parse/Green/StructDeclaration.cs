namespace Brim.Parse.Green;

public sealed record StructDeclaration(
  DeclarationName Name,
  GreenToken Colon,
  GreenToken StructOpen,
  StructuralArray<FieldDeclaration> Fields,
  StructuralArray<GreenToken> FieldSeparators,
  GreenToken Close,
  GreenToken Terminator) :
GreenNode(SyntaxKind.StructDeclaration, Name.Offset),
IParsable<StructDeclaration>
{
  public override int FullWidth => Terminator.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Colon;
    yield return StructOpen;
    for (int i = 0; i < Fields.Count; i++)
    {
      yield return Fields[i];
      if (i < FieldSeparators.Count)
        yield return FieldSeparators[i];
    }
    yield return Close;
    yield return Terminator;
  }

  // EBNF (updated): StructDecl ::= Identifier GenericParams? ':' StructToken FieldDecl (',' FieldDecl)* (',')? '}' Terminator
  public static StructDeclaration Parse(Parser p)
  {
    DeclarationName name = DeclarationName.Parse(p);
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);
    GreenToken open = p.ExpectSyntax(SyntaxKind.StructToken);

    ImmutableArray<FieldDeclaration>.Builder fields = ImmutableArray.CreateBuilder<FieldDeclaration>();
    ImmutableArray<GreenToken>.Builder seps = ImmutableArray.CreateBuilder<GreenToken>();

    if (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      while (true)
      {
        int before = p.Current.CoreToken.Offset;
        fields.Add(FieldDeclaration.Parse(p));
        if (p.MatchRaw(RawKind.Comma))
        {
          // consume comma and decide if trailing
          GreenToken commaTok = p.ExpectSyntax(SyntaxKind.CommaToken);
          seps.Add(commaTok);
          if (p.MatchRaw(RawKind.RBrace) || p.MatchRaw(RawKind.Eob))
            break; // trailing comma
          continue; // expect another field
        }
        break;
      }
    }

    StructuralArray<FieldDeclaration> fieldArray = [.. fields];
    StructuralArray<GreenToken> sepArray = [.. seps];

    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);

    return new(
        name,
        colon,
        open,
        fieldArray,
        sepArray,
        close,
        term);
  }
}
