using Brim.Parse.Producers;

namespace Brim.Parse.Tests;

public class SignificantProducerTests
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
  public void LeadingTriviaAttachedOrNotDependingOnProducerLogic()
  {
    var list = SigAll("""
      -- c1
      foo
      """);
    var id = Assert.Single(list, static t => t.CoreToken.Kind == RawTokenKind.Identifier);
    Assert.False(id.HasLeading);
  }

  [Fact]
  public void IdentifierFollowedByTerminatorKeepsTrailingTrivia()
  {
    var list = SigAll("foo   -- t\n");
    var id = Assert.Single(list, static t => t.CoreToken.Kind == RawTokenKind.Identifier);
    Assert.True(id.HasTrailing);
    Assert.True(id.TrailingTrivia.Count >= 2);
    Assert.Contains(list, static t => t.CoreToken.Kind == RawTokenKind.Terminator);
  }

  [Fact]
  public void SingleIdentifierAtEOFHasNoTrailing()
  {
    var list = SigAll("foo");
    var id = Assert.Single(list, static t => t.CoreToken.Kind == RawTokenKind.Identifier);
    Assert.False(id.HasTrailing);
  }
}
