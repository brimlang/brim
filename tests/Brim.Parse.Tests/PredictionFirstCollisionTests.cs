using Xunit;
using Brim.Parse;

namespace Brim.Parse.Tests;

/// <summary>
/// DEBUG-only validation of FIRST set uniqueness / collision for module tables.
/// </summary>
public class PredictionFirstCollisionTests
{
  [Fact]
  public void ModuleDirectiveFirstSetHasNoDuplicates()
  {
#if DEBUG
    HashSet<int> seen = [];
  foreach (Prediction pred in Parser.ModuleDirectivePredictions)
    {
      int k = (int)pred.Look[0];
      Assert.True(seen.Add(k), $"Duplicate first token in directives: {(RawTokenKind)k}");
    }
#endif
  }

  [Fact]
  public void ModuleMemberFirstSetHasNoDuplicates()
  {
#if DEBUG
    HashSet<int> seen = [];
  foreach (Prediction pred in Parser.ModuleMemberPredictions)
    {
      int k = (int)pred.Look[0];
      // Allow duplicates intentionally if longer look distinguishes; detect pure duplicates only.
      if (!seen.Add(k))
      {
        // Ensure the duplicate sequences are not identical beyond K1.
        // Collect all sequences sharing this K1 and assert no identical length/pattern duplicates.
        var group = Parser.ModuleMemberPredictions.Where(p => p.Look[0] == (RawTokenKind)k).ToList();
        for (int i = 0; i < group.Count; i++)
        {
          for (int j = i + 1; j < group.Count; j++)
          {
            var a = group[i].Look; var b = group[j].Look;
            if (a.Length == b.Length)
            {
              bool identical = true;
              for (int idx = 0; idx < a.Length; idx++) if (a[idx] != b[idx]) { identical = false; break; }
              Assert.False(identical, $"Duplicate prediction sequences for first token {(RawTokenKind)k}");
            }
          }
        }
      }
    }
#endif
  }
}