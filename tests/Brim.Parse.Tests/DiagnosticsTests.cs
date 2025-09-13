namespace Brim.Parse.Tests;

public class DiagnosticsTests
{
  static Green.BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void UnexpectedTokenEmitsDiagnostic()
  {
    // Number literal does not start any production (predictions: <<, [[, identifier)
    var m = Parse("123");
    Assert.Contains(m.Diagnostics, static d => d.Code == DiagCode.UnexpectedToken);
  }

  [Fact]
  public void MissingTokenEmitsDiagnostic()
  {
    // An import like: [[path]] without terminator maybe? Use struct decl missing close brace.
    var m2 = Parse("foo : %{\n");
    Assert.Contains(m2.Diagnostics, static d => d.Code == DiagCode.MissingToken);
  }

  [Fact]
  public void InvalidCharacterEmitsDiagnostic()
  {
    var (_, diags3) = Parse("$");
    var m3 = Parse("$");
    Assert.Contains(m3.Diagnostics, static d => d.Code == DiagCode.InvalidCharacter);
  }

  [Fact]
  public void UnterminatedStringEmitsDiagnostic()
  {
    var (_, diags4) = Parse("\"hello");
    var m4 = Parse("\"hello");
    Assert.Contains(m4.Diagnostics, static d => d.Code == DiagCode.UnterminatedString);
  }
}
