namespace Brim.Parse.Green;

public sealed record FlagsShape(
  CommaList<FlagMemberDeclaration> MemberList) :
GreenNode(SyntaxKind.FlagsShape, MemberList.Offset),
IParsable<FlagsShape>
{
  public override int FullWidth => MemberList.FullWidth;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return MemberList;
  }

  public static FlagsShape Parse(Parser p)
  {
    return new(CommaList<FlagMemberDeclaration>.Parse(
        p,
        SyntaxKind.FlagsToken,
        SyntaxKind.CloseBlockToken,
        FlagMemberDeclaration.Parse));
  }
}
