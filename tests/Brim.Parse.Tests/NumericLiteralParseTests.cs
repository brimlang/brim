using Brim.Parse.Green;
using Xunit;

namespace Brim.Parse.Tests;

public class NumericLiteralParseTests
{
  static IntegerLiteral ParseInt(string text)
  {
    Parser parser = ParserTestHelpers.CreateParser(text, out _);
    return IntegerLiteral.Parse(parser);
  }

  static DecimalLiteral ParseDecimal(string text)
  {
    Parser parser = ParserTestHelpers.CreateParser(text, out _);
    return DecimalLiteral.Parse(parser);
  }

  [Fact]
  public void ParsesIntegerLiteral()
  {
    IntegerLiteral lit = ParseInt("123");
    Assert.Equal(SyntaxKind.IntToken, lit.Token.SyntaxKind);
  }

  [Fact]
  public void ParsesDecimalLiteral()
  {
    DecimalLiteral lit = ParseDecimal("1.23");
    Assert.Equal(SyntaxKind.DecimalToken, lit.Token.SyntaxKind);
  }
}
