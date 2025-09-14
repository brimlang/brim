using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class TrailingCommaTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void Struct_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nS := %{ a:A, b:B, };\n");
    var sd = m.Members.OfType<StructDeclaration>().First();
    Assert.Equal(2, sd.Fields.Count);
    Assert.NotNull(sd.Fields[0].TrailingComma);
    Assert.NotNull(sd.Fields[1].TrailingComma); // trailing on last
  }

  [Fact]
  public void Union_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nU := |{ A:A, B:B, };\n");
    var ud = m.Members.OfType<UnionDeclaration>().First();
    Assert.Equal(2, ud.Variants.Count);
    Assert.NotNull(ud.Variants[0].TrailingComma);
    Assert.NotNull(ud.Variants[1].TrailingComma); // trailing
  }

  [Fact]
  public void Flags_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nF := & prim { ONE, TWO, };\n");
    var fd = m.Members.OfType<FlagsDeclaration>().First();
    Assert.Equal(2, fd.Members.Count);
    Assert.NotNull(fd.Members[0].TrailingComma);
    Assert.NotNull(fd.Members[1].TrailingComma);
  }

  [Fact]
  public void NamedTuple_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nPair := #{ A, B, };\n");
    var nt = m.Members.OfType<NamedTupleDeclaration>().First();
    Assert.Equal(2, nt.Elements.Count);
    Assert.NotNull(nt.Elements[0].TrailingComma);
    Assert.NotNull(nt.Elements[1].TrailingComma);
  }

  [Fact]
  public void GenericParams_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nBox[T,U,] := %{ inner: T };\n");
    var sd = m.Members.OfType<StructDeclaration>().First();
    var gp = sd.Name.GenericParams!;
    Assert.Equal(2, gp.Parameters.Length);
    Assert.NotNull(gp.Parameters[0].TrailingComma);
    Assert.NotNull(gp.Parameters[1].TrailingComma);
  }

  [Fact]
  public void GenericArgs_Allows_Trailing_Comma()
  {
    var m = Parse("[[m]];\nWrap := %{ field: Outer[Inner,] };\n");
    var sd = m.Members.OfType<StructDeclaration>().First();
    var field = sd.Fields[0];
    var gt = Assert.IsType<GenericType>(field.TypeAnnotation);
    Assert.Single(gt.Arguments.Arguments); // one argument
    Assert.NotNull(gt.Arguments.Arguments[0].TrailingComma);
  }
}
