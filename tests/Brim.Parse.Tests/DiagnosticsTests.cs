using Brim.Parse;
using Xunit;

namespace Brim.Parse.Tests;

public class DiagnosticsTests
{
  static (Parser parser, Brim.Parse.Green.BrimModule module) Parse(string src)
  {
    var p = new Parser(SourceText.From(src));
    var m = p.ParseModule();
    return (p, m);
  }

  [Fact]
  public void UnexpectedToken_EmitsDiagnostic()
  {
    // Number literal does not start any production (predictions: <<, [[, identifier)
    var (p, _) = Parse("123");
    Assert.Contains(p.Diagnostics, d => d.Code == DiagCode.UnexpectedToken);
  }

  [Fact]
  public void MissingToken_EmitsDiagnostic()
  {
    // An import like: [[path]] without terminator maybe? Use struct decl missing close brace.
    var (p, _) = Parse("foo = %{\n"); // struct declaration missing closing }
    Assert.Contains(p.Diagnostics, d => d.Code == DiagCode.MissingToken);
  }

  [Fact]
  public void InvalidCharacter_EmitsDiagnostic()
  {
    var (p, _) = Parse("$");
    Assert.Contains(p.Diagnostics, d => d.Code == DiagCode.InvalidCharacter);
  }

  [Fact]
  public void UnterminatedString_EmitsDiagnostic()
  {
    var (p, _) = Parse("\"hello");
    Assert.Contains(p.Diagnostics, d => d.Code == DiagCode.UnterminatedString);
  }
}
