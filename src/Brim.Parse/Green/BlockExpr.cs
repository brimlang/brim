using Brim.Parse.Collections;

namespace Brim.Parse.Green;

public sealed record BlockExpr(
  GreenToken OpenBrace,
  StructuralArray<GreenNode> Statements,
  GreenToken CloseBrace) :
GreenNode(SyntaxKind.BlockExpr, OpenBrace.Offset)
{
  public override int FullWidth => CloseBrace.EndOffset - Offset;
  public override IEnumerable<GreenNode> GetChildren()
  {
    yield return OpenBrace;
    foreach (GreenNode stmt in Statements) yield return stmt;
    yield return CloseBrace;
  }
}
