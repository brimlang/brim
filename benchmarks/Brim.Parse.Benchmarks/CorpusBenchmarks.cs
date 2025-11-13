using BenchmarkDotNet.Attributes;
using Brim.Core;
using Brim.Core.Collections;
using Brim.Parse.Collections;
using Brim.Parse.Producers;

namespace Brim.Parse.Benchmarks;

[MemoryDiagnoser]
public class CorpusBenchmarks
{
  string[] _files = [];
  string[] _texts = [];

  [Params(false, true)]
  public bool IncludeSynthetic { get; set; }

  [GlobalSetup]
  public void Setup()
  {
    string? root = Environment.GetEnvironmentVariable("BRIM_BENCH_CORPUS");
    _files = !string.IsNullOrWhiteSpace(root) && Directory.Exists(root)
      ? Directory.GetFiles(root, "*.brim", SearchOption.AllDirectories)
      : [];

    List<string> texts = [];
    foreach (string f in _files)
    {
      try { texts.Add(File.ReadAllText(f)); }
      catch { /* skip unreadable */ }
    }

    if (IncludeSynthetic || texts.Count == 0)
    {
      texts.Add(ParserBenchmarks.BuildModule(50));
      texts.Add(ParserBenchmarks.BuildModule(200));
    }

    _texts = [.. texts];
  }

  [Benchmark]
  public int Parse_Corpus()
  {
    int sum = 0;
    foreach (string t in _texts)
    {
      Green.BrimModule mod = Parser.ModuleFrom(SourceText.From(t));
      sum += mod.Diagnostics.Count + mod.Members.Count;
    }
    return sum;
  }

  [Benchmark]
  public int LexRaw_Corpus()
  {
    int sum = 0;
    foreach (string t in _texts)
    {
      DiagnosticList diags = DiagnosticList.Create();
      LexSource raw = new(SourceText.From(t), diags);
      while (raw.TryRead(out LexToken tok)) { if (tok.Kind == TokenKind.Eob) break; }
      sum += diags.Count;
    }
    return sum;
  }

  [Benchmark]
  public int LexSignificant_Corpus()
  {
    int sum = 0;
    foreach (string t in _texts)
    {
      DiagnosticList diags = DiagnosticList.Create();
      SignificantProducer<LexSource> sig = new(new RawProducer(SourceText.From(t), diags));
      while (sig.TryRead(out SignificantToken tok)) { if (tok.Kind == TokenKind.Eob) break; }
      sum += diags.Count;
    }
    return sum;
  }
}
