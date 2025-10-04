using System.CommandLine;
using System.Text;
using Brim.Parse;
using Brim.Parse.Collections;
using Brim.Parse.Producers;
using Spectre.Console;

namespace Brim.Tool.Commands;

class LexCommand : Command
{
  static readonly Argument<string> _fileArgument = new("file")
  {
    Description = "Brim source file to lex (use '-' for stdin)",
    Arity = ArgumentArity.ExactlyOne,
  };

  internal LexCommand() : base("lex")
  {
    Description = "Lex a Brim source file and print tokens";
    Arguments.Add(_fileArgument);
    SetAction(Handle);
  }

  static int Handle(ParseResult parseResult)
  {
    string file = parseResult.GetValue(_fileArgument)!;

    SourceText st;
    if (file == "-")
    {
      // Read from stdin
      string input = Console.In.ReadToEnd();
      st = SourceText.From(input);
    }
    else
    {
      // Read from file
      if (!File.Exists(file))
      {
        AnsiConsole.MarkupLine($"[red]File not found:[/] {file}");
        return -1;
      }
      st = SourceText.FromFile(file);
    }

    DiagnosticList sink = DiagnosticList.Create();
    RawProducer prod = new(st, sink);

    while (prod.TryRead(out RawToken token))
    {
      WriteToken(token, st);
      if (token.Kind == RawKind.Eob)
        break;
    }

    return 0;
  }

  static void WriteToken(RawToken token, SourceText source)
  {
    string literal = EscapeTokenValue(token.Value(source.Span));
    string color = GetKindStyle(token.Kind);

    AnsiConsole.MarkupLine(
      $"[{color}]{PadKind(token.Kind)}[/] [grey]{FormatLocation(token)}[/] '[white]{literal}[/]'");
  }

  static string PadKind(RawKind kind) => Markup.Escape(kind.ToString().PadRight(18));

  static string FormatLocation(RawToken token)
  {
    string line = token.Line.ToString().PadLeft(3);
    string column = token.Column.ToString().PadLeft(3);
    string width = token.Length.ToString().PadLeft(3);
    return $"@{line}:{column}({width})";
  }

  static string EscapeTokenValue(ReadOnlySpan<char> span)
  {
    if (span.IsEmpty)
      return string.Empty;

    StringBuilder builder = new(span.Length);

    foreach (char ch in span)
    {
      _ = ch switch
      {
        '\n' => builder.Append("\\n"),
        '\r' => builder.Append("\\r"),
        '\t' => builder.Append("\\t"),
        '\0' => builder.Append("\\0"),
        _ => builder.Append(ch),
      };
    }

    return Markup.Escape(builder.ToString());
  }

  static string GetKindStyle(RawKind kind)
  {
    if (kind == RawKind.Error)
      return "red";

    return kind switch
    {
      RawKind.Terminator => "magenta",
      RawKind.Identifier => "deepskyblue3",
      RawKind.CommentTrivia or RawKind.WhitespaceTrivia => "cyan",
      RawKind.IntegerLiteral or RawKind.DecimalLiteral or RawKind.StringLiteral or RawKind.RuneLiteral => "green",
      RawKind.Eob => "grey50",
      > RawKind._SentinelKeyword and < RawKind._SentinelGlyphs => "mediumpurple1",
      > RawKind._SentinelGlyphs and < RawKind._SentinelLiteral => "darkorange",
      >= RawKind._SentinelLiteral and < RawKind._SentinelTrivia => "green",
      >= RawKind._SentinelTrivia and < RawKind._SentinelSynthetic => "cyan",
      >= RawKind._SentinelSynthetic => "grey66",
      _ => "yellow",
    };
  }
}
