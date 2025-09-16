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
    var ss = Assert.IsType<StructShape>(td.TypeNode);
    Assert.Equal(2, ss.Fields.Count);
    Assert.NotNull(ss.Fields[0].TrailingComma);
    Assert.NotNull(ss.Fields[1].TrailingComma); // trailing on last
  }

  [Fact]
  public void UnionType_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nU := |{ A:A, B:B, };\n");
    var td = m.Members.OfType<TypeDeclaration>().First();
    var us = Assert.IsType<UnionShape>(td.TypeNode);
    Assert.Equal(2, us.Variants.Count);
    Assert.NotNull(us.Variants[0].TrailingComma);
    Assert.NotNull(us.Variants[1].TrailingComma); // trailing
  }

  [Fact]
  public void Flags_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nF := & prim { ONE, TWO, };\n");
    var td = m.Members.OfType<TypeDeclaration>().First();
    var fs = Assert.IsType<FlagsShape>(td.TypeNode);
    Assert.Equal(2, fs.Members.Count);
    Assert.NotNull(fs.Members[0].TrailingComma);
    Assert.NotNull(fs.Members[1].TrailingComma);
  }

  [Fact]
  public void NamedTuple_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nPair := #{ A, B, };\n");
    var td = m.Members.OfType<TypeDeclaration>().First();
    var nts = Assert.IsType<NamedTupleShape>(td.TypeNode);
    Assert.Equal(2, nts.Elements.Count);
    Assert.NotNull(nts.Elements[0].TrailingComma);
    Assert.NotNull(nts.Elements[1].TrailingComma);
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
    var ss = Assert.IsType<StructShape>(td.TypeNode);
    var field = ss.Fields[0];
    var gt = Assert.IsType<GenericType>(field.TypeAnnotation);
    Assert.Single(gt.Arguments.Arguments); // one argument
    Assert.NotNull(gt.Arguments.Arguments[0].TrailingComma);
  }

  [Fact]
  public void FunctionType_Params_TrailingComma_Allows()
  {
    var m = Parse("[[m]];\nF := (A, B,) C;\n");
    var td = m.Members.OfType<TypeDeclaration>().First();
    var ft = Assert.IsType<FunctionShape>(td.TypeNode);
    Assert.Equal(2, ft.Parameters.Count);
    Assert.NotNull(ft.Parameters[0].TrailingComma);
    Assert.NotNull(ft.Parameters[1].TrailingComma); // trailing on last
  }
}
