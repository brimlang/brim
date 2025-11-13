using System.CommandLine;
using System.Text;
using Brim.Core;
using Brim.Core.Collections;
using Brim.Lex;
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

    DiagnosticList diags = DiagnosticList.Create();
    LexTokenSource prod = new(st, diags);

    while (prod.TryRead(out LexToken token))
    {
      WriteToken(token, st);
      if (token.TokenKind == TokenKind.Eob)
        break;
    }

    return 0;
  }

  static void WriteToken(LexToken token, SourceText source)
  {
    string literal = EscapeTokenValue(token.Chars(source.Span));
    string color = GetKindStyle(token.TokenKind);

    AnsiConsole.MarkupLine(
      $"[{color}]{PadKind(token.TokenKind)}[/] [grey]{FormatLocation(token)}[/] '[white]{literal}[/]'");
  }

  static string PadKind(TokenKind kind) => Markup.Escape(kind.ToString().PadRight(18));

  static string FormatLocation(LexToken token)
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

  static string GetKindStyle(TokenKind kind)
  {
    if (kind == TokenKind.Error)
      return "red";

    return kind switch
    {
      TokenKind.Terminator => "magenta",
      TokenKind.Identifier => "deepskyblue3",
      TokenKind.CommentTrivia or TokenKind.WhitespaceTrivia => "cyan",
      TokenKind.IntegerLiteral or TokenKind.DecimalLiteral or TokenKind.StringLiteral or TokenKind.RuneLiteral => "green",
      TokenKind.Eob => "grey50",
      > TokenKind.Unitialized and < TokenKind._SentinelGlyphs => "mediumpurple1",
      > TokenKind._SentinelGlyphs and < TokenKind._SentinelLiteral => "darkorange",
      > TokenKind._SentinelLiteral and < TokenKind._SentinelSynthetic => "cyan",
      > TokenKind._SentinelSynthetic => "grey66",
      _ => "yellow",
    };
  }
}
