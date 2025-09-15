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
/// Read-only grouped prediction table, backed by arrays for caching.
/// Provides O(1) access to subset sharing the same first token.
/// </summary>
internal readonly struct PredictionTable
{
  readonly Prediction[] _entries;
  readonly int[] _groupStart; // -1 if none
  readonly byte[] _groupCount; // 0 if none

  private PredictionTable(Prediction[] entries, int[] starts, byte[] counts)
  {
    _entries = entries;
    _groupStart = starts;
    _groupCount = counts;
  }

  public ReadOnlySpan<Prediction> Entries => _entries;

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
    group = _entries.AsSpan(start, count);
    return true;
  }

  internal static PredictionTable Build(Prediction[] preds)
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

  internal static PredictionTable Build(ReadOnlySpan<Prediction> preds)
  {
    // Fallback that copies when only a span is available.
    Prediction[] arr = preds.Length == 0 ? [] : preds.ToArray();
    return Build(arr);
  }
}
