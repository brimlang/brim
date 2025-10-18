using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class GenericConstraintTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void SingleConstraintParses()
  {
    var m = Parse("=[m]=;\nFoo[T:Proto] := %{ a:T };");
    var decl = m.Members.OfType<TypeDeclaration>().FirstOrDefault();
    Assert.NotNull(decl);
    Assert.NotNull(decl!.Name.GenericParams);
    Assert.Single(decl.Name.GenericParams!.ParameterList.Elements);
    var gp = decl.Name.GenericParams!.ParameterList.Elements[0].Node;
    Assert.NotNull(gp.Constraints);
    Assert.Single(gp.Constraints!.Constraints);
  }

  [Fact]
  public void MultiConstraintParses()
  {
    var m = Parse("=[m]=;\nFoo[T:Proto+Other] := %{ a:T };");
    var decl = m.Members.OfType<TypeDeclaration>().FirstOrDefault();
    Assert.NotNull(decl);
    var gp = decl!.Name.GenericParams!.ParameterList.Elements[0].Node;
    Assert.NotNull(gp.Constraints);
    Assert.Equal(2, gp.Constraints!.Constraints.Count);
  }

  [Fact]
  public void MissingConstraintAfterColonEmitsDiagnostic()
  {
    var m = Parse("=[m]=;\nFoo[T:] := %{ a:T };");
    Assert.Contains(m.Diagnostics, d => d.Code == DiagCode.InvalidGenericConstraint);
  }

  [Fact]
  public void QualifiedConstraintParses()
  {
    string src = "=[m]=;\nFoo[T:runtime.Num] := T;";
    var m = Parse(src);
    var decl = m.Members.OfType<TypeDeclaration>().FirstOrDefault();
    Assert.NotNull(decl);
    var gp = decl!.Name.GenericParams!.ParameterList.Elements[0].Node;
    Assert.NotNull(gp.Constraints);
    Assert.Single(gp.Constraints!.Constraints);
    var constraintRef = gp.Constraints!.Constraints[0];
    Assert.IsType<TypeRef>(constraintRef.TypeNode);
    var typeRef = (TypeRef)constraintRef.TypeNode;
    Assert.Equal("Num", typeRef.Name.GetText(src));
    Assert.Single(typeRef.QualifierParts); // one qualifier: runtime + dot
    Assert.Equal("runtime", typeRef.QualifierParts[0].Name.GetText(src));
  }

  [Fact]
  public void MultipleQualifiedConstraintsParses()
  {
    string src = "=[m]=;\nNumbers[T:runtime.Num+runtime.Summable] := seq[T];";
    var m = Parse(src);
    var decl = m.Members.OfType<TypeDeclaration>().FirstOrDefault();
    Assert.NotNull(decl);
    var gp = decl!.Name.GenericParams!.ParameterList.Elements[0].Node;
    Assert.NotNull(gp.Constraints);
    Assert.Equal(2, gp.Constraints!.Constraints.Count);

    var first = (TypeRef)gp.Constraints!.Constraints[0].TypeNode;
    Assert.Equal("Num", first.Name.GetText(src));
    Assert.Single(first.QualifierParts);
    Assert.Equal("runtime", first.QualifierParts[0].Name.GetText(src));

    var second = (TypeRef)gp.Constraints!.Constraints[1].TypeNode;
    Assert.Equal("Summable", second.Name.GetText(src));
    Assert.Single(second.QualifierParts);
    Assert.Equal("runtime", second.QualifierParts[0].Name.GetText(src));
    Assert.NotNull(gp.Constraints!.Constraints[1].LeadingPlus);
  }

  [Fact]
  public void ConstraintWithGenericArgumentsParses()
  {
    string src = "=[m]=;\nContainer[T:Collection[Item] ] := seq[T];";
    var m = Parse(src);
    var decl = m.Members.OfType<TypeDeclaration>().FirstOrDefault();
    Assert.NotNull(decl);
    var gp = decl!.Name.GenericParams!.ParameterList.Elements[0].Node;
    Assert.NotNull(gp.Constraints);
    var typeRef = (TypeRef)gp.Constraints!.Constraints[0].TypeNode;
    Assert.NotNull(typeRef.GenericArgs);
    Assert.Single(typeRef.GenericArgs!.ArgumentList.Elements);
  }
}
