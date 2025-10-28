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
    string src = Header + "result :i32 = flag => {\n  true ?? cond => 1\n  false => 0\n}\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration result = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    MatchExpr match = Assert.IsType<MatchExpr>(result.Initializer);

    MatchBlock block = Assert.IsType<MatchBlock>(match.Body);
    Assert.Equal(2, block.Arms.Length);
    MatchArm guarded = block.Arms[0].Arm;
    Assert.NotNull(guarded.Guard);
    Assert.IsType<MatchGuard>(guarded.Guard);

    MatchArm fallback = block.Arms[1].Arm;
    Assert.Null(fallback.Guard);
  }

  [Fact]
  public void MatchExpression_SingleLine_SingleArm()
  {
    string src = Header + "result :i32 = flag => _ => 1\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration result = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    MatchExpr match = Assert.IsType<MatchExpr>(result.Initializer);

    // Single-line match: Body is MatchArm directly (not MatchBlock)
    MatchArm arm = Assert.IsType<MatchArm>(match.Body);
    Assert.IsType<BindingPattern>(arm.Pattern);
    // The terminator at end of line is preserved in the arm
    Assert.NotNull(arm.Terminator);
  }

  [Fact]
  public void MatchExpression_MultiLine_RequiresBraces()
  {
    string src = Header + "result :i32 = flag => {\n  true => 1\n  false => 0\n}\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration result = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    MatchExpr match = Assert.IsType<MatchExpr>(result.Initializer);

    // Multi-line match: Body is MatchBlock
    MatchBlock block = Assert.IsType<MatchBlock>(match.Body);
    Assert.Equal(2, block.Arms.Length);
    Assert.NotNull(block.OpenBrace);
    Assert.NotNull(block.CloseBrace);
  }

  [Fact]
  public void MatchBlock_PreservesLeadingTerminator()
  {
    string src = Header + "result :i32 = flag => {\n\n  true => 1\n  false => 0\n}\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration result = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    MatchExpr match = Assert.IsType<MatchExpr>(result.Initializer);
    MatchBlock block = Assert.IsType<MatchBlock>(match.Body);

    // Should have leading terminator preserved
    Assert.NotNull(block.LeadingTerminator);
  }

  [Fact]
  public void MatchBlock_TerminatorAfterLastArm()
  {
    string src = Header + "result :i32 = flag => {\n  _ => 1\n\n}\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration result = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    MatchExpr match = Assert.IsType<MatchExpr>(result.Initializer);
    MatchBlock block = Assert.IsType<MatchBlock>(match.Body);

    // The double newline is captured by the last arm's terminator
    Assert.NotNull(block.Arms[0].Arm.Terminator);
    // No separate trailing terminator since it's part of the arm
    Assert.Null(block.TrailingTerminator);
  }

  [Fact]
  public void MatchBlock_ArmTerminators_PreservedInOrder()
  {
    string src = Header + "result :i32 = flag => {\n  true => 1\n  false => 0\n}\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration result = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    MatchExpr match = Assert.IsType<MatchExpr>(result.Initializer);
    MatchBlock block = Assert.IsType<MatchBlock>(match.Body);

    // First arm should have terminator
    Assert.NotNull(block.Arms[0].Arm.Terminator);
    // Second arm should have terminator
    Assert.NotNull(block.Arms[1].Arm.Terminator);
  }

  [Fact]
  public void MatchExpression_NestedInMatch_RequiresBraces()
  {
    string src = Header + "result :i32 = x => {\n  0 => y => {\n    1 => 1\n    _ => 0\n  }\n  _ => 0\n}\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration result = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    MatchExpr outerMatch = Assert.IsType<MatchExpr>(result.Initializer);
    MatchBlock outerBlock = Assert.IsType<MatchBlock>(outerMatch.Body);

    // First arm's target should be another match expression
    MatchArm firstArm = outerBlock.Arms[0].Arm;
    MatchExpr innerMatch = Assert.IsType<MatchExpr>(firstArm.Target);
    MatchBlock innerBlock = Assert.IsType<MatchBlock>(innerMatch.Body);
    Assert.Equal(2, innerBlock.Arms.Length);
  }

  [Fact]
  public void BlockExpression_OptionalTrailingTerminator()
  {
    string src = Header + "f :(i32) i32 = |x|> {\n  x + 1\n}\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
    BlockExpr block = Assert.IsType<BlockExpr>(func.Body);

    // Should have result expression
    Assert.IsType<BinaryExpr>(block.Result);
    // No error should be raised
  }

  [Fact]
  public void BlockExpression_NoTrailingTerminator()
  {
    string src = Header + "f :(i32) i32 = |x|> { x + 1 }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
    BlockExpr block = Assert.IsType<BlockExpr>(func.Body);

    // Should have result expression
    Assert.IsType<BinaryExpr>(block.Result);
  }

  [Fact]
  public void BlockExpression_MultipleStatements_WithTrailingTerminator()
  {
    string src = Header + "f :(i32) i32 = |x|> {\n  x\n  x + 1\n}\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration value = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    FunctionLiteral func = Assert.IsType<FunctionLiteral>(value.Initializer);
    BlockExpr block = Assert.IsType<BlockExpr>(func.Body);

    // Should have 1 statement (x with terminator) and a result (x + 1)
    Assert.Equal(1, block.Statements.Length);
    Assert.IsType<BinaryExpr>(block.Result);
  }
}
