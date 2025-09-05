namespace Brim.Parse.Green;

public sealed record GreenToken(
  SyntaxKind SyntaxKind,
  RawToken Token)
: GreenNode(SyntaxKind, Token.Offset)
{
  public override int FullWidth => Token.Length;
  public override IEnumerable<GreenNode> GetChildren() => [];
}

