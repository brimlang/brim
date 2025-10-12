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
    var td = Assert.IsType<TypeDeclaration>(m.Members[0]);
    Assert.Equal("Alias", td.Name.Identifier.GetText(src));
    // TypeExpr wraps TypeRef wraps identifier token
    var te = Assert.IsType<TypeExpr>(td.TypeNode);
    Assert.Null(te.Suffix);
    var tr = Assert.IsType<TypeRef>(te.Core);
    Assert.Equal(SyntaxKind.IdentifierToken, tr.Name.SyntaxKind);
    Assert.Equal("Foo", tr.Name.GetText(src));
  }

  [Fact]
  public void Alias_GenericType_Parses()
  {
    string src = "[[m]];\nAlias := Wrapper[T];\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members[0]);
    var te = Assert.IsType<TypeExpr>(td.TypeNode);
    var tr = Assert.IsType<TypeRef>(te.Core);
    Assert.Equal("Wrapper", tr.Name.GetText(src));
    Assert.NotNull(tr.GenericArgs);
    Assert.Single(tr.GenericArgs!.ArgumentList.Elements);
  }

  [Fact]
  public void Alias_WithGenericParamsOnName_Parses()
  {
    string src = "[[m]];\nAlias[T] := Wrapper[T];\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members[0]);
    Assert.NotNull(td.Name.GenericParams);
    Assert.Single(td.Name.GenericParams!.ParameterList.Elements);
    var te = Assert.IsType<TypeExpr>(td.TypeNode);
    var tr = Assert.IsType<TypeRef>(te.Core);
    Assert.NotNull(tr.GenericArgs);
    Assert.Single(tr.GenericArgs!.ArgumentList.Elements);
  }

  [Fact]
  public void GenericParam_Constraints_Preserve_Plus_Separators()
  {
    string src = "[[m]];\nAlias[T: C1 + C2] := X;\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members[0]);
    var gp = td.Name.GenericParams!;
    Assert.Single(gp.ParameterList.Elements);
    var p0 = gp.ParameterList.Elements[0].Node;
    Assert.NotNull(p0.Constraints);
    Assert.Equal(2, p0.Constraints!.Constraints.Count);
  }

  [Fact]
  public void Alias_GenericArgs_TrailingComma_Allows()
  {
    string src = "[[m]];\nAlias := Outer[Inner,];\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members[0]);
    var te = Assert.IsType<TypeExpr>(td.TypeNode);
    var tr = Assert.IsType<TypeRef>(te.Core);
    Assert.NotNull(tr.GenericArgs);
    Assert.Single(tr.GenericArgs!.ArgumentList.Elements);
    Assert.NotNull(tr.GenericArgs!.ArgumentList.TrailingComma); // trailing comma on list
  }

  [Fact]
  public void Alias_NestedGeneric_Parses()
  {
    string src = "[[m]];\nAlias := Outer[Inner[Deep]];\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members[0]);
    var te = Assert.IsType<TypeExpr>(td.TypeNode);
    var outer = Assert.IsType<TypeRef>(te.Core);
    Assert.NotNull(outer.GenericArgs);
    Assert.Single(outer.GenericArgs!.ArgumentList.Elements);
    var innerArgElem = outer.GenericArgs!.ArgumentList.Elements[0];
    var innerArgNode = Assert.IsType<GenericArgument>(innerArgElem.Node);
    var innerTe = Assert.IsType<TypeExpr>(innerArgNode.TypeNode);
    var inner = Assert.IsType<TypeRef>(innerTe.Core);
    Assert.Equal("Inner", inner.Name.GetText(src));
    Assert.NotNull(inner.GenericArgs);
    Assert.Single(inner.GenericArgs!.ArgumentList.Elements);
  }
}
