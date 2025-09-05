namespace Brim.Parse.Green;

public sealed record GenericHead(
  Identifier Name,
  GenericParameterList Parameters,
  GreenToken Equal)
: GreenNode(SyntaxKind.GenericParameterList, Name.Offset) // reuse kind for now
{
  public override int FullWidth => Equal.EndOffset - Name.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Name;
    yield return Parameters;
    yield return Equal;
  }
}

public static class GenericDeclaration
{
  public static GreenNode Parse(Parser p)
  {
    Identifier name = Identifier.Parse(p);
    GreenToken open = p.ExpectSyntax(SyntaxKind.GenericOpenToken);
    ImmutableArray<Identifier>.Builder parms = ImmutableArray.CreateBuilder<Identifier>();
    bool first = true;
    bool empty = false;
    if (p.Match(RawTokenKind.RBracket))
    {
      empty = true; // record emptiness; still consume later via expect close
    }
    else
    {
      while (!p.Match(RawTokenKind.RBracket) && !p.Match(RawTokenKind.Eob))
      {
        // Progress guard: Identifier.Parse may fabricate a missing identifier without consuming
        // any real token (ExpectSyntax returns a fabricated token on mismatch). If the current
        // token does not advance we would otherwise spin forever. Capture offset before parse
        // and break if unchanged so closing bracket expectation / error recovery can proceed.
        int before = p.Current.CoreToken.Offset;
        if (!first)
        {
          if (p.Match(RawTokenKind.Comma))
            _ = p.Expect(RawTokenKind.Comma);
          else break;
        }

        parms.Add(Identifier.Parse(p));
        first = false;
        if (p.Current.CoreToken.Offset == before)
        {
          // No forward progress; break to avoid infinite loop.
          break;
        }
      }
    }
    GreenToken close = p.ExpectSyntax(SyntaxKind.GenericCloseToken);
    if (empty)
    {
      p.AddDiagEmptyGeneric(open);
    }

    GenericParameterList gpl = new(open, parms.ToImmutable(), close);
    GreenToken eq = p.ExpectSyntax(SyntaxKind.EqualToken);

    if (p.Match(RawTokenKind.PercentLBrace))
    {
      return ParseGenericStructBody(p, name, gpl, eq);
    }
    if (p.Match(RawTokenKind.PipeLBrace))
    {
      return ParseGenericUnionBody(p, name, gpl, eq);
    }

    p.AddDiagUnexpectedGenericBody();
    return new GreenToken(SyntaxKind.ErrorToken, p.Current);
  }

  static StructDeclaration ParseGenericStructBody(Parser p, Identifier name, GenericParameterList gpl, GreenToken eq)
  {
    GreenToken openStruct = p.ExpectSyntax(SyntaxKind.StructToken);
    ImmutableArray<FieldDeclaration>.Builder fields = ImmutableArray.CreateBuilder<FieldDeclaration>();
    while (!p.Match(RawTokenKind.RBrace) && !p.Match(RawTokenKind.Eob))
    {
      int before = p.Current.CoreToken.Offset;
      fields.Add(FieldDeclaration.Parse(p));
      if (p.Match(RawTokenKind.Comma)) _ = p.Expect(RawTokenKind.Comma);
      if (p.Current.CoreToken.Offset == before)
      {
        // Field parse made no progress; break to avoid infinite loop (recover via close brace expectation).
        break;
      }
    }

    StructuralArray<FieldDeclaration> arr = [.. fields];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);

    return new StructDeclaration(name, gpl, eq, openStruct, arr, close, term);
  }

  static UnionDeclaration ParseGenericUnionBody(Parser p, Identifier name, GenericParameterList gpl, GreenToken eq)
  {
    GreenToken openUnion = p.ExpectSyntax(SyntaxKind.UnionToken); // PipeLBrace
    ImmutableArray<UnionVariantDeclaration>.Builder vars = ImmutableArray.CreateBuilder<UnionVariantDeclaration>();
    while (!p.Match(RawTokenKind.RBrace) && !p.Match(RawTokenKind.Eob))
    {
      int before = p.Current.CoreToken.Offset;
      vars.Add(UnionVariantDeclaration.Parse(p));
      if (p.Match(RawTokenKind.Comma)) _ = p.Expect(RawTokenKind.Comma);
      if (p.Current.CoreToken.Offset == before)
      {
        // Variant parse made no progress; break to avoid infinite loop.
        break;
      }
    }
    StructuralArray<UnionVariantDeclaration> arr = [.. vars];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new UnionDeclaration(name, gpl, eq, openUnion, arr, close, term);
  }
}
