using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record GreenToken(
  SyntaxKind SyntaxKind,
  SignificantToken Significant)
: GreenNode(SyntaxKind, Significant.CoreToken.Offset)
{
  public RawToken Token => Significant.CoreToken;
  public StructuralArray<RawToken> LeadingTrivia => Significant.LeadingTrivia;

  public bool HasLeading => LeadingTrivia.Count > 0;

  // Keep width semantics to core token text (exclude trivia).
  public override int FullWidth => Token.Length;
  public override IEnumerable<GreenNode> GetChildren() => [];

  // Convenience for constructing from a raw token when no trivia available (e.g. fabricated/missing)
  public GreenToken(SyntaxKind kind, RawToken raw)
    : this(kind, new SignificantToken(raw, [])) { }
}

