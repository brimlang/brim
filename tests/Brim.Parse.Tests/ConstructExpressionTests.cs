using System.Linq;
using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class ConstructExpressionTests
{
  const string Header = "=[test::module]=\n";

  static string TokenText(GreenToken token, string source) =>
    source.Substring(token.CoreToken.Offset, token.CoreToken.Length);

  [Fact]
  public void SeqConstruct_WithGenericArgs()
  {
    string src = Header + "ints :seq[i32] = seq[i32]{ 1, 2, 3, 4, 5 }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    SeqConstruct seq = Assert.IsType<SeqConstruct>(decl.Initializer);

    Assert.NotNull(seq.GenericArgs);
    Assert.Equal(5, seq.Elements.Elements.Length);
  }

  [Fact]
  public void SeqConstruct_WithoutGenericArgs()
  {
    string src = Header + "items :list = seq{ 1, 2, 3 }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);

    // seq without generic args is parsed as identifier (no construct form)
    Assert.IsType<IdentifierExpr>(decl.Initializer);
  }

  [Fact]
  public void StructConstruct_WithFieldInits()
  {
    string src = Header + "user :User = User%{ name = \"Alice\", age = 30 }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    StructConstruct construct = Assert.IsType<StructConstruct>(decl.Initializer);

    Assert.Equal(2, construct.Fields.Elements.Length);

    FieldInit nameField = construct.Fields.Elements[0].Node;
    Assert.Equal("name", TokenText(nameField.Name, src));
    Assert.IsType<LiteralExpr>(nameField.Value);
  }

  [Fact]
  public void UnionConstruct_WithVariantValue()
  {
    string src = Header + "area :Area = Area|{ Admin = perms }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    UnionConstruct construct = Assert.IsType<UnionConstruct>(decl.Initializer);

    Assert.Equal("Admin", TokenText(construct.Variant.Name, src));
    Assert.NotNull(construct.Variant.EqualsToken);
    Assert.NotNull(construct.Variant.Value);
  }

  [Fact]
  public void UnionConstruct_WithoutVariantValue()
  {
    string src = Header + "area :Area = Area|{ Banned }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    UnionConstruct construct = Assert.IsType<UnionConstruct>(decl.Initializer);

    Assert.Equal("Banned", TokenText(construct.Variant.Name, src));
    Assert.Null(construct.Variant.EqualsToken);
    Assert.Null(construct.Variant.Value);
  }

  [Fact]
  public void FlagsConstruct_InMatchArm()
  {
    string src = Header + "perms :Perms = is_admin => {\n  true => Perms&{ read, write, exec }\n  false => Perms&{ read }\n}\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    MatchExpr match = Assert.IsType<MatchExpr>(decl.Initializer);

    MatchBlock block = Assert.IsType<MatchBlock>(match.Body);
    MatchArm firstArm = block.Arms[0].Arm;
    FlagsConstruct flags = Assert.IsType<FlagsConstruct>(firstArm.Target);
    Assert.Equal(3, flags.Flags.Elements.Length);

    MatchArm secondArm = block.Arms[1].Arm;
    FlagsConstruct singleFlag = Assert.IsType<FlagsConstruct>(secondArm.Target);
    Assert.Equal(1, singleFlag.Flags.Elements.Length);
  }

  [Fact]
  public void OptionConstruct_WithValue()
  {
    string src = Header + "x :i32? = ?{ 42 }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    OptionConstruct opt = Assert.IsType<OptionConstruct>(decl.Initializer);

    Assert.NotNull(opt.Expr);
    Assert.IsType<LiteralExpr>(opt.Expr);
  }

  [Fact]
  public void OptionConstruct_Empty()
  {
    string src = Header + "x :i32? = ?{ }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    OptionConstruct opt = Assert.IsType<OptionConstruct>(decl.Initializer);

    Assert.Null(opt.Expr);
  }

  [Fact]
  public void ResultConstruct_OkValue()
  {
    string src = Header + "x :result = !{ 42 }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    ResultConstruct result = Assert.IsType<ResultConstruct>(decl.Initializer);

    Assert.IsType<LiteralExpr>(result.Expr);
  }

  [Fact]
  public void ErrorConstruct_ErrorValue()
  {
    string src = Header + "x :err = !!{ \"Not Authorized\" }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    ErrorConstruct error = Assert.IsType<ErrorConstruct>(decl.Initializer);

    Assert.IsType<LiteralExpr>(error.Expr);
  }

  [Fact]
  public void StructConstruct_NestedInBlockExpr()
  {
    string src = Header + "get_user :() User = ||> { User%{ name = \"Bob\", age = 25 } }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    BlockExpr block = decl.Initializer switch
    {
      FunctionLiteral lambda => Assert.IsType<BlockExpr>(lambda.Body),
      ZeroParameterFunctionLiteral zero => Assert.IsType<BlockExpr>(zero.Body),
      _ => throw new Xunit.Sdk.XunitException("Expected lambda literal")
    };

    StructConstruct construct = block.StatementList.Elements
      .Select(e => e.Node)
      .OfType<StructConstruct>()
      .First();
    Assert.Equal(2, construct.Fields.Elements.Length);
  }

  [Fact]
  public void TupleConstruct_WithElements()
  {
    string src = Header + "point :Point = Point#{ 10, 20, 30 }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    TupleConstruct tuple = Assert.IsType<TupleConstruct>(decl.Initializer);

    Assert.Equal(3, tuple.Elements.Elements.Length);
  }

  [Fact]
  public void ServiceConstruct_WithFields()
  {
    string src = Header + "svc :MyService = MyService@{ count = 0, active = true }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    ServiceConstruct service = Assert.IsType<ServiceConstruct>(decl.Initializer);

    Assert.Equal(2, service.Fields.Elements.Length);
  }

  [Fact]
  public void ServiceConstruct_BareForm()
  {
    string src = Header + "svc :MyService = @{ count = 42, name = \"test\" }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    ServiceConstruct service = Assert.IsType<ServiceConstruct>(decl.Initializer);

    Assert.Equal(2, service.Fields.Elements.Length);

    FieldInit countField = service.Fields.Elements[0].Node;
    Assert.Equal("count", TokenText(countField.Name, src));
  }

  [Fact]
  public void ServiceConstruct_BareForm_InBlockExpression()
  {
    string src = Header + "make :(x :i32) MyService { @{ count = x } }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    FunctionDeclaration func = ParserTestHelpers.GetMember<FunctionDeclaration>(module, 0);
    BlockExpr block = Assert.IsType<BlockExpr>(func.Body);
    ServiceConstruct service = block.StatementList.Elements
      .Select(e => e.Node)
      .OfType<ServiceConstruct>()
      .First();

    Assert.Single(service.Fields.Elements);
  }

  [Fact]
  public void ServiceConstruct_BareForm_InConstructorBody()
  {
    string src = Header +
      "MyService := @{ count :i32 }\n" +
      "MyService { (initial :i32) @! { @{ count = initial } } }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ServiceLifecycleDecl lifecycle = ParserTestHelpers.GetMember<ServiceLifecycleDecl>(module, 1);
    ServiceCtorDecl ctor = Assert.IsType<ServiceCtorDecl>(lifecycle.Members[0]);

    BlockExpr block = Assert.IsType<BlockExpr>(ctor.Block);
    ServiceConstruct service = block.StatementList.Elements
      .Select(e => e.Node)
      .OfType<ServiceConstruct>()
      .First();

    Assert.Single(service.Fields.Elements);
  }

  [Fact]
  public void ServiceConstruct_BareForm_NestedFields()
  {
    string src = Header + "data :MyService = @{ x = 1, y = 2, nested = @{ z = 3 } }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    ServiceConstruct outer = Assert.IsType<ServiceConstruct>(decl.Initializer);

    Assert.Equal(3, outer.Fields.Elements.Length);

    FieldInit nestedField = outer.Fields.Elements[2].Node;
    ServiceConstruct inner = Assert.IsType<ServiceConstruct>(nestedField.Value);
    Assert.Single(inner.Fields.Elements);
  }
}
