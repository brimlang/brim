using System.Linq;
using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class FunctionDeclarationTests
{
  const string Header = "=[test::module]=\n";

  static string TokenText(GreenToken token, string source) =>
    source.Substring(token.CoreToken.Offset, token.CoreToken.Length);

  [Fact]
  public void FunctionDecl_WithNamedParams()
  {
    string src = Header + "add :(a :i32, b :i32) i32 { a + b }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    FunctionDeclaration decl = ParserTestHelpers.GetMember<FunctionDeclaration>(module, 0);
    Assert.Equal(2, decl.Parameters.Elements.Length);

    FunctionParam firstParam = decl.Parameters.Elements[0].Node;
    Assert.Equal("a", TokenText(firstParam.Name, src));

    BlockExpr body = Assert.IsType<BlockExpr>(decl.Body);
    GreenNode lastStatement = body.StatementNodes().Last();
    Assert.IsType<BinaryExpr>(lastStatement);
  }

  [Fact]
  public void FunctionDecl_NoParams()
  {
    // Note: :() is ambiguous - defaults to value declaration
    // Use at least one parameter for unambiguous function declaration  
    string src = Header + "get_answer :() i32 = ||> 42\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    // Empty params parse as value declaration
    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    Assert.True(decl.Initializer is FunctionLiteral or ZeroParameterFunctionLiteral);
  }

  [Fact]
  public void FunctionDecl_DistinctFromValueDecl()
  {
    string src = Header +
      "add_val :(i32, i32) i32 = |a, b|> a + b\n" +
      "add_fn :(a :i32, b :i32) i32 { a + b }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    // First should be value declaration
    ValueDeclaration valueDecl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    Assert.IsType<FunctionLiteral>(valueDecl.Initializer);

    // Second should be function declaration
    FunctionDeclaration funcDecl = ParserTestHelpers.GetMember<FunctionDeclaration>(module, 1);
    Assert.Equal(2, funcDecl.Parameters.Elements.Length);
  }

  [Fact]
  public void FunctionDecl_WithGenericParams()
  {
    string src = Header + "ident[T] :(x :T) T { x }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    FunctionDeclaration decl = ParserTestHelpers.GetMember<FunctionDeclaration>(module, 0);
    Assert.NotNull(decl.Name.GenericParams);
    Assert.Single(decl.Parameters.Elements);
  }

  [Fact]
  public void FunctionDecl_MultiStatement()
  {
    string src = Header + "compute :(x :i32, y :i32) i32 {\n  z = x + 1\n  z * y\n}\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    FunctionDeclaration decl = ParserTestHelpers.GetMember<FunctionDeclaration>(module, 0);
    BlockExpr body = Assert.IsType<BlockExpr>(decl.Body);
    Assert.NotEmpty(body.StatementNodes());
  }
}
