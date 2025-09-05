namespace Brim.Parse.Tests;

public class GenericDiagnosticsTests
{
  static (Brim.Parse.Green.BrimModule module, IReadOnlyList<Diagnostic> diags) Parse(string src) => ParseFacade.ParseModule(src);

  [Fact]
  public void EmptyGenericParamListDiagnostic()
  {
    var (_, diags) = Parse("[[m]];\nFoo[] = %{ x:Foo };");
    Assert.Contains(diags, static d => d.Code == DiagCode.EmptyGenericParamList);
  }

  [Fact]
  public void UnexpectedGenericBodyDiagnostic()
  {
    // Use a body opener not yet supported after generic head, e.g. '&{' (Ampersand then '{' maybe not combined; falls back to unexpected)
    var (_, diags2) = Parse("[[m]];\nFoo[T] = @{}\n");
    Assert.Contains(diags2, static d => d.Code == DiagCode.UnexpectedGenericBody);
  }
}
