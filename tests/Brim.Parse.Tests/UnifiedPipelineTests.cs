using Brim.Parse;
using Brim.Parse.Collections;
using Brim.Parse.Green;
using Brim.Parse.Producers;

namespace Brim.Parse.Tests;

public static class PipelineCorpus
{
  public static IEnumerable<object[]> Samples()
  {
    string corpusRoot = Path.Combine(AppContext.BaseDirectory, "TestCorpus");
    if (!Directory.Exists(corpusRoot))
      yield break;

    foreach (string file in Directory.EnumerateFiles(corpusRoot, "*.brim", SearchOption.AllDirectories))
      yield return new object[] { Path.GetFileName(file), file };
  }
}

public class UnifiedPipelineTests
{
  public static IEnumerable<object[]> CorpusSamples => PipelineCorpus.Samples();

  [Theory]
  [MemberData(nameof(CorpusSamples))]
  public void Corpus_runs_clean_through_lexers_and_parser(string label, string path)
  {
    string text = File.ReadAllText(path);

    // Stage 1: raw producer
    SourceText rawSource = SourceText.From(text);
    DiagnosticList rawDiagnostics = DiagnosticList.Create();
    RawProducer raw = new(rawSource, rawDiagnostics);
    List<RawToken> rawTokens = [];
    int prevEnd = -1;
    while (raw.TryRead(out RawToken token))
    {
      rawTokens.Add(token);
      if (prevEnd >= 0)
      {
        int start = token.Offset;
        Assert.True(start >= prevEnd, $"{label}: token {token.Kind} started before previous token ended");
      }
      prevEnd = token.Offset + token.Length;
      if (token.Kind == RawKind.Eob) break;
    }

    Assert.NotEmpty(rawTokens);
    Assert.Equal(RawKind.Eob, rawTokens[^1].Kind);
    Assert.DoesNotContain(rawTokens, t => t.Kind == RawKind.Error);
    Assert.Empty(rawDiagnostics.GetSortedDiagnostics());

    // Stage 2: significant producer
    SourceText sigSource = SourceText.From(text);
    DiagnosticList sigDiagnostics = DiagnosticList.Create();
    SignificantProducer<RawProducer> significant = new(new RawProducer(sigSource, sigDiagnostics));
    List<SignificantToken> sigTokens = [];
    while (significant.TryRead(out SignificantToken token))
    {
      sigTokens.Add(token);
      if (token.CoreToken.Kind == RawKind.Eob) break;
    }

    Assert.NotEmpty(sigTokens);
    Assert.Equal(RawKind.Eob, sigTokens[^1].CoreToken.Kind);
    Assert.DoesNotContain(sigTokens, t => t.CoreToken.Kind == RawKind.Error);
    Assert.Empty(sigDiagnostics.GetSortedDiagnostics());

    // Stage 3: parser
    BrimModule module = Parser.ModuleFrom(SourceText.From(text));
    Assert.Empty(module.Diagnostics);
  }
}
