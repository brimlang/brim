namespace Brim.Parse.Green;

public sealed record ProtocolShape(
  CommaList<MethodSignature> MethodList)
  : GreenNode(SyntaxKind.ProtocolShape, MethodList.Offset)
{
  public override int FullWidth => MethodList.FullWidth;
  public override IEnumerable<GreenNode> GetChildren() => MethodList.GetChildren();

  public static ProtocolShape Parse(Parser p)
  {
    CommaList<MethodSignature> methods = CommaList<MethodSignature>.Parse(
      p,
      SyntaxKind.ProtocolToken,
      SyntaxKind.CloseBlockToken,
      MethodSignature.Parse);

    return new ProtocolShape(methods);
  }
}
