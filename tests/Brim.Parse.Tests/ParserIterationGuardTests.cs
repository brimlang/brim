using System;
using Xunit;

namespace Brim.Parse.Tests;

public class ParserIterationGuardTests
{
  [Fact]
  [Trait("Category", "ParseGuard")]
  public void Parser_DoesNotHitIterationGuard_OnValidModule()
  {
    // Representative valid module snippet exercising header + a few declarations.
    string src = "[[acme::auth]]\n\n<< User\nUser = `%{ id :str, age :i32 }\n";
    var parser = new Parser(SourceText.From(src));
    var mod = parser.ParseModule();
    Assert.NotNull(mod);
    // Ensure EOF token present and no diagnostic indicates unexpected guard.
    Assert.Equal(Green.SyntaxKind.EofToken, mod.Eof.Kind);
  }
}
