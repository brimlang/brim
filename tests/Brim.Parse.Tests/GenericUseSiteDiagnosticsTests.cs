namespace Brim.Parse.Tests;

public class GenericUseSiteDiagnosticsTests
{
  static Green.BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void EmptyGenericArgumentListDiagnostic()
  {
    var m = Parse("[[m]];\nBox[T] : %{ bad:Foo[] };");
    Assert.Contains(m.Diagnostics, static d => d.Code == DiagCode.EmptyGenericArgList);
  }

  // Nested generic arguments deferred; add later when full tree supported.
}
