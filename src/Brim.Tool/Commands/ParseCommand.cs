using System.CommandLine;
using Brim.Parse;
using Brim.Parse.Green;
using Spectre.Console;

namespace Brim.Tool.Commands;

class ParseCommand : Command
{
  static readonly Argument<string> _fileArgument = new("file")
  {
    Description = "Brim source file to parse",
    Arity = ArgumentArity.ExactlyOne,
  };

  public ParseCommand() : base("parse")
  {
    Description = "Parse a Brim source file and print the module header";
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
    Parser parser = new(SourceText.From(source));
    BrimModule module = parser.ParseModule();

    AnsiConsole.Write(module.ModuleDirective.Format(source));
    foreach (GreenNode member in module.Members)
    {
      AnsiConsole.Write(member.Format(source));
    }

    AnsiConsole.WriteLine(module.GetText(source));

    return 0;
  }
}
