using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class GenericUseSiteTests
{
  static (BrimModule module, IReadOnlyList<Diagnostic> diags) Parse(string src) => ParseFacade.ParseModule(src);

  [Fact]
  public void StructFieldWithGenericArgumentListParses()
  {
    var (m, _) = Parse("[[m]];\nBox[T] = %{ inner:Wrapper[T] };");
    var sd = m.Members.OfType<StructDeclaration>().First();
    var field = sd.Fields[0];
    _ = Assert.IsType<GenericType>(field.TypeAnnotation);
    var gt = (GenericType)field.TypeAnnotation;
    _ = Assert.Single(gt.Arguments.Arguments);
  }

  [Fact]
  public void UnionVariantWithGenericArgumentListParses()
  {
    var (m, _) = Parse("[[m]];\nResult[T] = |{ Ok:Ok[T], Err:Err };");
    var ud = m.Members.OfType<UnionDeclaration>().First();
    var ok = ud.Variants[0];
    _ = Assert.IsType<GenericType>(ok.Type);
    var gt = (GenericType)ok.Type;
    _ = Assert.Single(gt.Arguments.Arguments);
  }
}
