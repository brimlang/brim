namespace Brim.Parse.Tests;

public class UnsupportedModuleMemberTests
{
  static Green.BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void FunctionHeaderAtModule_EmitsUnsupported()
  {
    string src = "=[m]=;\nfoo :(i32) i32\n"; // no body; header only
    var m = Parse(src);
    Assert.Contains(m.Diagnostics, d => d.Code == DiagCode.UnsupportedModuleMember);
  }
}

