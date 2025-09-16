using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class ProtocolServiceHeaderTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void Protocol_Header_EmptyBody_Parses()
  {
    string src = "[[m]];\nProto := .{};\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members.First());
    var ps = Assert.IsType<ProtocolShape>(td.TypeNode);
    Assert.Empty(ps.Methods);
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

  [Fact]
  public void Protocol_With_Methods_Parses()
  {
    string src = "[[m]];\nP := .{ m1:(A) B, m2:(A,B) C, };\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members.First());
    var ps = Assert.IsType<ProtocolShape>(td.TypeNode);
    Assert.Equal(2, ps.Methods.Count);
    Assert.Equal("m1", ps.Methods[0].Name.Identifier.GetText(src));
  }

  [Fact]
  public void Service_With_Implements_List_Parses()
  {
    string src = "[[m]];\nSvc :^ recv {} : P + Q[R]\n";
    var m = Parse(src);
    var decl = Assert.IsType<ServiceDeclaration>(m.Members.First());
    Assert.Equal(2, decl.Implements.Count);
    Assert.NotNull(decl.Implements[0].TrailingPlus);
    Assert.Null(decl.Implements[1].TrailingPlus);
  }
}
