using Brim.Parse.Green;

namespace Brim.Parse.Tests;

public class BufTypeParseTests
{
  static BrimModule Parse(string src) => Parser.ParseModule(src);

  [Fact]
  public void BufType_WithStarAndSize_Parses()
  {
    var src = "[[m]];\nx : buf[i32*4] = buf[i32*4]{ 1, 2, 3, 4 };";
    var m = Parse(src);
    Assert.DoesNotContain(m.Diagnostics, static d => d.Code == DiagCode.UnexpectedToken);

    // find the binding and its type
    var bind = m.Members.OfType<ValueDeclaration>().FirstOrDefault();
    Assert.NotNull(bind);
    var typeExpr = bind!.TypeNode;
    Assert.IsType<TypeExpr>(typeExpr);

    // The core should be a BufTypeExpr
    Assert.IsType<BufTypeExpr>(typeExpr.Core);
    var bufCore = (BufTypeExpr)typeExpr.Core;
    Assert.NotNull(bufCore.Star);
    Assert.NotNull(bufCore.Size);
    Assert.Equal(SyntaxKind.StarToken, bufCore.Star!.SyntaxKind);
    Assert.Equal(SyntaxKind.IntToken, bufCore.Size!.SyntaxKind);
  }
}
