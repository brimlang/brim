using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class TreeShapeRegressionTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void IdentifierWrapperNodeRemoved_FromDeclarationsAndGenerics()
  {
    string src = "=[m]=;\nFoo[T]: %{\n  field: Bar[Baz]\n};\nBar[X,Y]: |{\n  A: Qux,\n  B: Qux[Z]\n};\nFlags[A]: & prim {\n  ONE, TWO\n};\nTuple[K]: #{ Alpha, Beta[Gamma] };\n";

    var module = Parse(src);

    var legacy = module.Enumerate().Where(n => n is not GreenToken && n.Kind == SyntaxKind.IdentifierToken).ToList();
    Assert.Empty(legacy);
  }
}

internal static class GreenNodeExtensions
{
  public static IEnumerable<GreenNode> Enumerate(this GreenNode node)
  {
    yield return node;
    foreach (GreenNode child in node.GetChildren())
    {
      foreach (GreenNode d in child.Enumerate()) yield return d;
    }
  }
}
