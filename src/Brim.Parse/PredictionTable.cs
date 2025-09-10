using Brim.Parse.Collections;
using Brim.Parse.Green;

namespace Brim.Parse;

/// <summary>
/// Parsing action delegate.
/// </summary>
internal delegate GreenNode ParseAction(Parser parser);

/// <summary>
/// Ordered prediction entry. The <see cref="TokenSequence"/> is matched against the token stream; first matching entry wins.
/// </summary>
internal readonly record struct Prediction(ParseAction Action, TokenSequence Sequence);

/// <summary>
/// Read-only grouped prediction table. Provides O(1) access to subset sharing the same first token.
/// </summary>
readonly ref struct PredictionTable
{
  readonly ReadOnlySpan<int> _groupStart; // -1 if none
  readonly ReadOnlySpan<byte> _groupCount; // 0 if none

  public PredictionTable(
    ReadOnlySpan<Prediction> entries,
    ReadOnlySpan<int> starts,
    ReadOnlySpan<byte> counts)
  {
    Entries = entries;
    _groupStart = starts;
    _groupCount = counts;
  }

  public ReadOnlySpan<Prediction> Entries { get; }

  public bool TryGetGroup(RawKind kind, out ReadOnlySpan<Prediction> group)
  {
    int idx = (int)kind;
    if (idx >= _groupStart.Length || _groupStart[idx] < 0)
    {
      group = [];
      return false;
    }

    int start = _groupStart[idx];
    int count = _groupCount[idx];
    group = Entries.Slice(start, count);
    return true;
  }

  internal static PredictionTable Build(ReadOnlySpan<Prediction> preds)
  {
    if (preds.Length == 0)
      return new PredictionTable(preds, [], []);

    int maxKind = 0;
    foreach (Prediction p in preds)
    {
      int k = (int)p.Sequence[0];
      if (k > maxKind) maxKind = k;
    }

    int[] starts = new int[maxKind + 1];
    byte[] counts = new byte[maxKind + 1];
    Array.Fill(starts, -1);

    // We rely on original order; first occurrence sets start, every occurrence bumps count.
    for (int i = 0; i < preds.Length; i++)
    {
      int k = (int)preds[i].Sequence[0];
      if (starts[k] == -1)
      {
        starts[k] = i;
        counts[k] = 1;
      }
      else
      {
        counts[k]++;
      }
    }

    return new PredictionTable(preds, starts, counts);
  }
}

