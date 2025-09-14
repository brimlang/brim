using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class ProtocolServiceHeaderTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void Protocol_Header_EmptyBody_Parses()
  {
    string src = "[[m]];\nProto :. {}\n";
    var m = Parse(src);
    var decl = Assert.IsType<ProtocolDeclaration>(m.Members.First());
    Assert.Equal("Proto", decl.Name.Identifier.GetText(src));
  }

  [Fact]
  public void Service_Header_EmptyBody_Parses()
  {
    string src = "[[m]];\nSvc :^ recv {}\n";
    var m = Parse(src);
    var decl = Assert.IsType<ServiceDeclaration>(m.Members.First());
    Assert.Equal("Svc", decl.Name.Identifier.GetText(src));
    Assert.Equal("recv", decl.Receiver.GetText(src));
  }
}

