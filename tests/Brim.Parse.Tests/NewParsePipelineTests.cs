using Brim.Parse.Producers;

namespace Brim.Parse.Tests;

public class NewParsePipelineTests
{
  static List<RawToken> LexAll(string text)
  {
    var src = SourceText.From(text);
  RawProducer raw = new(src, DiagSink.Create());
    List<RawToken> list = [];
    while (raw.TryRead(out RawToken t)) { list.Add(t); if (t.Kind == RawTokenKind.Eob) break; }
    return list;
  }

  static List<SignificantToken> SigAll(string text)
  {
    var src = SourceText.From(text);
  RawProducer raw = new(src, DiagSink.Create());
  SignificantProducer<RawProducer> sig = new(raw);
    List<SignificantToken> list = [];
    while (sig.TryRead(out SignificantToken st)) { list.Add(st); if (st.CoreToken.Kind == RawTokenKind.Eob) break; }
    return list;
  }

  [Fact]
  public void TerminatorReceivesLeadingTrivia()
  {
    List<SignificantToken> toks = SigAll("   ;\n"); // whitespace then terminator
    SignificantToken term = Assert.Single(toks, static t => t.CoreToken.Kind == RawTokenKind.Terminator);
    Assert.True(term.HasLeading);
    _ = Assert.Single(term.LeadingTrivia);
  }

  [Fact]
  public void Lookahead4HardFail()
  {
    var src = SourceText.From("foo bar baz qux");
  RawProducer raw = new(src, DiagSink.Create());
  SignificantProducer<RawProducer> signif = new(raw);
  LookAheadWindow<SignificantToken, SignificantProducer<RawProducer>> la = new(signif, 4);
    _ = la.Peek(3); // ok
    bool threw = false;
    try { _ = la.Peek(4); } catch (ArgumentOutOfRangeException) { threw = true; }
    Assert.True(threw);
  }

  [Fact]
  public void RawProducerLexesIdentifiersNumbersStringsAndWhitespace()
  {
    List<RawToken> list = LexAll("foo 123 \"bar\"");
    Assert.Contains(list, static t => t.Kind == RawTokenKind.Identifier);
    Assert.Contains(list, static t => t.Kind == RawTokenKind.NumberLiteral);
    Assert.Contains(list, static t => t.Kind == RawTokenKind.StringLiteral);
    Assert.Contains(list, static t => t.Kind == RawTokenKind.WhitespaceTrivia);
  }

  [Fact]
  public void RawProducerUnterminatedStringIsStringLiteral()
  {
    List<RawToken> list = LexAll("\"");
    Assert.Contains(list, static t => t.Kind == RawTokenKind.StringLiteral);
  }

  [Fact]
  public void SignificantProducerAttachesFormerTrailingAsLeadingOnNext()
  {
    var toks = SigAll("foo   -- c\n"); // identifier, whitespace+comment, terminator
    SignificantToken id = Assert.Single(toks, static t => t.CoreToken.Kind == RawTokenKind.Identifier);
  Assert.False(id.HasLeading);
    var term = Assert.Single(toks, static t => t.CoreToken.Kind == RawTokenKind.Terminator);
    Assert.True(term.HasLeading);
    Assert.True(term.LeadingTrivia.Count >= 2);
  }
}
