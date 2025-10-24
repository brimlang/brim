using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class ConstructExpressionTests
{
  const string Header = "=[test::module]=\n";

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
    Assert.Equal("name", nameField.Name.Token.Value(src));
    Assert.IsType<LiteralExpr>(nameField.Value);
  }

  [Fact]
  public void UnionConstruct_WithVariantValue()
  {
    string src = Header + "area :Area = Area|{ Admin = perms }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    UnionConstruct construct = Assert.IsType<UnionConstruct>(decl.Initializer);
    
    Assert.Equal("Admin", construct.Variant.Name.Token.Value(src));
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
    
    Assert.Equal("Banned", construct.Variant.Name.Token.Value(src));
    Assert.Null(construct.Variant.EqualsToken);
    Assert.Null(construct.Variant.Value);
  }

  [Fact]
  public void FlagsConstruct_InMatchArm()
  {
    string src = Header + "perms :Perms = is_admin =>\n  true => Perms&{ read, write, exec }\n  false => Perms&{ read }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    MatchExpr match = Assert.IsType<MatchExpr>(decl.Initializer);
    
    MatchArm firstArm = match.Arms.Arms[0];
    FlagsConstruct flags = Assert.IsType<FlagsConstruct>(firstArm.Target);
    Assert.Equal(3, flags.Flags.Elements.Length);
    
    MatchArm secondArm = match.Arms.Arms[1];
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
    
    Assert.NotNull(opt.Value);
    Assert.IsType<LiteralExpr>(opt.Value);
  }

  [Fact]
  public void OptionConstruct_Empty()
  {
    string src = Header + "x :i32? = ?{ }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    OptionConstruct opt = Assert.IsType<OptionConstruct>(decl.Initializer);
    
    Assert.Null(opt.Value);
  }

  [Fact]
  public void ResultConstruct_OkValue()
  {
    string src = Header + "x :result = !{ 42 }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    ResultConstruct result = Assert.IsType<ResultConstruct>(decl.Initializer);
    
    Assert.IsType<LiteralExpr>(result.Value);
  }

  [Fact]
  public void ErrorConstruct_ErrorValue()
  {
    string src = Header + "x :err = !!{ \"Not Authorized\" }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    ErrorConstruct error = Assert.IsType<ErrorConstruct>(decl.Initializer);
    
    Assert.IsType<LiteralExpr>(error.Value);
  }

  [Fact]
  public void StructConstruct_NestedInBlockExpr()
  {
    string src = Header + "get_user :() User = ||> { User%{ name = \"Bob\", age = 25 } }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ValueDeclaration decl = ParserTestHelpers.GetMember<ValueDeclaration>(module, 0);
    FunctionLiteral lambda = Assert.IsType<FunctionLiteral>(decl.Initializer);
    BlockExpr block = Assert.IsType<BlockExpr>(lambda.Body);
    
    StructConstruct construct = Assert.IsType<StructConstruct>(block.Result);
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
}
