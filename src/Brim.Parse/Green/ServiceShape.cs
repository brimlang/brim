namespace Brim.Parse.Green;

public sealed record ServiceShape(
  CommaList<ProtocolRef> ProtocolList
) : GreenNode(SyntaxKind.ServiceShape, ProtocolList.Offset)
{
  public override int FullWidth => ProtocolList.FullWidth;

  public override IEnumerable<GreenNode> GetChildren() => ProtocolList.GetChildren();

  public static ServiceShape Parse(Parser p)
  {
    return new ServiceShape(CommaList<ProtocolRef>.Parse(
      p,
      SyntaxKind.ServiceToken,
      SyntaxKind.CloseBlockToken,
      ProtocolRef.Parse));
  }
}
