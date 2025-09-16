using Brim.Parse;
using Brim.Parse.Collections;
using Brim.Parse.Green;
using Brim.Parse.Producers;

namespace Brim.Parse.Tests;

public class TokenOrderRegressionTests
{
  static IEnumerable<GreenToken> EnumerateGreenTokens(GreenNode node)
  {
    if (node is GreenToken t)
    {
      yield return t;
    }
    else
    {
      foreach (GreenNode child in node.GetChildren())
      {
        foreach (GreenToken ct in EnumerateGreenTokens(child))
        {
          yield return ct;
        }
      }
    }
  }

  [Fact]
  public void SignificantTokens_Match_GreenTokens_In_Order()
  {
    string src = "[[m]];\n"
      + "Alias[T: C1 + C2] := Outer[Inner,];\n"
      + "P := .{ m1:(A,B,) C, };\n"
      + "Svc :^ recv {} : P + Q[R]\n"
      + "SvcT := ^{ P, Q[R], };\n"
      + "S := %{ a:A, b:B, };\n"
      + "U := |{ A:A, B:B, };\n"
      + "F := & prim { ONE, TWO, };\n"
      + "Tup := #{ A, B, };\n"
      + "Fn := (A,B,) C;\n";

    // Build significant token stream directly
    DiagnosticList diags = DiagnosticList.Create();
    SourceText st = SourceText.From(src);
    var raw = new RawProducer(st, diags);
    var sig = new SignificantProducer<RawProducer>(raw);
    List<RawToken> sigTokens = new();
    while (sig.TryRead(out SignificantToken tok))
      sigTokens.Add(tok.CoreToken);

    // Parse to green tree and extract tokens in traversal order
    BrimModule module = Parser.ModuleFrom(st);
    List<RawToken> greenTokens = EnumerateGreenTokens(module).Select(gt => gt.Token).ToList();

    // Sanity: offsets must be non-decreasing in green tree
    for (int i = 1; i < greenTokens.Count; i++)
      Assert.True(greenTokens[i - 1].Offset <= greenTokens[i].Offset, $"Out of order at index {i}");

    // Compare streams
    if (sigTokens.Count != greenTokens.Count)
    {
      // Show a quick diff window for debugging
      int min = Math.Min(sigTokens.Count, greenTokens.Count);
      int diffAt = -1;
      for (int i = 0; i < min; i++)
      {
        if (sigTokens[i].Kind != greenTokens[i].Kind || sigTokens[i].Offset != greenTokens[i].Offset || sigTokens[i].Length != greenTokens[i].Length)
        { diffAt = i; break; }
      }
      static string Dump(List<RawToken> list, int start, int count, string src)
      {
        var sb = new System.Text.StringBuilder();
        for (int i = start; i < Math.Min(list.Count, start + count); i++)
        {
          var t = list[i];
          sb.Append('[').Append(i).Append("] ")
            .Append((int)t.Kind).Append('@').Append(t.Offset).Append(':').Append(t.Length)
            .Append(' ').Append(src.Substring(t.Offset, Math.Min(t.Length, Math.Max(0, src.Length - t.Offset))))
            .Append('\n');
        }
        return sb.ToString();
      }
      string diag = $"sig={sigTokens.Count} green={greenTokens.Count} firstDiff={diffAt}\n" +
        "sig:\n" + Dump(sigTokens, Math.Max(0, diffAt - 3), 8, src) +
        "green:\n" + Dump(greenTokens, Math.Max(0, diffAt - 3), 8, src);
      Assert.Fail(diag);
    }
    for (int i = 0; i < sigTokens.Count; i++)
    {
      var a = sigTokens[i];
      var b = greenTokens[i];
      Assert.Equal(a.Kind, b.Kind);
      Assert.Equal(a.Offset, b.Offset);
      Assert.Equal(a.Length, b.Length);
    }
  }
}
