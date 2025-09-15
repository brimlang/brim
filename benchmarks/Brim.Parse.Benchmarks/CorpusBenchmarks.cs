using BenchmarkDotNet.Attributes;
using Brim.Parse.Collections;
using Brim.Parse.Producers;

namespace Brim.Parse.Benchmarks;

[MemoryDiagnoser]
public class CorpusBenchmarks
{
  string[] _files = Array.Empty<string>();
  string[] _texts = Array.Empty<string>();

  [Params(false, true)]
  public bool IncludeSynthetic { get; set; }

  [GlobalSetup]
  public void Setup()
  {
    string? root = Environment.GetEnvironmentVariable("BRIM_BENCH_CORPUS");
    if (!string.IsNullOrWhiteSpace(root) && Directory.Exists(root))
    {
      _files = Directory.GetFiles(root, "*.brim", SearchOption.AllDirectories);
    }
    else
    {
      _files = Array.Empty<string>();
    }

    List<string> texts = new();
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

    _texts = texts.ToArray();
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
      RawProducer raw = new(SourceText.From(t), diags);
      while (raw.TryRead(out RawToken tok)) { if (tok.Kind == RawKind.Eob) break; }
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
      SignificantProducer<RawProducer> sig = new(new RawProducer(SourceText.From(t), diags));
      while (sig.TryRead(out SignificantToken tok)) { if (tok.Kind == RawKind.Eob) break; }
      sum += diags.Count;
    }
    return sum;
  }
}
