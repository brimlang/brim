using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class TrailingCommaTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void Struct_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nS : %{ a:A, b:B, };\n");
    var sd = m.Members.OfType<StructDeclaration>().First();
    Assert.Equal(2, sd.Fields.Count);
    Assert.Equal(2, sd.FieldSeparators.Count); // includes trailing
  }

  [Fact]
  public void Union_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nU : |{ A:A, B:B, };\n");
    var ud = m.Members.OfType<UnionDeclaration>().First();
    Assert.Equal(2, ud.Variants.Count);
    Assert.Equal(2, ud.VariantSeparators.Count);
  }

  [Fact]
  public void Flags_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nF : & prim { ONE, TWO, };\n");
    var fd = m.Members.OfType<FlagsDeclaration>().First();
    Assert.Equal(2, fd.Members.Count);
    Assert.Equal(2, fd.MemberSeparators.Count);
  }

  [Fact]
  public void NamedTuple_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nPair : #{ A, B, };\n");
    var nt = m.Members.OfType<NamedTupleDeclaration>().First();
    Assert.Equal(2, nt.ElementTypes.Count);
    Assert.Equal(2, nt.ElementSeparators.Count);
  }

  [Fact]
  public void GenericParams_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nBox[T,U,] : %{ inner: T };\n");
    var sd = m.Members.OfType<StructDeclaration>().First();
    Assert.NotNull(sd.Name.GenericParams);
    Assert.Equal(2, sd.Name.GenericParams!.Parameters.Length);
    Assert.Equal(2, sd.Name.GenericParams!.ParameterSeparators.Count);
  }

  [Fact]
  public void GenericArgs_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nWrap : %{ field: Outer[Inner,] };\n");
    var sd = m.Members.OfType<StructDeclaration>().First();
    var field = sd.Fields[0];
    var gt = Assert.IsType<GenericType>(field.TypeAnnotation);
    Assert.Single(gt.Arguments.Arguments); // one argument
    Assert.Single(gt.Arguments.ArgumentSeparators); // trailing comma present
  }
}
