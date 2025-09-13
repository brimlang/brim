namespace Brim.Parse.Tests;

public class LegacyEqualSyntaxTests
{
  static Green.BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void LegacyEqualStructIsUnexpected()
  {
    var m = Parse("[[m]];\nOld = %{ a:i32 };");
    Assert.Contains(m.Diagnostics, d => d.Code == DiagCode.UnexpectedToken);
  }
}
