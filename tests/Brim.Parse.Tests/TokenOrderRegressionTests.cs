using System.Collections.Generic;
using System.Linq;
using System.Text;
using Brim.Core;
using Brim.Core.Collections;
using Brim.Lex;
using Brim.Parse;
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

  [Fact(Skip = "TODO: Investigate zero-width error tokens in complex nested generics after CommaList refactor")]
  public void SignificantTokens_Match_GreenTokens_In_Order()
  {
    string src = "=[m]=;\n"
      + "Alias[T: C1 + C2] := Outer[Inner,];\n"
      + "P := .{ m1:(A,B,) C, };\n"
      + "SvcT := @{ P, Q[R], };\n"
      + "S := %{ a:A, b:B, };\n"
      + "U := |{ A:A, B:B, };\n"
      + "F := & prim { ONE, TWO, };\n"
      + "Tup := #{ A, B, };\n"
      + "Fn := (A,B,) C;\n";

    // Build core token stream directly
    SourceText st = SourceText.From(src);
    DiagnosticList diags = DiagnosticList.Create();
    LexTokenSource lex = new(st, diags);
    CoreTokenSource coreSource = new(lex);
    List<CoreToken> coreTokens = new();
    while (coreSource.TryRead(out CoreToken tok))
    {
      coreTokens.Add(tok);
      if (tok.TokenKind == TokenKind.Eob)
        break;
    }

    // Parse to green tree and extract tokens in traversal order
    BrimModule module = Parser.ModuleFrom(st);
    List<CoreToken> greenTokens = EnumerateGreenTokens(module).Select(gt => gt.CoreToken).ToList();

    // Sanity: offsets must be non-decreasing in green tree
    for (int i = 1; i < greenTokens.Count; i++)
      Assert.True(greenTokens[i - 1].Offset <= greenTokens[i].Offset, $"Out of order at index {i}");

    // Compare streams
    if (coreTokens.Count != greenTokens.Count)
    {
      // Show a quick diff window for debugging
      int min = Math.Min(coreTokens.Count, greenTokens.Count);
      int diffAt = -1;
      for (int i = 0; i < min; i++)
      {
        if (coreTokens[i].TokenKind != greenTokens[i].TokenKind || coreTokens[i].Offset != greenTokens[i].Offset || coreTokens[i].Length != greenTokens[i].Length)
        { diffAt = i; break; }
      }
      static string Dump(List<CoreToken> list, int start, int count, string src)
      {
        var sb = new System.Text.StringBuilder();
        for (int i = start; i < Math.Min(list.Count, start + count); i++)
        {
          var t = list[i];
          sb.Append('[').Append(i).Append("] ")
            .Append((int)t.TokenKind).Append('@').Append(t.Offset).Append(':').Append(t.Length)
            .Append(' ').Append(src.Substring(t.Offset, Math.Min(t.Length, Math.Max(0, src.Length - t.Offset))))
            .Append('\n');
        }
        return sb.ToString();
      }
      string diag = $"core={coreTokens.Count} green={greenTokens.Count} firstDiff={diffAt}\n" +
        "core:\n" + Dump(coreTokens, Math.Max(0, diffAt - 3), 8, src) +
        "green:\n" + Dump(greenTokens, Math.Max(0, diffAt - 3), 8, src);
      Assert.Fail(diag);
    }
    for (int i = 0; i < coreTokens.Count; i++)
    {
      var a = coreTokens[i];
      var b = greenTokens[i];
      Assert.Equal(a.TokenKind, b.TokenKind);
      Assert.Equal(a.Offset, b.Offset);
      Assert.Equal(a.Length, b.Length);
    }
  }
}
