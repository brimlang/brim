using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class GenericUseSiteTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void StructFieldWithGenericArgumentListParses()
  {
    var m = Parse("[[m]];\nBox[T] := %{ inner:Wrapper[T] };");
    var sd = m.Members.OfType<StructDeclaration>().First();
    var field = sd.Fields[0];
    _ = Assert.IsType<GenericType>(field.TypeAnnotation);
    var gt = (GenericType)field.TypeAnnotation;
    _ = Assert.Single(gt.Arguments.Arguments);
  }

  [Fact]
  public void UnionVariantWithGenericArgumentListParses()
  {
    var m = Parse("[[m]];\nResult[T] := |{ Ok:Ok[T], Err:Err };");
    var ud = m.Members.OfType<UnionDeclaration>().First();
    var ok = ud.Variants[0];
    _ = Assert.IsType<GenericType>(ok.Type);
    var gt = (GenericType)ok.Type;
    _ = Assert.Single(gt.Arguments.Arguments);
  }
}
