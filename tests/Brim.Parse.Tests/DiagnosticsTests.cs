namespace Brim.Parse.Tests;

public class DiagnosticsTests
{
  static (Green.BrimModule module, IReadOnlyList<Diagnostic> diags) Parse(string src) => ParseFacade.ParseModule(src);

  [Fact]
  public void UnexpectedTokenEmitsDiagnostic()
  {
    // Number literal does not start any production (predictions: <<, [[, identifier)
    var (_, diags) = Parse("123");
    Assert.Contains(diags, static d => d.Code == DiagCode.UnexpectedToken);
  }

  [Fact]
  public void MissingTokenEmitsDiagnostic()
  {
    // An import like: [[path]] without terminator maybe? Use struct decl missing close brace.
    var (_, diags2) = Parse("foo = %{\n");
    Assert.Contains(diags2, static d => d.Code == DiagCode.MissingToken);
  }

  [Fact]
  public void InvalidCharacterEmitsDiagnostic()
  {
    var (_, diags3) = Parse("$");
    Assert.Contains(diags3, static d => d.Code == DiagCode.InvalidCharacter);
  }

  [Fact]
  public void UnterminatedStringEmitsDiagnostic()
  {
    var (_, diags4) = Parse("\"hello");
    Assert.Contains(diags4, static d => d.Code == DiagCode.UnterminatedString);
  }
}
