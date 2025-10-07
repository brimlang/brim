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
    var te = Assert.IsType<TypeExpr>(td.TypeNode);
    var ps = Assert.IsType<ProtocolShape>(te.Core);
    Assert.Empty(ps.MethodList.Elements);
  }

  // TODO: Re-enable after ServiceDeclaration is properly imported
  // [Fact]
  void Service_Header_EmptyBody_Parses()
  {
    // string src = "[[m]];\nSvc :@ recv {}\n";
    // var m = Parse(src);
    // var decl = Assert.IsType<ServiceDeclaration>(m.Members.First());
    // Assert.Equal("Svc", decl.Name.Identifier.GetText(src));
    // Assert.Equal("recv", decl.Receiver.GetText(src));
  }

  [Fact]
  public void Protocol_With_Methods_Parses()
  {
    string src = "[[m]];\nP := .{ m1:(A) B, m2:(A,B) C, };\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members.First());
    var te = Assert.IsType<TypeExpr>(td.TypeNode);
    var ps = Assert.IsType<ProtocolShape>(te.Core);
    Assert.Equal(2, ps.MethodList.Elements.Count);
    Assert.Equal("m1", ps.MethodList.Elements[0].Node.Name.Identifier.GetText(src));
  }

  // TODO: Re-enable after ServiceDeclaration is properly imported
  // [Fact]
  void Service_With_Implements_List_Parses()
  {
    // string src = "[[m]];\nSvc :@ recv {} : P + Q[R]\n";
    // var m = Parse(src);
    // var decl = Assert.IsType<ServiceDeclaration>(m.Members.First());
    // Assert.Equal(2, decl.Implements.Count);
    // Assert.NotNull(decl.Implements[0].TrailingPlus);
    // Assert.Null(decl.Implements[1].TrailingPlus);
  }
}
