using Brim.Core;

namespace Brim.Parse.Tests;

/// <summary>
/// DEBUG-only validation of FIRST set uniqueness / collision for module tables.
/// </summary>
public class PredictionFirstCollisionTests
{
  [Fact]
  public void ModuleMemberFirstSetHasNoDuplicates()
  {
    HashSet<int> seen = [];
    foreach (Prediction pred in Parser.ModuleMemberPredictions)
    {
      int k = (int)pred.Sequence[0];
      // Allow duplicates intentionally if longer look distinguishes; detect pure duplicates only.
      if (!seen.Add(k))
      {
        // Ensure the duplicate sequences are not identical beyond K1.
        // Collect all sequences sharing this K1 and assert no identical length/pattern duplicates.
        var group = Parser.ModuleMemberPredictions.Where(p => p.Sequence[0] == (TokenKind)k).ToList();
        for (int i = 0; i < group.Count; i++)
        {
          for (int j = i + 1; j < group.Count; j++)
          {
            var a = group[i].Sequence; var b = group[j].Sequence;
            if (a.Length == b.Length)
            {
              bool identical = true;
              for (int idx = 0; idx < a.Length; idx++) if (a[idx] != b[idx]) { identical = false; break; }
              if (identical)
              {
                var sbldrA = new System.Text.StringBuilder();
                var sbldrB = new System.Text.StringBuilder();
                for (int ii = 0; ii < a.Length; ii++)
                {
                  if (ii > 0) sbldrA.Append(',');
                  sbldrA.Append(((int)a[ii]).ToString());
                }
                for (int ii = 0; ii < b.Length; ii++)
                {
                  if (ii > 0) sbldrB.Append(',');
                  sbldrB.Append(((int)b[ii]).ToString());
                }
                string sa = sbldrA.ToString();
                string sb = sbldrB.ToString();
                Assert.False(identical, $"Duplicate prediction sequences for first token {(TokenKind)k}: a=[{sa}] b=[{sb}]");
              }
            }
          }
        }
      }
    }
  }
}
