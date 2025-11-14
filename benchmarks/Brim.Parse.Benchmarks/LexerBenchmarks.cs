using BenchmarkDotNet.Attributes;
using Brim.Core;
using Brim.Core.Collections;
using Brim.Lex;
using Brim.Parse.Producers;

namespace Brim.Parse.Benchmarks;

[MemoryDiagnoser]
public class LexerBenchmarks
{
  string _text = string.Empty;

  [Params(200, 2000)]
  public int Members { get; set; }

  [GlobalSetup]
  public void Setup() => _text = ParserBenchmarks.BuildModule(Members);

  [Benchmark]
  public int RawProducerOnly()
  {
    SourceText src = SourceText.From(_text);
    DiagnosticList diags = DiagnosticList.Create();
    LexTokenSource raw = new(src, diags);
    int count = 0;
    while (raw.TryRead(out LexToken tok))
    {
      count++;
      if (tok.TokenKind == TokenKind.Eob) break;
    }
    return count + diags.Count;
  }

  [Benchmark]
  public int SignificantProducerOnly()
  {
    SourceText src = SourceText.From(_text);
    DiagnosticList diags = DiagnosticList.Create();
    CoreTokenSource sig = new(new LexTokenSource(src, diags));
    int count = 0;
    while (sig.TryRead(out CoreToken tok))
    {
      count++;
      if (tok.TokenKind == TokenKind.Eob) break;
    }
    return count + diags.Count;
  }
}
