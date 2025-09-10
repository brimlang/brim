using Brim.Parse.Collections;
using Brim.Parse.Producers;

namespace Brim.Parse.Tests;

public class SignificantProducerTests
{
  static List<SignificantToken> SigAll(string text)
  {
    RawProducer raw = new(SourceText.From(text), DiagnosticList.Create());
    SignificantProducer<RawProducer> sig = new(raw);
    List<SignificantToken> list = [];
    while (sig.TryRead(out SignificantToken st)) { list.Add(st); if (st.CoreToken.Kind == RawKind.Eob) break; }
    return list;
  }

  [Fact]
  public void LeadingTriviaAttachedOrNotDependingOnProducerLogic()
  {
    var list = SigAll("""
      -- c1
      foo
      """);
    var id = Assert.Single(list, static t => t.CoreToken.Kind == RawKind.Identifier);
    Assert.False(id.HasLeading);
  }

  [Fact]
  public void IdentifierFollowedByTerminatorTriviaBecomesLeadingOfTerminator()
  {
    var list = SigAll("foo   -- t\n");
    var id = Assert.Single(list, static t => t.CoreToken.Kind == RawKind.Identifier);
    // Identifier should have no leading trivia in this scenario (source starts with identifier)
    Assert.False(id.HasLeading);
    var term = Assert.Single(list, static t => t.CoreToken.Kind == RawKind.Terminator);
    Assert.True(term.HasLeading); // whitespace + comment now leading on terminator
    Assert.True(term.LeadingTrivia.Count >= 2);
  }

  [Fact]
  public void SingleIdentifierAtEOFHasNoFollowingTriviaModel()
  {
    var list = SigAll("foo");
    var id = Assert.Single(list, static t => t.CoreToken.Kind == RawKind.Identifier);
    Assert.False(id.HasLeading);
  }
}
