using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class GenericUseSiteTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void StructFieldWithGenericArgumentListParses()
  {
    var m = Parse("[[m]];\nBox[T] := %{ inner:Wrapper[T] };");
    var td = m.Members.OfType<TypeDeclaration>().First();
    var ss = Assert.IsType<StructShape>(td.TypeNode);
    var field = ss.Fields[0];
    _ = Assert.IsType<GenericType>(field.TypeAnnotation);
    var gt = (GenericType)field.TypeAnnotation;
    _ = Assert.Single(gt.Arguments.ArgumentList.Elements);
  }

  [Fact]
  public void UnionVariantWithGenericArgumentListParses()
  {
    var m = Parse("[[m]];\nResult[T] := |{ Ok:Ok[T], Err:Err };");
    var td = m.Members.OfType<TypeDeclaration>().First();
    var us = Assert.IsType<UnionShape>(td.TypeNode);
    var ok = us.Variants[0];
    _ = Assert.IsType<GenericType>(ok.Type);
    var gt = (GenericType)ok.Type;
    _ = Assert.Single(gt.Arguments.ArgumentList.Elements);
  }
}
