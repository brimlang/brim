using Brim.Core;

namespace Brim.Parse.Tests;

public class ModuleHeaderRequirementTests
{
  [Fact]
  public void ProperHeaderFollowedByExportHasNoMissingHeaderDiag()
  {
    var mod = Parser.ParseModule("=[m]=\n<< Foo\nFoo : `%{ a:i32 }\n");
    Assert.DoesNotContain(mod.Diagnostics, d => d.Code == DiagCode.MissingModuleHeader);
  }
}
