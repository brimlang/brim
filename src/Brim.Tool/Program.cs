using Brim.C0;
using Brim.Emit.WitWasm;
using Brim.Parse;
using Spectre.Console;

return Run(args);

static int Run(string[] args)
{
  if (args.Length == 0 || IsHelp(args[0]))
    return PrintHelp();

  string cmd = args[0].ToLowerInvariant();
  switch (cmd)
  {
    case "version":
      AnsiConsole.MarkupLine("brim (dev)");
      return 0;

    case "build":
      return Build([.. args.Skip(1)]);

    default:
      AnsiConsole.MarkupLine($"[red]unknown command:[/] {cmd}");
      return PrintHelp();
  }
}

static int Build(string[] args)
{
  // very small arg parser: --dump=... and paths
  string? dump = null;
  List<string> paths = [];
  foreach (string a in args)
  {
    if (a.StartsWith("--dump=", StringComparison.Ordinal))
      dump = a["--dump=".Length..];
    else if (a == "--dump")
      dump = ""; // allow --dump parse later to read next token if you want to extend
    else
      paths.Add(a);
  }
  if (paths.Count == 0) paths.Add(".");

  List<string> files = ExpandInputs(paths);
  if (files.Count == 0)
  {
    AnsiConsole.MarkupLine("[red]no .brim files found[/]");
    return 1;
  }

  string[] which = (dump ?? "")
      .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

  foreach (string file in files)
  {
    string text = File.ReadAllText(file);
    ParsedUnit parsed = BrimParser.ParseString(text);
    C0Module c0 = C0FromParsed.From(parsed);

    if (which.Contains("parse"))
    {
      AnsiConsole.MarkupLine($"[grey]{file}[/]");
      AnsiConsole.MarkupLine($"  module: [cyan]{parsed.ModuleName ?? "(none)"}[/]");
      if (!string.IsNullOrEmpty(parsed.TomlFrontMatter)) AnsiConsole.MarkupLine("  toml:    (present)");
      if (parsed.Exports.Count > 0) AnsiConsole.MarkupLine($"  exports: {string.Join(", ", parsed.Exports)}");
      if (parsed.HasTopLevelVarDecl) AnsiConsole.MarkupLine("  state:   (top-level var decls present)");
    }

    if (which.Contains("c0"))
    {
      AnsiConsole.MarkupLine("[grey]C0:[/]");
      AnsiConsole.WriteLine($"  module: {c0.CanonicalName}, decls: {c0.Decls.Count}");
    }

    if (which.Contains("wit"))
    {
      string wit = WitEmitter.EmitWit(c0);
      AnsiConsole.MarkupLine("[grey]WIT:[/]");
      AnsiConsole.WriteLine(wit);
    }

    if (which.Length == 0)
      AnsiConsole.MarkupLine("[green]ok[/]");
  }

  return 0;
}

static bool IsHelp(string s) => s is "-h" or "--help" or "help" or "/?";

static int PrintHelp()
{
  AnsiConsole.WriteLine("brim — single-binary toolchain");
  AnsiConsole.WriteLine();
  AnsiConsole.WriteLine("Usage:");
  AnsiConsole.WriteLine("  brim build [--dump=parse,c0,wit] [paths...]");
  AnsiConsole.WriteLine("  brim version");
  return 1;
}

static List<string> ExpandInputs(IEnumerable<string> inputs)
{
  List<string> outFiles = [];
  foreach (string p in inputs)
  {
    if (Directory.Exists(p))
    {
      outFiles.AddRange(Directory.EnumerateFiles(p, "*.brim", SearchOption.AllDirectories));
    }
    else if (File.Exists(p) && Path.GetExtension(p).Equals(".brim", StringComparison.OrdinalIgnoreCase))
    {
      outFiles.Add(p);
    }
  }
  return outFiles;
}

