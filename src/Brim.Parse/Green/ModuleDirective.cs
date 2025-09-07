namespace Brim.Parse.Green;

public sealed record ModuleDirective(
    ModuleHeader ModuleHeader,
    GreenToken Terminator) :
GreenNode(SyntaxKind.ModuleDirective, ModuleHeader.Offset),
IParsable<ModuleDirective>
{
  public override int FullWidth => Terminator.EndOffset - ModuleHeader.Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return ModuleHeader;
    yield return Terminator;
  }

  public static ModuleDirective Parse(Parser p) => new(
    ModuleHeader.Parse(p),
    p.ExpectSyntax(SyntaxKind.TerminatorToken)
  );
}

