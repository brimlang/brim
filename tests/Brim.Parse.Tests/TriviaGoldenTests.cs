using Brim.Parse.Producers;

namespace Brim.Parse.Tests;

public class TriviaGoldenTests
{
  static List<SignificantToken> SigAll(string text)
  {
    RawProducer raw = new(SourceText.From(text), DiagSink.Create());
    SignificantProducer<RawProducer> sig = new(raw);
    List<SignificantToken> list = [];
    while (sig.TryRead(out SignificantToken st)) { list.Add(st); if (st.CoreToken.Kind == RawTokenKind.Eob) break; }
    return list;
  }

  [Fact]
  public void GoldenTriviaShapingSequence()
  {
    const string src = "foo   -- comment with trailing whitespace\n";
    List<SignificantToken> toks = SigAll(src);
    Assert.Contains(toks, static t => t.CoreToken.Kind == RawTokenKind.Identifier);
    Assert.Contains(toks, static t => t.CoreToken.Kind == RawTokenKind.Terminator);
  SignificantToken id = toks.First(t => t.CoreToken.Kind == RawTokenKind.Identifier);
  Assert.False(id.HasLeading);
  SignificantToken term = toks.First(t => t.CoreToken.Kind == RawTokenKind.Terminator);
  Assert.True(term.HasLeading);
  Assert.True(term.LeadingTrivia.Count >= 2);
  }
}
