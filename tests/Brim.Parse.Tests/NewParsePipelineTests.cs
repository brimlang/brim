using Xunit;

namespace Brim.Parse.Tests;

public class NewParsePipelineTests
{
  static List<RawToken> LexAll(string text)
  {
    var src = SourceText.From(text);
    RawTokenProducer raw = new(src);
    List<RawToken> list = [];
    while (raw.TryRead(out RawToken t)) { list.Add(t); if (t.Kind == RawTokenKind.Eof) break; }
    return list;
  }

  static List<SignificantToken> SigAll(string text)
  {
    var src = SourceText.From(text);
    RawTokenProducer raw = new(src);
    SignificantProducer<RawTokenProducer> sig = new(raw, static (ref RawTokenProducer p, out RawToken t) => p.TryRead(out t));
    List<SignificantToken> list = [];
    while (sig.TryRead(out SignificantToken st)) { list.Add(st); if (st.Token.Kind == RawTokenKind.Eof) break; }
    return list;
  }

  [Fact]
  public void TerminatorReceivesLeadingTrivia()
  {
    List<SignificantToken> toks = SigAll("   ;\n"); // whitespace then terminator
    SignificantToken term = Assert.Single(toks, t => t.Token.Kind == RawTokenKind.Terminator);
    Assert.True(term.HasLeading);
    Assert.Single(term.LeadingTrivia);
  }

  [Fact]
  public void Lookahead4HardFail()
  {
    var src = SourceText.From("foo bar baz qux");
    RawTokenProducer raw = new(src);
    SignificantProducer<RawTokenProducer> signif = new(raw, static (ref RawTokenProducer p, out RawToken t) => p.TryRead(out t));
    LookAheadWindow<SignificantToken, SignificantProducer<RawTokenProducer>> la = new(signif, static (ref SignificantProducer<RawTokenProducer> s, out SignificantToken st) => s.TryRead(out st), 4);
    _ = la.Peek(3); // ok
    bool threw = false;
    try { _ = la.Peek(4); } catch (ArgumentOutOfRangeException) { threw = true; }
    Assert.True(threw);
  }

  [Fact]
  public void RawTokenProducer_LexesIdentifiersNumbersStringsAndWhitespace()
  {
    List<RawToken> list = LexAll("foo 123 \"bar\"");
    Assert.Contains(list, t => t.Kind == RawTokenKind.Identifier);
    Assert.Contains(list, t => t.Kind == RawTokenKind.NumberLiteral);
    Assert.Contains(list, t => t.Kind == RawTokenKind.StringLiteral);
    Assert.Contains(list, t => t.Kind == RawTokenKind.WhitespaceTrivia);
  }

  [Fact]
  public void RawTokenProducer_UnterminatedStringIsStringLiteral()
  {
    List<RawToken> list = LexAll("\"");
    Assert.Contains(list, t => t.Kind == RawTokenKind.StringLiteral);
  }

  [Fact]
  public void SignificantProducer_AttachesTrailingTriviaToPrecedingToken()
  {
    var toks = SigAll("foo   -- c\n"); // identifier, whitespace+comment, terminator
    SignificantToken id = Assert.Single(toks, t => t.Token.Kind == RawTokenKind.Identifier);
    Assert.True(id.HasTrailing); // whitespace + comment
    Assert.True(id.TrailingTrivia.Count >= 2);
  }
}
