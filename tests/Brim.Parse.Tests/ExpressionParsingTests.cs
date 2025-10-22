using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class ExpressionParsingTests
{
  const string Header = "=[test::module]=\n";

  [Fact]
  public void LambdaLiteral_CanonicalValueDeclaration()
  {
    string src = Header + "ident[T] :(T) T = |x|> x\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);

    Assert.NotNull(value.Name.GenericParams);
    Assert.IsType<FunctionTypeExpr>(value.TypeNode.Core);

    FunctionLiteral literal = Assert.IsType<FunctionLiteral>(value.Initializer);
    Assert.Equal(1, literal.Parameters.Parameters.Length);

    LambdaParams.Parameter parameter = literal.Parameters.Parameters[0];
    Assert.Null(parameter.LeadingComma);
  }

  [Fact]
  public void LambdaLiteral_WithTypeAlias()
  {
    string src = Header + "adder := (i32, i32) i32\nadd_a :adder = |a, b|> a + b\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 1);
    FunctionLiteral literal = Assert.IsType<FunctionLiteral>(value.Initializer);
    Assert.Equal(2, literal.Parameters.Parameters.Length);
  }

  [Fact]
  public void Dispatcher_TypeDeclarationAfterGenericName()
  {
    string src = Header + "adder := (i32, i32) i32\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    TypeDeclaration decl = ParserTestHelpers.GetMember<TypeDeclaration>(module, 0);
    Assert.Null(decl.Name.GenericParams);
    Assert.IsType<FunctionTypeExpr>(decl.TypeNode.Core);
  }

  [Fact]
  public void MatchExpression_WithGuards()
  {
    string src = Header + "result :i32 = flag =>\n  true ?? cond => 1\n  false => 0\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration result = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    MatchExpr match = Assert.IsType<MatchExpr>(result.Initializer);

    Assert.Equal(2, match.Arms.Arms.Length);
    MatchArm guarded = match.Arms.Arms[0];
    Assert.NotNull(guarded.Guard);
    Assert.IsType<MatchGuard>(guarded.Guard);

    MatchArm fallback = match.Arms.Arms[1];
    Assert.Null(fallback.Guard);
  }
}
