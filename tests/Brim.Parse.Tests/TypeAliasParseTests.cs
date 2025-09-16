using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class TypeAliasParseTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void Alias_SimpleType_Parses()
  {
    string src = "[[m]];\nAlias := Foo;\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members.First());
    Assert.Equal("Alias", td.Name.Identifier.GetText(src));
    // Identifier alias: type node is the identifier token
    var tok = Assert.IsType<GreenToken>(td.TypeNode);
    Assert.Equal(SyntaxKind.IdentifierToken, tok.SyntaxKind);
    Assert.Equal("Foo", tok.GetText(src));
  }

  [Fact]
  public void Alias_GenericType_Parses()
  {
    string src = "[[m]];\nAlias := Wrapper[T];\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members.First());
    var gt = Assert.IsType<GenericType>(td.TypeNode);
    Assert.Equal("Wrapper", gt.Name.GetText(src));
    Assert.Single(gt.Arguments.Arguments);
  }

  [Fact]
  public void Alias_WithGenericParamsOnName_Parses()
  {
    string src = "[[m]];\nAlias[T] := Wrapper[T];\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members.First());
    Assert.NotNull(td.Name.GenericParams);
    Assert.Single(td.Name.GenericParams!.Parameters);
    var gt = Assert.IsType<GenericType>(td.TypeNode);
    Assert.Single(gt.Arguments.Arguments);
  }

  [Fact]
  public void GenericParam_Constraints_Preserve_Plus_Separators()
  {
    string src = "[[m]];\nAlias[T: C1 + C2] := X;\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members.First());
    var gp = td.Name.GenericParams!;
    Assert.Single(gp.Parameters);
    var p0 = gp.Parameters[0];
    Assert.NotNull(p0.Constraints);
    Assert.Equal(2, p0.Constraints!.Constraints.Count);
    Assert.NotNull(p0.Constraints!.Constraints[0].TrailingPlus);
    Assert.Null(p0.Constraints!.Constraints[1].TrailingPlus);
  }

  [Fact]
  public void Alias_GenericArgs_TrailingComma_Allows()
  {
    string src = "[[m]];\nAlias := Outer[Inner,];\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members.First());
    var gt = Assert.IsType<GenericType>(td.TypeNode);
    Assert.Single(gt.Arguments.Arguments);
    Assert.NotNull(gt.Arguments.Arguments[0].TrailingComma);
  }

  [Fact]
  public void Alias_NestedGeneric_Parses()
  {
    string src = "[[m]];\nAlias := Outer[Inner[Deep]];\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members.First());
    var outer = Assert.IsType<GenericType>(td.TypeNode);
    Assert.Single(outer.Arguments.Arguments);
    var innerArg = outer.Arguments.Arguments[0];
    var inner = Assert.IsType<GenericType>(innerArg.TypeNode);
    Assert.Equal("Inner", inner.Name.GetText(src));
    Assert.Single(inner.Arguments.Arguments);
  }
}
