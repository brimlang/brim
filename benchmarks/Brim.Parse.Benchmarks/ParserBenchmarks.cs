using System.Text;
using BenchmarkDotNet.Attributes;
using Brim.Core;
using Brim.Core.Collections;
using Brim.Lex;
using Brim.Parse.Producers;

namespace Brim.Parse.Benchmarks;

[MemoryDiagnoser]
public class ParserBenchmarks
{
  string _small = string.Empty;
  string _medium = string.Empty;
  string _large = string.Empty;

  [GlobalSetup]
  public void Setup()
  {
    _small = BuildModule(2);
    _medium = BuildModule(20);
    _large = BuildModule(200);
  }

  internal static string BuildModule(int memberCount)
  {
    StringBuilder sb = new();
    sb.AppendLine("[[ core:demo ]]\n");
    for (int i = 0; i < memberCount; i++)
    {
      int idx = i % 6;
      switch (idx)
      {
        case 0:
          sb.AppendLine($"T{i} := ^{{ core:svc, recv: i32 }}");
          break;
        case 1:
          sb.AppendLine($"Alias{i} := T{Math.Max(0, i - 1)}");
          break;
        case 2:
          sb.AppendLine($"U{i} := |{{ A: i32, B: str }}");
          break;
        case 3:
          sb.AppendLine($"S{i} := %{{ X: i32, Y: i32, Z: i32 }}");
          break;
        case 4:
          sb.AppendLine($"N{i} := #{{ x: i32, y: i32 }}");
          break;
        default:
          sb.AppendLine($"F{i} := (i32, i32) i32");
          break;
      }
      sb.AppendLine(";");
    }
    return sb.ToString();
  }

  [Benchmark]
  public Green.BrimModule Parse_Small() => Parser.ModuleFrom(SourceText.From(_small));

  [Benchmark]
  public Green.BrimModule Parse_Medium() => Parser.ModuleFrom(SourceText.From(_medium));

  [Benchmark]
  public Green.BrimModule Parse_Large() => Parser.ModuleFrom(SourceText.From(_large));

  [Benchmark]
  public int Mixed_LexThenParse_Medium()
  {
    // Simulate a pipeline that lexes significant tokens (discarded), then parses fresh.
    DiagnosticList diags = DiagnosticList.Create();
    SourceText src = SourceText.From(_medium);
    CoreTokenSource sig = new(new LexTokenSource(src, diags));
    int count = 0;
    while (sig.TryRead(out CoreToken tok)) { count++; if (tok.TokenKind == TokenKind.Eob) break; }
    Green.BrimModule mod = Parser.ModuleFrom(src);
    return count + mod.Diagnostics.Count;
  }
}
