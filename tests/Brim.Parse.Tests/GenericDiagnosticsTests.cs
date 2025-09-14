namespace Brim.Parse.Tests;

public class GenericDiagnosticsTests
{
  static Green.BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void EmptyGenericParamListDiagnostic()
  {
    var m = Parse("[[m]];\nFoo[] : %{ x:Foo };");
    Assert.Contains(m.Diagnostics, static d => d.Code == DiagCode.EmptyGenericParamList);
  }

  [Fact]
  public void UnexpectedGenericBodyDiagnostic()
  {
    // '&{' invalid body start after generic head (expect & Ident '{')
    var m2 = Parse("[[m]];\nFoo[T] := &{}\n");
    Assert.Contains(m2.Diagnostics, static d => d.Code == DiagCode.UnexpectedGenericBody);
  }
}
