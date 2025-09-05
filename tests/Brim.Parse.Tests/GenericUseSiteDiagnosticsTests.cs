namespace Brim.Parse.Tests;

public class GenericUseSiteDiagnosticsTests
{
  static (Green.BrimModule module, IReadOnlyList<Diagnostic> diags) Parse(string src) => ParseFacade.ParseModule(src);

  [Fact]
  public void EmptyGenericArgumentListDiagnostic()
  {
    var (_, diags) = Parse("[[m]];\nBox[T] = %{ bad:Foo[] };");
    Assert.Contains(diags, static d => d.Code == DiagCode.EmptyGenericArgList);
  }

  // Nested generic arguments deferred; add later when full tree supported.
}
