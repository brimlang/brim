using Brim.Parse.Collections;
using Brim.Parse.Green;
using Brim.Parse.Producers;

namespace Brim.Parse.Tests;

public class NumericLiteralParseTests
{
  static IntegerLiteral ParseInt(string text)
  {
    SourceText st = SourceText.From(text);
    DiagnosticList diags = DiagnosticList.Create();
    RawProducer raw = new(st, diags);
    SignificantProducer<RawProducer> sig = new(raw);
    RingBuffer<SignificantToken, SignificantProducer<RawProducer>> la = new(sig, 4);
    Parser p = new(la, diags);
    return IntegerLiteral.Parse(p);
  }

  static DecimalLiteral ParseDecimal(string text)
  {
    SourceText st = SourceText.From(text);
    DiagnosticList diags = DiagnosticList.Create();
    RawProducer raw = new(st, diags);
    SignificantProducer<RawProducer> sig = new(raw);
    RingBuffer<SignificantToken, SignificantProducer<RawProducer>> la = new(sig, 4);
    Parser p = new(la, diags);
    return DecimalLiteral.Parse(p);
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

