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
  public void ServiceImpl_StateBlock_Parses_WithFields()
  {
    string src = "[[m]];\n<IntService, i>{\n  < accum :T, call_count :u64, >\n  ^(){} name() T {} ~(){}\n}\n";
    var m = Parse(src);
    var impl = m.Members.OfType<ServiceImpl>().FirstOrDefault();
    Assert.NotNull(impl);
    Assert.Contains("IntService", impl!.ServiceRef.GetText(src));
    Assert.Equal("i", impl.ReceiverIdent.GetText(src));
    Assert.Equal(2, impl.State.Fields.Count);
  }

  [Fact]
  public void ServiceImpl_StateBlock_Empty_Allows_Stateless()
  {
    string src = "[[m]];\n<S, _>{\n  <>\n  ^(){}\n}\n";
    var m = Parse(src);
    var impl = m.Members.OfType<ServiceImpl>().FirstOrDefault();
    Assert.NotNull(impl);
    Assert.Empty(impl!.State.Fields);
  }
}
