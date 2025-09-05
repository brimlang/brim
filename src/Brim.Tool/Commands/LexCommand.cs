using System.CommandLine;
using Brim.Parse;
using Spectre.Console;

namespace Brim.Tool.Commands;

class LexCommand : Command
{
  static readonly Argument<string> _fileArgument = new("file")
  {
    Description = "Brim source file to lex",
    Arity = ArgumentArity.ExactlyOne,
  };

  public LexCommand() : base("lex")
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

    string source = File.ReadAllText(file);
    SourceText st = SourceText.From(source);
    RawTokenProducer prod = new(st);
    while (prod.TryRead(out RawToken token))
    {
      AnsiConsole.Write(token.GetDebugMarkup(source.AsSpan(), static (kind, args) => kind switch
      {
        RawToken.ErrorKind.UnexpectedChar => $"Unexpected character '{args?[0]}'",
        RawToken.ErrorKind.UnterminatedString => "Unterminated string literal",
        RawToken.ErrorKind.UnexpectedToken => "Unexpected token",
        _ => "Unknown error"
      }));
      if (token.Kind == RawTokenKind.Eof) break;
    }

    return 0;
  }
}
