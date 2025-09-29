using System.CommandLine;
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
      AnsiConsole.Write(GetMarkup(token, st));
      if (token.Kind == RawKind.Eob)
        break;
    }

    return 0;
  }

  static Markup GetMarkup(RawToken token, SourceText source)
  {
    string tokenText = token.Value(source.Span).ToString();
    return token.Kind switch
    {
      RawKind.Terminator => Markup.FromInterpolated($"[magenta]{{{token.Kind}}}[/] '[grey]{tokenText.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t")}[/]'\n"),
      RawKind.Identifier => Markup.FromInterpolated($"[blue]{{{token.Kind}}}[/] '[grey]{tokenText}[/]'\n"),
      RawKind.IntegerLiteral => Markup.FromInterpolated($"[green]{{{token.Kind}}}[/] '[grey]{tokenText}[/]'\n"),
      RawKind.DecimalLiteral => Markup.FromInterpolated($"[green]{{{token.Kind}}}[/] '[grey]{tokenText}[/]'\n"),
      RawKind.StringLiteral => Markup.FromInterpolated($"[green]{{{token.Kind}}}[/] '[grey]{tokenText}[/]'\n"),
      RawKind.RuneLiteral => Markup.FromInterpolated($"[green]{{{token.Kind}}}[/] '[grey]{tokenText}[/]'\n"),
      RawKind.WhitespaceTrivia => Markup.FromInterpolated($"[cyan]{{{token.Kind}}}[/] '[grey]{tokenText.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t")}[/]'\n"),
      RawKind.CommentTrivia => Markup.FromInterpolated($"[cyan]{{{token.Kind}}}[/] '[grey]{tokenText.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t")}[/]'\n"),
      RawKind.Error => Markup.FromInterpolated($"[red]{{{token.Kind}}}[/] '[grey]{tokenText}[/]'\n"),
      _ => Markup.FromInterpolated($"[yellow]{{{token.Kind}}}[/] '[white]{tokenText}[/]'\n"),
    };
  }
}
