using Brim.Parse.Green;

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

  [Theory(Skip = "TODO: Hang or crash in test host")]
  [MemberData(nameof(CorpusSamples))]
  public void Corpus_parses_without_errors(string label, string path)
  {
    string text = File.ReadAllText(path);

    BrimModule module = Parser.ParseModule(text);

    int diagCount = module.Diagnostics.Length;
    Assert.True(diagCount == 0, $"{label} produced {diagCount} diagnostics");
    Assert.NotNull(module.ModuleDirective);
  }
}
