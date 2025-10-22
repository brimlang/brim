using Brim.Parse;
using Brim.Parse.Green;
using Xunit;

namespace Brim.Parse.Tests;

static class ParserTestHelpers
{
  public static BrimModule ParseModule(string source)
    => Parser.ParseModule(source);

  public static TMember GetMember<TMember>(BrimModule module, int index)
    where TMember : GreenNode
    => Assert.IsType<TMember>(module.Members[index]);
}
