using Brim.Core;
using Brim.Parse.Collections;
using Brim.Parse.Producers;

namespace Brim.Parse.Tests;

public class NewParsePipelineTests
{
  static List<LexToken> LexAll(string text)
  {
    var src = SourceText.From(text);
    LexSource raw = new(src, DiagnosticList.Create());
    List<LexToken> list = [];
    while (raw.TryRead(out LexToken t)) { list.Add(t); if (t.Kind == TokenKind.Eob) break; }
    return list;
  }

  static List<SignificantToken> SigAll(string text)
  {
    var src = SourceText.From(text);
    LexSource raw = new(src, DiagnosticList.Create());
    SignificantProducer<LexSource> sig = new(raw);
    List<SignificantToken> list = [];
    while (sig.TryRead(out SignificantToken st)) { list.Add(st); if (st.CoreToken.Kind == TokenKind.Eob) break; }
    return list;
  }

  [Fact]
  public void TerminatorReceivesLeadingTrivia()
  {
    List<SignificantToken> toks = SigAll("   ;\n"); // whitespace then terminator
    SignificantToken term = Assert.Single(toks, static t => t.CoreToken.Kind == TokenKind.Terminator);
    Assert.True(term.HasLeading);
    _ = Assert.Single(term.LeadingTrivia);
  }

  [Fact]
  public void Lookahead4HardFail()
  {
    var src = SourceText.From("foo bar baz quux");
    LexSource raw = new(src, DiagnosticList.Create());
    SignificantProducer<LexSource> signif = new(raw);
    RingBuffer<SignificantToken, SignificantProducer<LexSource>> la = new(signif, 4);
    _ = la.Peek(3); // ok
    bool threw = false;
    try { _ = la.Peek(4); } catch (ArgumentOutOfRangeException) { threw = true; }
    Assert.True(threw);
  }

  [Fact]
  public void RawProducerLexesIdentifiersNumbersStringsAndWhitespace()
  {
    List<LexToken> list = LexAll("foo 123 1.23 \"bar\"");
    Assert.Contains(list, static t => t.Kind == TokenKind.Identifier);
    Assert.Contains(list, static t => t.Kind == TokenKind.IntegerLiteral);
    Assert.Contains(list, static t => t.Kind == TokenKind.DecimalLiteral);
    Assert.Contains(list, static t => t.Kind == TokenKind.StringLiteral);
    Assert.Contains(list, static t => t.Kind == TokenKind.WhitespaceTrivia);
  }

  [Fact]
  public void RawProducerUnterminatedStringIsStringLiteral()
  {
    List<LexToken> list = LexAll("\"");
    Assert.Contains(list, static t => t.Kind == TokenKind.StringLiteral);
  }

  [Fact]
  public void SignificantProducerAttachesFormerTrailingAsLeadingOnNext()
  {
    var toks = SigAll("foo   -- c\n"); // identifier, whitespace+comment, terminator
    SignificantToken id = Assert.Single(toks, static t => t.CoreToken.Kind == TokenKind.Identifier);
    Assert.False(id.HasLeading);
    var term = Assert.Single(toks, static t => t.CoreToken.Kind == TokenKind.Terminator);
    Assert.True(term.HasLeading);
    Assert.True(term.LeadingTrivia.Count >= 2);
  }
}
