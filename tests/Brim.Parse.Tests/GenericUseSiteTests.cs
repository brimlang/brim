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
    var tde = Assert.IsType<TypeExpr>(td.TypeNode);
    var ss = Assert.IsType<StructShape>(tde.Core);
    var field = ss.Fields[0];
    var fieldType = Assert.IsType<TypeExpr>(field.TypeAnnotation);
    var tr = Assert.IsType<TypeRef>(fieldType.Core);
    Assert.NotNull(tr.GenericArgs);
    _ = Assert.Single(tr.GenericArgs!.ArgumentList.Elements);
  }

  [Fact]
  public void UnionVariantWithGenericArgumentListParses()
  {
    var m = Parse("[[m]];\nResult[T] := |{ Ok:Ok[T], Err:Err };");
    var td = m.Members.OfType<TypeDeclaration>().First();
    var tde = Assert.IsType<TypeExpr>(td.TypeNode);
    var us = Assert.IsType<UnionShape>(tde.Core);
    var ok = us.Variants[0];
    var okType = Assert.IsType<TypeExpr>(ok.Type);
    var tr = Assert.IsType<TypeRef>(okType.Core);
    Assert.NotNull(tr.GenericArgs);
    _ = Assert.Single(tr.GenericArgs!.ArgumentList.Elements);
  }
}
