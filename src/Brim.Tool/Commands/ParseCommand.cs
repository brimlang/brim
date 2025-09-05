using System.CommandLine;
using Brim.Parse;
using Brim.Parse.Green;
using Brim.Parse.Producers;
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
    SourceText st = SourceText.From(source);
    DiagSink sink = DiagSink.Create();
    RawProducer raw = new(st, sink);
    SignificantProducer<RawProducer> sig = new(raw);
    LookAheadWindow<SignificantToken, SignificantProducer<RawProducer>> la = new(sig, 4);
    Parser parser = new(la, sink);
    BrimModule module = parser.ParseModule();

    GreenNodeFormatter formatter = new(RenderFlags.Default);
    AnsiConsole.Write(formatter.Render(source, module.ModuleDirective));
    foreach (GreenNode member in module.Members)
      AnsiConsole.Write(formatter.Render(source, member));

    AnsiConsole.WriteLine(module.GetText(source));

    IReadOnlyList<Diagnostic> diags = parser.Diagnostics;
    if (diags.Count > 0)
    {
      AnsiConsole.MarkupLine("[red]Diagnostics:[/]");
      foreach (Diagnostic d in diags)
      {
        string msg = DiagRenderer.Render(d);
        AnsiConsole.MarkupLine($"[yellow]{d.Line}:{d.Column}[/] {msg}");
      }
    }

    return 0;
  }
}
