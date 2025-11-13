using System.Collections.Generic;
using System.IO;
using Brim.Core;

namespace Brim.Lex.Tests;

public class CorpusLexingTests
{
  public static IEnumerable<object[]> CorpusFiles()
  {
    string corpusRoot = Path.Combine(AppContext.BaseDirectory, "TestCorpus");
    if (!Directory.Exists(corpusRoot))
      yield break;

    foreach (string file in Directory.EnumerateFiles(corpusRoot, "*.brim", SearchOption.AllDirectories))
      yield return new object[] { file };
  }

  [Theory]
  [MemberData(nameof(CorpusFiles))]
  public void CorpusSamplesLexWithoutDiagnostics(string corpusFile)
  {
    LexTestHost.LexResult result = LexTestHost.LexFile(corpusFile);

    Assert.NotEmpty(result.Tokens);
    Assert.Equal(TokenKind.Eob, result.Tokens[^1].TokenKind);
    Assert.DoesNotContain(result.Diagnostics, d => d.Severity == DiagSeverity.Error);
  }
}
