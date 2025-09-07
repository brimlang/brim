using System.CommandLine;
using Brim.Parse;
using Brim.Parse.Green;
using Brim.Parse.Producers;
using Brim.Tool.Diagnostics;
using Spectre.Console;

namespace Brim.Tool.Commands;

class ParseCommand : Command
{
  static readonly Argument<string> _fileArgument = new("file")
  {
    Description = "Brim source file to parse",
    Arity = ArgumentArity.ExactlyOne,
  };

  static readonly Option<bool> _diagnosticsOption = new("--diagnostics", "-d")
  {
    Description = "Show diagnostics",
    Arity = ArgumentArity.ZeroOrOne,
  };

  internal ParseCommand() : base("parse")
  {
    Description = "Parse a Brim source file and display the parse tree";
    Arguments.Add(_fileArgument);
    Options.Add(_diagnosticsOption);
    SetAction(Handle);
  }

  static int Handle(ParseResult parseResult)
  {
    string file = parseResult.GetValue(_fileArgument)!;
    bool showDiagnostics = parseResult.GetValue(_diagnosticsOption);
    if (!File.Exists(file))
    {
      AnsiConsole.MarkupLine($"[red]File not found:[/] {file}");
      return -1;
    }

    string source = File.ReadAllText(file);
    SourceText st = SourceText.From(source);
    DiagSink sink = DiagSink.Create();
    RawProducer raw = new(st, sink);
    SignificantProducer<RawProducer> sig = new(raw);
    LookAheadWindow<SignificantToken, SignificantProducer<RawProducer>> la = new(sig, 4);
    Parser parser = new(la, sink);
    BrimModule module = parser.ParseModule();

    AnsiConsole.MarkupLine("[bold]Parse Tree:[/]");
    AnsiConsole.Write(GreenNodeFormatter.RenderTree(source, module));

    AnsiConsole.WriteLine(module.GetText(source));

    if (module.Diagnostics.Count == 0)
    {
      AnsiConsole.MarkupLine("[green]No diagnostics.[/]");
    }
    else
    {
      AnsiConsole.MarkupLine($"[red]{module.Diagnostics.Count} diagnostics.[/]");
    }

    if (showDiagnostics)
    {
      StructuralArray<Diagnostic> diags = module.Diagnostics;
      AnsiConsole.MarkupLine("[red]Diagnostics:[/]");
      foreach (Diagnostic d in diags)
      {
        string msg = DiagnosticRenderer.Render(d);
        AnsiConsole.MarkupLineInterpolated($"[yellow]{d.Line}:{d.Column}[/] {msg}");
      }
    }

    return 0;
  }
}
