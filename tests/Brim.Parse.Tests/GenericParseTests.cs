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
    TypeDeclaration? td = m.Members.OfType<TypeDeclaration>().FirstOrDefault();
    Assert.NotNull(td);
    Assert.NotNull(td!.Name.GenericParams);
    Assert.Equal(2, td.Name.GenericParams!.Parameters.Length);
  }

  [Fact]
  public void StructWithEmptyGenericListAllowsMissingAndEmitsMissingTokenDiag()
  {
    var m2 = Parse("[[m]];\nFoo[] := %{ x:Foo };");
    Assert.Contains(m2.Diagnostics, static d => d.Code == DiagCode.EmptyGenericParamList);
  }
}
