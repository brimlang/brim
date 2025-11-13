using System.CommandLine;
using System.Text;
using Brim.Core;
using Brim.Core.Collections;
using Brim.Parse;
using Brim.Parse.Green;
using Brim.Tool.Diagnostics;
using Spectre.Console;

namespace Brim.Tool.Commands;

class ParseCommand : Command
{
  static readonly Argument<string> _fileArgument = new("file")
  {
    Description = "Brim source file to parse (use '-' for stdin)",
    Arity = ArgumentArity.ExactlyOne,
  };

  static readonly Option<bool> _diagnosticsOption = new("--diagnostics", "-d")
  {
    Description = "Show diagnostics",
    Arity = ArgumentArity.ZeroOrOne,
  };

  static readonly Option<bool> _showSourceOption = new("--source", "-s")
  {
    Description = "Display labeled source with line/column character guides",
    Arity = ArgumentArity.ZeroOrOne,
  };

  internal ParseCommand() : base("parse")
  {
    Description = "Parse a Brim source file and display the parse tree";
    Arguments.Add(_fileArgument);
    Options.Add(_diagnosticsOption);
    Options.Add(_showSourceOption);
    SetAction(Handle);
  }

  static int Handle(ParseResult parseResult)
  {
    string file = parseResult.GetValue(_fileArgument)!;
    bool showDiagnostics = parseResult.GetValue(_diagnosticsOption);
    bool showSource = parseResult.GetValue(_showSourceOption);

    string source;
    if (file == "-")
    {
      // Read from stdin
      source = Console.In.ReadToEnd();
    }
    else
    {
      // Read from file
      if (!File.Exists(file))
      {
        AnsiConsole.MarkupLine($"[red]File not found:[/] {file}");
        return -1;
      }
      source = File.ReadAllText(file);
    }

    SourceText st = SourceText.From(source);
    BrimModule module = Parser.ModuleFrom(st);

    AnsiConsole.MarkupLine("[bold]Parse Tree:[/]");
    AnsiConsole.Write(GreenNodeFormatter.RenderTree(source, module));
    if (showSource)
    {
      AnsiConsole.WriteLine();
      string sourceText = source;

      List<string> lines = [];
      StringBuilder lineBuilder = new();
      for (int i = 0; i < sourceText.Length; i++)
      {
        char ch = sourceText[i];
        if (ch == '\n')
        {
          lineBuilder.Append('\n'); // keep marker to visualize
          lines.Add(lineBuilder.ToString());
          lineBuilder.Clear();
          continue;
        }

        if (ch == '\r')
        {
          // If next is \n we still show both explicitly
          lineBuilder.Append('\r');
          continue;
        }

        lineBuilder.Append(ch);
      }

      if (lineBuilder.Length > 0)
        lines.Add(lineBuilder.ToString());

      int lineDigits = Math.Max(2, lines.Count.ToString().Length);
      AnsiConsole.MarkupLine("[bold]Source[/]");
      for (int i = 0; i < lines.Count; i++)
      {
        string ln = (i + 1).ToString().PadLeft(lineDigits, '0');
        // Visualize control chars inside line
        string content = lines[i]
          .Replace("\r", "␍")
          .Replace("\n", "␊");
        content = Markup.Escape(content);
        AnsiConsole.MarkupLine($"[grey]{ln}[/] {content}");
      }

      AnsiConsole.WriteLine();
    }

    if (module.Diagnostics.Count == 0)
      AnsiConsole.MarkupLine("[green]No diagnostics.[/]");
    else
      AnsiConsole.MarkupLine($"[red]{module.Diagnostics.Count} diagnostics.[/]");

    if (showDiagnostics)
    {
      StructuralArray<Diagnostic> diags = module.Diagnostics;
      AnsiConsole.MarkupLine("[red]Diagnostics:[/]");
      foreach (Diagnostic d in diags)
      {
        string msg = DiagnosticRenderer.Render(d);
        AnsiConsole.MarkupLineInterpolated($"[yellow]{d.Line:D3}:{d.Column:D3}[/] {msg}");
      }
    }

    return 0;
  }
}
