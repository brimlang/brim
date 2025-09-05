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

  public static ModuleHeader Parse(ref Parser p) => new(
    p.ExpectSyntax(SyntaxKind.ModulePathOpenToken),
    ModulePath.Parse(ref p),
    p.ExpectSyntax(SyntaxKind.ModulePathCloseToken)
  );
}
