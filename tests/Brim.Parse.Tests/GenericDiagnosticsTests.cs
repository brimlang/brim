namespace Brim.Parse.Tests;

public class GenericDiagnosticsTests
{
  static Brim.Parse.Green.BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact(Skip = "TODO: Broken")]
  public void EmptyGenericParamListDiagnostic()
  {
    var m = Parse("[[m]];\nFoo[] = %{ x:Foo };");
    Assert.Contains(m.Diagnostics, static d => d.Code == DiagCode.EmptyGenericParamList);
  }

  [Fact]
  public void UnexpectedGenericBodyDiagnostic()
  {
    // Use a body opener not yet supported after generic head, e.g. '&{' (Ampersand then '{' maybe not combined; falls back to unexpected)
    var m2 = Parse("[[m]];\nFoo[T] = @{}\n");
    Assert.Contains(m2.Diagnostics, static d => d.Code == DiagCode.UnexpectedGenericBody);
  }
}
