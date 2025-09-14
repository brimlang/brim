using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class GenericParseTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void StructWithGenericParamsParses()
  {
    var m = Parse("[[m]];\nMyStruct[T,U] := %{ a:T, b:U };");
    Assert.DoesNotContain(m.Diagnostics, static d => d.Code == DiagCode.UnexpectedToken);
    StructDeclaration? sd = m.Members.OfType<StructDeclaration>().FirstOrDefault();
    Assert.NotNull(sd);
    Assert.NotNull(sd!.Name.GenericParams);
    Assert.Equal(2, sd.Name.GenericParams!.Parameters.Length);
  }

  [Fact(Skip = "TODO: Broken")]
  public void StructWithEmptyGenericListAllowsMissingAndEmitsMissingTokenDiag()
  {
    var m2 = Parse("[[m]];\nFoo[] := %{ x:Foo };");
    Assert.Contains(m2.Diagnostics, static d => d.Code == DiagCode.EmptyGenericParamList);
  }
}
