using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class ServiceParseTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void ServiceType_Protocols_List_TrailingComma_Parses()
  {
    string src = "[[m]];\nIntService[T] := ^{ Adder[T], Fmt, };\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members.First());
    var sv = Assert.IsType<ServiceShape>(td.TypeNode);
    Assert.Equal(2, sv.Protocols.Count);
    Assert.NotNull(sv.Protocols[0].TrailingComma);
    Assert.NotNull(sv.Protocols[1].TrailingComma);
  }

  [Fact]
  public void ServiceImpl_InitDecls_Parses_WithFields()
  {
    string src = "[[m]];\n^IntService<i>(){\n  accum :T = seed\n  @call_count :u64 = 0u64\n  name() T {} ~ {}\n}\n";
    var m = Parse(src);
    var impl = m.Members.OfType<ServiceImpl>().FirstOrDefault();
    Assert.NotNull(impl);
    Assert.Contains("IntService", impl!.ServiceRef.GetText(src));
    Assert.Equal("i", impl.ReceiverIdent.GetText(src));
    Assert.Equal(2, impl.InitDecls.Count);
  }

  [Fact]
  public void ServiceImpl_ZeroState_Allows_Stateless()
  {
    string src = "[[m]];\n^S<_>(){ name() T {} }\n";
    var m = Parse(src);
    var impl = m.Members.OfType<ServiceImpl>().FirstOrDefault();
    Assert.NotNull(impl);
    Assert.Empty(impl!.InitDecls);
  }
  
  [Fact]
  public void ServiceImpl_Ignores_Destructor_After_Methods()
  {
    string src = "[[m]];\n^S<i>(){ name() T {} ~ { } }\n";
    var m = Parse(src);
    var impl = m.Members.OfType<ServiceImpl>().FirstOrDefault();
    Assert.NotNull(impl);
    Assert.DoesNotContain(impl!.Members, n => n is ServiceDtorHeader);
    Assert.Contains(impl.Members, n => n is ServiceMethodHeader);
  }

  [Fact]
  public void ServiceImpl_Only_One_Destructor_Recognized()
  {
    string src = "[[m]];\n^S<i>(){ ~ { } ~ { } name() T {} }\n";
    var m = Parse(src);
    var impl = m.Members.OfType<ServiceImpl>().FirstOrDefault();
    Assert.NotNull(impl);
    int dtorCount = impl!.Members.Count(n => n is ServiceDtorHeader);
    Assert.Equal(1, dtorCount);
  }
}
