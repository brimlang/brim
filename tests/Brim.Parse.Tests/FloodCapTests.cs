using System.Text;
using Brim.Parse.Collections;
namespace Brim.Parse.Tests;

public class FloodCapTests
{
  static Green.BrimModule Parse(string src) => Parser.ParseModule(src);

  // Create many separate invalid tokens by separating with spaces so lexer can't coalesce them.
  static string MakeInvalidSeparated(int count)
  {
    StringBuilder sb = new(count * 2);
    for (int i = 0; i < count; i++)
      _ = sb.Append('$').Append(' ');
    return sb.ToString();
  }

  [Fact]
  public void EmitsTooManyErrorsDiagnosticAtCap()
  {
    // Generate more invalid tokens than cap to force suppression.
    string src = MakeInvalidSeparated(DiagnosticList.MaxDiagnostics + 50);
    var mod = Parse(src);
    var diags = mod.Diagnostics;
    Assert.NotEmpty(diags);
    Assert.True(diags.Count <= DiagnosticList.MaxDiagnostics);
    Assert.Equal(DiagCode.TooManyErrors, diags[^1].Code);
  }

  [Fact]
  public void BelowCapDoesNotEmitTooManyErrors()
  {
    // Use a conservative count well below half (each invalid may trigger multiple diagnostics)
    int invalidCount = (DiagnosticList.MaxDiagnostics / 4) - 10;
    if (invalidCount < 1) invalidCount = 1;
    string src = MakeInvalidSeparated(invalidCount);
    var mod = Parse(src);
    var diags = mod.Diagnostics;
    Assert.NotEmpty(diags);
    Assert.True(diags.Count < DiagnosticList.MaxDiagnostics);
    Assert.DoesNotContain(diags, static d => d.Code == DiagCode.TooManyErrors);
  }
}
