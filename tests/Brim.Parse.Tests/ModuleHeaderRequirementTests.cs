namespace Brim.Parse.Tests;

public class ModuleHeaderRequirementTests
{
  [Fact]
  public void MissingHeaderProducesDiagnosticAndFabricatedHeader()
  {
    var mod = Parser.ParseModule("<< Foo\nFoo = `%{ a:i32 }\n"); // starts with export, no header
    Assert.Contains(mod.Diagnostics, d => d.Code == DiagCode.MissingModuleHeader);
    // First directive should be the fabricated header
    var header = mod.ModuleDirective;
    Assert.Equal(Green.SyntaxKind.ModuleDirective, header.Kind);
  }

  [Fact]
  public void ProperHeaderFollowedByExportHasNoMissingHeaderDiag()
  {
    var mod = Parser.ParseModule("[[m]]\n<< Foo\nFoo = `%{ a:i32 }\n");
    Assert.DoesNotContain(mod.Diagnostics, d => d.Code == DiagCode.MissingModuleHeader);
  }
}