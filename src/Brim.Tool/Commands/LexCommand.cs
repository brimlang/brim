using System.CommandLine;
using Brim.Parse;
using Brim.Parse.Producers;
using Spectre.Console;

namespace Brim.Tool.Commands;

class LexCommand : Command
{
  static readonly Argument<string> _fileArgument = new("file")
  {
    Description = "Brim source file to lex",
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
    if (!File.Exists(file))
    {
      AnsiConsole.MarkupLine($"[red]File not found:[/] {file}");
      return -1;
    }

    DiagnosticList sink = DiagnosticList.Create();
    string source = File.ReadAllText(file);
    SourceText st = SourceText.From(source);
    RawProducer prod = new(st, sink);

    while (prod.TryRead(out RawToken token))
    {
      AnsiConsole.Write(GetMarkup(token, source));
      if (token.Kind == RawKind.Eob)
        break;
    }

    return 0;
  }

  static Markup GetMarkup(RawToken token, string source)
  {
    return token.Kind switch
    {
      RawKind.Terminator => Markup.FromInterpolated($"[magenta]{{{token.Kind}}}[/] '[grey]{token.Value(source).ToString().Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t")}[/]'\n"),
      RawKind.Identifier => Markup.FromInterpolated($"[blue]{{{token.Kind}}}[/] '[grey]{token.Value(source).ToString()}[/]'\n"),
      RawKind.NumberLiteral => Markup.FromInterpolated($"[green]{{{token.Kind}}}[/] '[grey]{token.Value(source).ToString()}[/]'\n"),
      RawKind.StringLiteral => Markup.FromInterpolated($"[green]{{{token.Kind}}}[/] '[grey]{token.Value(source).ToString()}[/]'\n"),
      RawKind.WhitespaceTrivia => Markup.FromInterpolated($"[cyan]{{{token.Kind}}}[/] '[grey]{token.Value(source).ToString().Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t")}[/]'\n"),
      RawKind.CommentTrivia => Markup.FromInterpolated($"[cyan]{{{token.Kind}}}[/] '[grey]{token.Value(source).ToString().Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t")}[/]'\n"),
      RawKind.Error => Markup.FromInterpolated($"[red]{{{token.Kind}}}[/] '[grey]{token.Value(source).ToString()}[/]'\n"),
      _ => Markup.FromInterpolated($"[yellow]{{{token.Kind}}}[/] '[white]{token.Value(source).ToString()}[/]'\n"),
    };
  }
}
