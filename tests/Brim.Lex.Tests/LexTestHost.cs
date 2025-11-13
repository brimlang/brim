using Brim.Core;
using Brim.Core.Collections;

namespace Brim.Lex.Tests;

internal static class LexTestHost
{
  public static LexResult Lex(string text) => Lex(SourceText.From(text));

  public static LexResult LexFile(string path) => Lex(SourceText.FromFile(path));

  static LexResult Lex(SourceText source)
  {
    DiagnosticList diagnostics = DiagnosticList.Create();
    LexTokenSource lexer = new(source, diagnostics);

    List<LexToken> tokens = [];
    while (lexer.TryRead(out LexToken token))
      tokens.Add(token);

    ImmutableArray<LexToken> immutableTokens = [.. tokens];
    ImmutableArray<Diagnostic> immutableDiagnostics = diagnostics.GetSortedDiagnostics();
    return new LexResult(source, immutableTokens, immutableDiagnostics);
  }

  internal readonly record struct LexResult(SourceText Source, ImmutableArray<LexToken> Tokens, ImmutableArray<Diagnostic> Diagnostics)
  {
    public string Slice(LexToken token) => token.Length == 0
      ? string.Empty
      : new string(Source.Span.Slice(token.Offset, token.Length));
  }
}
