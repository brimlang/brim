using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class GenericConstraintTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void SingleConstraintParses()
  {
    var m = Parse("[[m]];\nFoo[T:Proto] := %{ a:T };");
    var decl = m.Members.OfType<StructDeclaration>().FirstOrDefault();
    Assert.NotNull(decl);
    Assert.NotNull(decl!.Name.GenericParams);
    Assert.Single(decl.Name.GenericParams!.Parameters);
    var gp = decl.Name.GenericParams!.Parameters[0];
    Assert.NotNull(gp.Constraints);
    Assert.Single(gp.Constraints!.Constraints);
  }

  [Fact]
  public void MultiConstraintParses()
  {
    var m = Parse("[[m]];\nFoo[T:Proto+Other] := %{ a:T };");
    var decl = m.Members.OfType<StructDeclaration>().FirstOrDefault();
    Assert.NotNull(decl);
    var gp = decl!.Name.GenericParams!.Parameters[0];
    Assert.NotNull(gp.Constraints);
    Assert.Equal(2, gp.Constraints!.Constraints.Count);
  }

  [Fact]
  public void MissingConstraintAfterColonEmitsDiagnostic()
  {
    var m = Parse("[[m]];\nFoo[T:] := %{ a:T };");
    Assert.Contains(m.Diagnostics, d => d.Code == DiagCode.InvalidGenericConstraint);
  }
}
