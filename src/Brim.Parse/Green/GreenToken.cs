namespace Brim.Parse.Green;

public sealed record GreenToken(SyntaxKind SyntaxKind, CoreToken CoreToken)
: GreenNode(SyntaxKind, CoreToken.Offset), IToken
{
  public StructuralArray<TriviaToken> LeadingTrivia => CoreToken.LeadingTrivia;
  public bool HasLeading => CoreToken.HasLeading;

  public TokenKind TokenKind => CoreToken.TokenKind;
  public int Length => CoreToken.Length;
  public int Line => CoreToken.Line;
  public int Column => CoreToken.Column;

  public override int FullWidth => CoreToken.Length;
  public override IEnumerable<GreenNode> GetChildren() => [];
}

