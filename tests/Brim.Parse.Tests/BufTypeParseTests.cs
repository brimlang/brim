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

    // The core should be a TypeRef with an identifier name and generic args
    Assert.IsType<TypeRef>(typeExpr.Core);
    var tref = (TypeRef)typeExpr.Core;
    Assert.Equal(SyntaxKind.IdentifierToken, tref.Name.SyntaxKind);
    Assert.NotNull(tref.GenericArgs);
    Assert.True(tref.GenericArgs!.ArgumentList.Elements.Count > 0);
    // Basic sanity: first generic argument should be a GenericArgument wrapping a TypeExpr
    var firstElement = tref.GenericArgs.ArgumentList.Elements[0];
    Assert.IsType<GenericArgument>(firstElement.Node);
    Assert.IsType<TypeExpr>(((GenericArgument)firstElement.Node).TypeNode);

  }
}
