using Xunit;

namespace Brim.Parse.Tests;

public class SignificantProducerTests
{
  static List<SignificantToken> SigAll(string text)
  {
    RawTokenProducer raw = new(SourceText.From(text));
    SignificantProducer<RawTokenProducer> sig = new(raw, static (ref RawTokenProducer p, out RawToken t) => p.TryRead(out t));
    List<SignificantToken> list = [];
    while (sig.TryRead(out SignificantToken st)) { list.Add(st); if (st.Token.Kind == RawTokenKind.Eof) break; }
    return list;
  }

  [Fact]
  public void LeadingTriviaAttachedOrNotDependingOnProducerLogic()
  {
    var list = SigAll("   -- c1\nfoo");
    var id = Assert.Single(list, t => t.Token.Kind == RawTokenKind.Identifier);
    // Current implementation discards trivia before first non-terminator significant token.
    Assert.False(id.HasLeading);
  }

  [Fact]
  public void IdentifierFollowedByTerminatorKeepsTrailingTrivia()
  {
    var list = SigAll("foo   -- t\n");
    var id = Assert.Single(list, t => t.Token.Kind == RawTokenKind.Identifier);
    Assert.True(id.HasTrailing);
    Assert.True(id.TrailingTrivia.Count >= 2);
    Assert.Contains(list, t => t.Token.Kind == RawTokenKind.Terminator);
  }

  [Fact]
  public void SingleIdentifierAtEOFHasNoTrailing()
  {
    var list = SigAll("foo");
    var id = Assert.Single(list, t => t.Token.Kind == RawTokenKind.Identifier);
    Assert.False(id.HasTrailing);
  }
}
