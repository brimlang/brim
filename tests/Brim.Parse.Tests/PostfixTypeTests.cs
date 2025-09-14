using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class PostfixTypeTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void Option_Postfix_Parses()
  {
    string src = "[[m]];\nOpt := str?;\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members.First());
    var opt = Assert.IsType<OptionType>(td.TypeNode);
    var innerTok = Assert.IsType<GreenToken>(opt.Inner);
    Assert.Equal(SyntaxKind.IdentifierToken, innerTok.SyntaxKind);
  }

  [Fact]
  public void Result_Postfix_Parses()
  {
    string src = "[[m]];\nRes := i32!;\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members.First());
    var res = Assert.IsType<ResultType>(td.TypeNode);
    var innerTok = Assert.IsType<GreenToken>(res.Inner);
    Assert.Equal(SyntaxKind.IdentifierToken, innerTok.SyntaxKind);
  }

  [Fact]
  public void Generic_With_Option_Postfix_Parses()
  {
    string src = "[[m]];\nAlias := Wrapper[T]?;\n";
    var m = Parse(src);
    var td = Assert.IsType<TypeDeclaration>(m.Members.First());
    var opt = Assert.IsType<OptionType>(td.TypeNode);
    _ = Assert.IsType<GenericType>(opt.Inner);
  }
}

