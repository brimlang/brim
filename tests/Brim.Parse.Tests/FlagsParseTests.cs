using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class FlagsParseTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void FlagsDeclarationParses()
  {
    ReadOnlySpan<char> c = "[[m]];\nPerms := &u8{ read, write, exec };";
    var m = Parse(c.ToString());
    var td = m.Members.OfType<TypeDeclaration>().FirstOrDefault();
    Assert.NotNull(td);
    Assert.Equal("Perms", td!.Name.Identifier.GetText(c));
    var fs = Assert.IsType<FlagsShape>(td.TypeNode);
    Assert.Equal(3, fs.Members.Count);
    Assert.Equal("u8", fs.UnderlyingType.GetText(c));
  }
}
