namespace Brim.Parse.Green;

/// <summary>
/// A single entry inside a union constructor term body.
/// Shape: Variant or Variant = PayloadExpr
/// </summary>
public sealed record UnionConstructorEntry(
  GreenToken Variant,
  GreenToken? Equal,
  GreenNode? Payload)
  : GreenNode(SyntaxKind.UnionConstructorEntry, Variant.Offset)
{
  public override int FullWidth => (Payload?.EndOffset ?? Equal?.EndOffset ?? Variant.EndOffset) - Variant.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Variant;
    if (Equal is not null) yield return Equal;
    if (Payload is not null) yield return Payload;
  }
}

/// <summary>
/// Union constructor term expression.
/// Shape: TypeHead "|{" (Variant | Variant '=' PayloadExpr) "}"
/// Exactly one entry is allowed in the body.
/// </summary>
public sealed record UnionConstructorExpr(
  GreenNode TypeHead,
  GreenToken OpenToken,
  UnionConstructorEntry Entry,
  GreenToken CloseToken)
  : GreenNode(SyntaxKind.UnionConstructorExpr, TypeHead.Offset)
{
  public override int FullWidth => CloseToken.EndOffset - TypeHead.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return TypeHead;
    yield return OpenToken;
    yield return Entry;
    yield return CloseToken;
  }

  /// <summary>
  /// Parses a union constructor term at the current position.
  /// This is foundational scaffolding and only parses minimal payload expressions (identifiers and literals) for now.
  /// </summary>
  public static UnionConstructorExpr ParseTerm(Parser p)
  {
    // Parse the type head: Identifier with optional generic arguments.
    GreenToken typeNameTok = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenNode typeHead = typeNameTok;
    if (p.MatchRaw(RawKind.LBracket) && !p.MatchRaw(RawKind.LBracketLBracket))
    {
      typeHead = GenericType.ParseAfterName(p, typeNameTok);
    }

    // Expect the union constructor open token: |{
    GreenToken open = p.ExpectSyntax(SyntaxKind.UnionToken);

    // Parse exactly one entry: Variant or Variant = Payload
    GreenToken variant = p.ExpectSyntax(SyntaxKind.IdentifierToken);
    GreenToken? equal = null;
    GreenNode? payload = null;

    if (p.MatchRaw(RawKind.Equal))
    {
      equal = p.ExpectSyntax(SyntaxKind.EqualToken);

      // Minimal payload expression coverage: integer, decimal, string, or identifier.
      if (p.MatchRaw(RawKind.IntegerLiteral))
      {
        payload = IntegerLiteral.Parse(p);
      }
      else if (p.MatchRaw(RawKind.DecimalLiteral))
      {
        payload = DecimalLiteral.Parse(p);
      }
      else if (p.MatchRaw(RawKind.StringLiteral))
      {
        payload = p.ExpectSyntax(SyntaxKind.StrToken);
      }
      else if (p.MatchRaw(RawKind.Identifier))
      {
        payload = p.ExpectSyntax(SyntaxKind.IdentifierToken);
      }
      else
      {
        // Fabricate a missing payload by expecting an identifier (produces a diagnostic)
        payload = p.ExpectSyntax(SyntaxKind.IdentifierToken);
      }
    }

    // Close brace for the single-entry body.
    GreenToken close = p.ExpectSyntax(SyntaxKind.CloseBraceToken);

    return new(typeHead, open, new UnionConstructorEntry(variant, equal, payload), close);
  }
}

