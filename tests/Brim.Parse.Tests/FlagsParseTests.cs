using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class FlagsParseTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void FlagsDeclarationParses()
  {
    ReadOnlySpan<char> c = "[[m]];\nPerms : &u8{ read, write, exec };";
    var m = Parse(c.ToString());
    FlagsDeclaration? fd = m.Members.OfType<FlagsDeclaration>().FirstOrDefault();
    Assert.NotNull(fd);
    Assert.Equal("Perms", fd!.Name.Identifier.GetText(c));
    Assert.Equal(3, fd.Members.Count);
    Assert.Equal("u8", fd.UnderlyingType.GetText(c));
  }
}
