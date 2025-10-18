using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class NamedTupleParseTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void NamedTupleBasicParses()
  {
    var m = Parse("=[m]=;\nPair[T,U] := #{T, U};");
    var td = m.Members.OfType<TypeDeclaration>().FirstOrDefault();
    Assert.NotNull(td);
    var te = Assert.IsType<TypeExpr>(td!.TypeNode);
    var nts = Assert.IsType<NamedTupleShape>(te.Core);
    Assert.Equal(2, nts.ElementList.Elements.Count);
  }

  [Fact]
  public void NamedTupleEmptyEmitsUnexpected()
  {
    var m = Parse("=[m]=;\nZeroBad := #{};");
    Assert.Contains(m.Diagnostics, d => d.Code == DiagCode.UnexpectedToken);
  }
}
