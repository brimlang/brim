namespace Brim.Parse.Tests;

public class ParserIterationGuardTests
{
  [Fact]
  [Trait("Category", "ParseGuard")]
  public void ParserDoesNotHitIterationGuardOnValidModule()
  {
    string src = "[[acme::auth]]\n\n<< User\nUser : `%{ id :str, age :i32 }\n";
    var mod = Parser.ParseModule(src);
    Assert.NotNull(mod);
    Assert.Equal(Green.SyntaxKind.EobToken, mod.Eob.Kind);
  }
}
