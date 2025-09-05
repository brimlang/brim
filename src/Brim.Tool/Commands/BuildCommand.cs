using System.CommandLine;
using Spectre.Console;

namespace Brim.Tool.Commands;

class BuildCommand : Command
{
  static readonly Argument<string> _sourceArgument = new("source")
  {
    Description = "source file or directory",
    Arity = ArgumentArity.ZeroOrOne,
    // DefaultValueFactory = static a => ".",
  };

  public BuildCommand() : base("build")
  {
    Description = "build brim sources";
    Arguments.Add(_sourceArgument);
    SetAction(Handle);
  }

  static void Handle(ParseResult parseResult)
  {
    Common.Verbosity verbosity = parseResult.GetValue(Common.VerbosityOption);
    string? source = parseResult.GetValue(_sourceArgument) ?? ".";
    AnsiConsole.MarkupLine($"[green] Building [red]{source}[/] with verbosity {verbosity}... [/]");
  }
}

