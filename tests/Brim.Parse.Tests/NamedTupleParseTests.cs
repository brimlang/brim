using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class NamedTupleParseTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void NamedTupleBasicParses()
  {
    var m = Parse("[[m]];\nPair[T,U] : #{T, U};");
    var nt = m.Members.OfType<NamedTupleDeclaration>().FirstOrDefault();
    Assert.NotNull(nt);
    Assert.Equal(2, nt!.Elements.Count);
  }

  [Fact]
  public void NamedTupleEmptyEmitsUnexpected()
  {
    var m = Parse("[[m]];\nZeroBad : #{};");
    Assert.Contains(m.Diagnostics, d => d.Code == DiagCode.UnexpectedToken);
  }
}
