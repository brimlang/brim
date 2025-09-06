namespace Brim.Parse.Tests;

public class ParserStallRegressionTests
{
  static string MakeInvalidStream(int length) =>
    // Use '$' which lexer treats as invalid character (diagnostic) but still advances one char per tokenization.
    new('$', length);

  [Fact]
  [Trait("Category", "ParseGuard")]
  public void ParserTerminatesOnInvalidStream()
  {
    string src = MakeInvalidStream(4_000); // sizeable but should parse quickly
  var mod = Parser.ParseModule(src);
  var diags = mod.Diagnostics;
    // Expect at least one InvalidCharacter diagnostic
    Assert.Contains(diags, static d => d.Code == DiagCode.InvalidCharacter);
    Assert.True(diags.Count > 0);
  }

  [Fact]
  public void ParserDoesNotExceedIterationBackstop()
  {
  string src = MakeInvalidStream(8_000); // still should terminate well below backstop
  var mod2 = Parser.ParseModule(src);
  var diags = mod2.Diagnostics;
    // Bounded by our guard logic; assert we at least progressed (non-zero diags) and not absurdly high.
    Assert.NotEmpty(diags);
  Assert.True(diags.Count < 20_000);
  }
}
