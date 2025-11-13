namespace Brim.Parse.Green;

public sealed record ModuleHeader(
  GreenToken Open,
  ModulePath ModulePath,
  GreenToken Close)
: GreenNode(SyntaxKind.ModuleHeader, Open.Offset)
, IParsable<ModuleHeader>
{
  public override int FullWidth => Close.EndOffset - Open.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return Open;
    yield return ModulePath;
    yield return Close;
  }

  public static ModuleHeader Parse(Parser p) => new(
    p.Expect(SyntaxKind.ModulePathOpenToken),
  ModulePath.Parse(p),
    p.Expect(SyntaxKind.ModulePathCloseToken)
  );
}
