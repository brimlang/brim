using System;
using System.Collections.Generic;
using Brim.Core;
using Brim.Parse;
using Brim.Parse.Collections;
using Xunit;

public class PredictionTableTests
{
  [Fact]
  public void Build_BucketsPredictionsByFirstToken_PreservesOrder()
  {
    // Helper parse action that never runs; we only test grouping behavior.
    ParseAction action = (Parser p) => throw new NotSupportedException();

    Prediction p0 = new(action, new TokenSequence(TokenKind.Identifier, TokenKind.ColonEqual));
    Prediction p1 = new(action, new TokenSequence(TokenKind.Hat, TokenKind.Identifier, TokenKind.Colon));
    Prediction p2 = new(action, new TokenSequence(TokenKind.Identifier, TokenKind.Colon, TokenKind.Identifier));
    Prediction p3 = new(action, new TokenSequence(TokenKind.Identifier, TokenKind.Colon, TokenKind.Identifier));

    // Non-contiguous ordering: identifier-starting entries interleaved with others
    Prediction[] mixed = new[] { p0, p1, p2, p3 };

    var table = PredictionTable.Build(mixed);

    Assert.True(table.TryGetGroup(TokenKind.Identifier, out ReadOnlySpan<Prediction> group));

    // Collect token sequences from the returned span
    List<TokenSequence> seqs = new();
    foreach (Prediction pred in group) seqs.Add(pred.Sequence);

    // Filter those that start with Identifier
    var idSeqs = seqs.Where(s => s[0] == TokenKind.Identifier).ToArray();
    Assert.Equal(3, idSeqs.Length);

    Assert.Equal(p0.Sequence.Length, idSeqs[0].Length);
    Assert.Equal(p0.Sequence[0], idSeqs[0][0]);

    Assert.Equal(p2.Sequence.Length, idSeqs[1].Length);
    Assert.Equal(p2.Sequence[0], idSeqs[1][0]);

    Assert.Equal(p3.Sequence.Length, idSeqs[2].Length);
    Assert.Equal(p3.Sequence[0], idSeqs[2][0]);
  }
}
