namespace Brim.Parse.Green;


public static class GenericDeclaration
{
  public static GreenNode Parse(Parser p)
  {
    Identifier name = Identifier.Parse(p);
    GreenToken open = p.ExpectSyntax(SyntaxKind.GenericOpenToken);
    ImmutableArray<GenericParameter>.Builder parms = ImmutableArray.CreateBuilder<GenericParameter>();
    bool first = true;
    bool empty = false;
    if (p.MatchRaw(RawKind.RBracket))
    {
      empty = true; // record emptiness; still consume later via expect close
    }
    else
    {
      while (!p.MatchRaw(RawKind.RBracket) && !p.MatchRaw(RawKind.Eob))
      {
        // Progress guard: Identifier.Parse may fabricate a missing identifier without consuming
        // any real token (ExpectSyntax returns a fabricated token on mismatch). If the current
        // token does not advance we would otherwise spin forever. Capture offset before parse
        // and break if unchanged so closing bracket expectation / error recovery can proceed.
        int before = p.Current.CoreToken.Offset;
        if (!first)
        {
          if (p.MatchRaw(RawKind.Comma))
            _ = p.ExpectRaw(RawKind.Comma);
          else break;
        }

        Identifier paramName = Identifier.Parse(p);
        ConstraintList? constraints = null;
        if (p.MatchRaw(RawKind.Colon))
        {
          GreenToken colonTok = p.ExpectSyntax(SyntaxKind.ColonToken);
          // Parse one or more ProtoRefs separated by '+'; for now ProtoRef ::= Identifier GenericArgList?
          ImmutableArray<GreenNode>.Builder refs = ImmutableArray.CreateBuilder<GreenNode>();
          bool firstRef = true;
          while (!p.MatchRaw(RawKind.RBracket) && !p.MatchRaw(RawKind.Eob))
          {
            int beforeRef = p.Current.CoreToken.Offset;
            if (!firstRef)
            {
              if (p.MatchRaw(RawKind.Plus)) _ = p.ExpectRaw(RawKind.Plus); else break;
            }
            Identifier typeName = Identifier.Parse(p);
            GreenNode refNode = typeName;
            if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
            {
              refNode = GenericType.ParseAfterName(p, typeName);
            }
            refs.Add(refNode);
            firstRef = false;
            if (p.Current.CoreToken.Offset == beforeRef) break;
          }
          StructuralArray<GreenNode> arr = [.. refs];
          if (arr.Count == 0)
          {
            // No constraint after ':' => diagnostic best-effort.
            p.AddDiagInvalidGenericConstraint();
          }
          constraints = new ConstraintList(colonTok, arr);
        }
        parms.Add(new GenericParameter(paramName, constraints));
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
    GreenToken colon = p.ExpectSyntax(SyntaxKind.ColonToken);

    if (p.MatchRaw(RawKind.PercentLBrace))
    {
      return ParseGenericStructBody(p, name, gpl, colon);
    }
    if (p.MatchRaw(RawKind.PipeLBrace))
    {
      return ParseGenericUnionBody(p, name, gpl, colon);
    }
    if (p.MatchRaw(RawKind.HashLBrace))
    {
      return ParseGenericNamedTupleBody(p, name, gpl, colon);
    }

    p.AddDiagUnexpectedGenericBody();
    return new GreenToken(SyntaxKind.ErrorToken, p.Current);
  }

  static StructDeclaration ParseGenericStructBody(Parser p, Identifier name, GenericParameterList gpl, GreenToken colon)
  {
    GreenToken openStruct = p.ExpectSyntax(SyntaxKind.StructToken);
    ImmutableArray<FieldDeclaration>.Builder fields = ImmutableArray.CreateBuilder<FieldDeclaration>();
    while (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      int before = p.Current.CoreToken.Offset;
      fields.Add(FieldDeclaration.Parse(p));
      if (p.MatchRaw(RawKind.Comma)) _ = p.ExpectRaw(RawKind.Comma);
      if (p.Current.CoreToken.Offset == before)
      {
        // Field parse made no progress; break to avoid infinite loop (recover via close brace expectation).
        break;
      }
    }

    StructuralArray<FieldDeclaration> arr = [.. fields];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);

    return new StructDeclaration(name, gpl, colon, openStruct, arr, close, term);
  }

  static UnionDeclaration ParseGenericUnionBody(Parser p, Identifier name, GenericParameterList gpl, GreenToken colon)
  {
    GreenToken openUnion = p.ExpectSyntax(SyntaxKind.UnionToken); // PipeLBrace
    ImmutableArray<UnionVariantDeclaration>.Builder vars = ImmutableArray.CreateBuilder<UnionVariantDeclaration>();
    while (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      int before = p.Current.CoreToken.Offset;
      vars.Add(UnionVariantDeclaration.Parse(p));
      if (p.MatchRaw(RawKind.Comma)) _ = p.ExpectRaw(RawKind.Comma);
      if (p.Current.CoreToken.Offset == before)
      {
        // Variant parse made no progress; break to avoid infinite loop.
        break;
      }
    }

    StructuralArray<UnionVariantDeclaration> arr = [.. vars];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new UnionDeclaration(name, gpl, colon, openUnion, arr, close, term);
  }

  static NamedTupleDeclaration ParseGenericNamedTupleBody(Parser p, Identifier name, GenericParameterList gpl, GreenToken colon)
  {
    GreenToken open = p.ExpectSyntax(SyntaxKind.NamedTupleToken);
    ImmutableArray<GreenNode>.Builder elems = ImmutableArray.CreateBuilder<GreenNode>();
    bool first = true;
    while (!p.MatchRaw(RawKind.RBrace) && !p.MatchRaw(RawKind.Eob))
    {
      int before = p.Current.CoreToken.Offset;
      if (!first)
      {
        if (p.MatchRaw(RawKind.Comma)) _ = p.ExpectRaw(RawKind.Comma); else break;
      }
      Identifier typeName = Identifier.Parse(p);
      GreenNode ty = typeName;
      if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
      {
        ty = GenericType.ParseAfterName(p, typeName);
      }
      elems.Add(ty);
      first = false;
      if (p.Current.CoreToken.Offset == before) break;
    }
    if (elems.Count == 0)
    {
      p.AddDiagEmptyNamedTupleElementList();
    }
    StructuralArray<GreenNode> arr = [.. elems];
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);
    GreenToken term = p.ExpectSyntax(SyntaxKind.TerminatorToken);
    return new NamedTupleDeclaration(name, gpl, colon, open, arr, close, term);
  }
}
