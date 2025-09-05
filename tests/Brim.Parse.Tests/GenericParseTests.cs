using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class GenericParseTests
{
  static (BrimModule module, IReadOnlyList<Diagnostic> diags) Parse(string src) => ParseFacade.ParseModule(src);

  [Fact]
  public void StructWithGenericParamsParses()
  {
    var (m, diags) = Parse("[[m]];\nMyStruct[T,U] = %{ a:T, b:U };");
    Assert.DoesNotContain(diags, d => d.Code == DiagCode.UnexpectedToken);
    StructDeclaration? sd = m.Members.OfType<StructDeclaration>().FirstOrDefault();
    Assert.NotNull(sd);
    Assert.NotNull(sd!.GenericParams);
    Assert.Equal(2, sd.GenericParams!.Parameters.Length);
  }

  [Fact]
  public void StructWithEmptyGenericListAllowsMissingAndEmitsMissingTokenDiag()
  {
    var (_, diags2) = Parse("[[m]];\nFoo[] = %{ x:Foo };");
    Assert.Contains(diags2, d => d.Code == DiagCode.EmptyGenericParamList);
  }
}
