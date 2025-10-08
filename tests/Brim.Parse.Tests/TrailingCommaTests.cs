using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class TrailingCommaTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void Struct_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nS := %{ a:A, b:B, };\n");
    var td = m.Members.OfType<TypeDeclaration>().First();
    var te = Assert.IsType<TypeExpr>(td.TypeNode);
    var ss = Assert.IsType<StructShape>(te.Core);
    Assert.Equal(2, ss.Fields.Count);
    Assert.NotNull(ss.Fields[0].TrailingComma);
    Assert.NotNull(ss.Fields[1].TrailingComma); // trailing on last
  }

  [Fact]
  public void UnionType_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nU := |{ A:A, B:B, };\n");
    var td = m.Members.OfType<TypeDeclaration>().First();
    var te = Assert.IsType<TypeExpr>(td.TypeNode);
    var us = Assert.IsType<UnionShape>(te.Core);
    Assert.Equal(2, us.Variants.Count);
    Assert.NotNull(us.Variants[0].TrailingComma);
    Assert.NotNull(us.Variants[1].TrailingComma); // trailing
  }

  [Fact]
  public void Flags_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nF := &{ ONE, TWO, };\n");
    var td = m.Members.OfType<TypeDeclaration>().First();
    var te = Assert.IsType<TypeExpr>(td.TypeNode);
    var fs = Assert.IsType<FlagsShape>(te.Core);
    Assert.Equal(2, fs.MemberList.Elements.Count);
  }

  [Fact]
  public void NamedTuple_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nPair := #{ A, B, };\n");
    var td = m.Members.OfType<TypeDeclaration>().First();
    var te = Assert.IsType<TypeExpr>(td.TypeNode);
    var nts = Assert.IsType<NamedTupleShape>(te.Core);
    Assert.Equal(2, nts.ElementList.Elements.Count);
    Assert.NotNull(nts.ElementList.Elements[1].LeadingComma); // interior comma on second element
    Assert.NotNull(nts.ElementList.TrailingComma); // trailing comma on list
  }

  [Fact]
  public void GenericParams_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nBox[T,U,] := %{ inner: T };\n");
    var td = m.Members.OfType<TypeDeclaration>().First();
    var gp = td.Name.GenericParams!;
    Assert.Equal(2, gp.Parameters.Length);
    Assert.NotNull(gp.Parameters[0].TrailingComma);
    Assert.NotNull(gp.Parameters[1].TrailingComma);
  }

  [Fact]
  public void GenericArgs_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nWrap := %{ field: Outer[Inner,] };\n");
    var td = m.Members.OfType<TypeDeclaration>().First();
    var tde = Assert.IsType<TypeExpr>(td.TypeNode);
    var ss = Assert.IsType<StructShape>(tde.Core);
    var field = ss.Fields[0];
    var fieldType = Assert.IsType<TypeExpr>(field.TypeAnnotation);
    var tr = Assert.IsType<TypeRef>(fieldType.Core);
    Assert.NotNull(tr.GenericArgs);
    Assert.Single(tr.GenericArgs!.ArgumentList.Elements); // one argument
    Assert.NotNull(tr.GenericArgs!.ArgumentList.TrailingComma); // trailing comma on list
  }

  [Fact]
  public void FunctionType_Params_TrailingComma_Allows()
  {
    var m = Parse("[[m]];\nF := (A, B,) C;\n");
    var td = m.Members.OfType<TypeDeclaration>().First();
    var tde = Assert.IsType<TypeExpr>(td.TypeNode);
    var fte = Assert.IsType<FunctionTypeExpr>(tde.Core);
    var ft = fte.Shape;
    Assert.Equal(2, ft.ParameterList.Elements.Count);
    Assert.NotNull(ft.ParameterList.Elements[1].LeadingComma); // interior comma on second element
    Assert.NotNull(ft.ParameterList.TrailingComma); // trailing comma now on list
  }
}
