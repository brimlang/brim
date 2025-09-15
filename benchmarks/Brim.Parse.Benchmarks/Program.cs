using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace Brim.Parse.Benchmarks;

public static class Program
{
  public static void Main(string[] args)
  {
    string artifacts = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "benchmarks"));
    Directory.CreateDirectory(artifacts);

    IConfig config = ManualConfig.Create(DefaultConfig.Instance)
      .WithArtifactsPath(artifacts)
      .AddExporter(MarkdownExporter.GitHub, HtmlExporter.Default, CsvExporter.Default)
      .AddJob(Job.ShortRun);
    BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
  }
}
