using Brim.Parse;
using Spectre.Console;

namespace Brim.Tool;

static class TokenExtensions
{
  public static Markup GetDebugMarkup(this RawToken t, ReadOnlySpan<char> source, Func<RawToken.ErrorKind, object[]?, string>? formatter = null)
  {
    string slice = new string(t.Value(source))
      .Replace("\n", "\0")
      .Replace("\r", "\0")
      .Replace("[", "[[")
      .Replace("]", "]]");

    string errorMsg = string.Empty;
    if (t.Kind == RawTokenKind.Error)
    {
      errorMsg = formatter is not null
        ? $" {formatter(t.Error, t.ErrorArgs)}"
        : $" {t.Error}{(t.ErrorArgs?.Length > 0 ? $"({string.Join(", ", t.ErrorArgs)})" : string.Empty)}";
    }

    return new($"[blue]{t.Kind}[/]@[yellow]{t.Line}:{t.Column}({t.Length})[/] [grey]'{slice}'[/][red]{errorMsg}[/]\n");
  }
}
