using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class ServiceLifecycleTests
{
  const string Header = "=[test::module]=\n";

  [Fact]
  public void ServiceConstructor_WithBareServiceConstruct()
  {
    string src = Header +
      "MyService := @{ count :i32 }\n" +
      "MyService { (initial :i32) @! { @{ count = initial } } }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ServiceLifecycleDecl lifecycle = ParserTestHelpers.GetMember<ServiceLifecycleDecl>(module, 1);
    Assert.Single(lifecycle.Members);

    ServiceCtorDecl ctor = Assert.IsType<ServiceCtorDecl>(lifecycle.Members[0]);
    Assert.Single(ctor.ParamList.Elements);

    BlockExpr body = Assert.IsType<BlockExpr>(ctor.Block);
    Assert.IsType<ServiceConstruct>(body.Result);
  }

  [Fact]
  public void ServiceConstructor_WithMultipleStatements()
  {
    string src = Header +
      "MyService := @{ count :i32 }\n" +
      "MyService { (x :i32) @! { y = x + 1\n @{ count = y } } }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ServiceLifecycleDecl lifecycle = ParserTestHelpers.GetMember<ServiceLifecycleDecl>(module, 1);
    ServiceCtorDecl ctor = Assert.IsType<ServiceCtorDecl>(lifecycle.Members[0]);

    BlockExpr body = Assert.IsType<BlockExpr>(ctor.Block);
    Assert.NotEmpty(body.Statements);
    Assert.IsType<ServiceConstruct>(body.Result);
  }

  [Fact]
  public void ServiceDestructor_WithBody()
  {
    string src = Header +
      "MyService := @{ count :i32 }\n" +
      "MyService { ~(svc :@) unit { svc.count } }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ServiceLifecycleDecl lifecycle = ParserTestHelpers.GetMember<ServiceLifecycleDecl>(module, 1);
    Assert.Single(lifecycle.Members);

    ServiceDtorDecl dtor = Assert.IsType<ServiceDtorDecl>(lifecycle.Members[0]);
    Assert.Single(dtor.Params.Elements);

    BlockExpr body = Assert.IsType<BlockExpr>(dtor.Block);
    Assert.IsType<MemberAccessExpr>(body.Result);
  }

  [Fact]
  public void ServiceLifecycle_ConstructorAndDestructor()
  {
    string src = Header +
      "MyService := @{ count :i32 }\n" +
      "MyService {\n" +
      "  (initial :i32) @! { @{ count = initial } }\n" +
      "  ~(svc :@) unit { svc.count }\n" +
      "}\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ServiceLifecycleDecl lifecycle = ParserTestHelpers.GetMember<ServiceLifecycleDecl>(module, 1);
    Assert.Equal(2, lifecycle.Members.Length);

    Assert.IsType<ServiceCtorDecl>(lifecycle.Members[0]);
    Assert.IsType<ServiceDtorDecl>(lifecycle.Members[1]);
  }

  [Fact]
  public void ServiceMethod_WithBareServiceConstruct()
  {
    string src = Header +
      "MyService := @{ count :i32 }\n" +
      "MyProtocol := .{ reset :() MyService }\n" +
      "MyService<MyProtocol>(svc :@) {\n" +
      "  reset :() MyService { @{ count = 0 } }\n" +
      "}\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ServiceProtocolDecl protocol = ParserTestHelpers.GetMember<ServiceProtocolDecl>(module, 2);
    Assert.Single(protocol.Methods);

    ServiceMethodDecl method = Assert.IsType<ServiceMethodDecl>(protocol.Methods[0]);
    BlockExpr body = Assert.IsType<BlockExpr>(method.Block);
    Assert.IsType<ServiceConstruct>(body.Result);
  }

  [Fact]
  public void ServiceMethod_WithStatements()
  {
    string src = Header +
      "MyService := @{ count :i32 }\n" +
      "MyProtocol := .{ increment :() unit }\n" +
      "MyService<MyProtocol>(svc :@) {\n" +
      "  increment :() unit { svc.count = svc.count + 1 }\n" +
      "}\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ServiceProtocolDecl protocol = ParserTestHelpers.GetMember<ServiceProtocolDecl>(module, 2);
    ServiceMethodDecl method = Assert.IsType<ServiceMethodDecl>(protocol.Methods[0]);

    BlockExpr body = Assert.IsType<BlockExpr>(method.Block);
    Assert.NotEmpty(body.Statements);
  }

  [Fact]
  public void ServiceConstructor_EmptyParams()
  {
    string src = Header +
      "MyService := @{ count :i32 }\n" +
      "MyService { () @! { @{ count = 42 } } }\n";
    BrimModule module = ParserTestHelpers.ParseModule(src);

    ServiceLifecycleDecl lifecycle = ParserTestHelpers.GetMember<ServiceLifecycleDecl>(module, 1);
    ServiceCtorDecl ctor = Assert.IsType<ServiceCtorDecl>(lifecycle.Members[0]);

    Assert.Empty(ctor.ParamList.Elements);
  }
}
