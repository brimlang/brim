using System.CommandLine;
using Brim.Tool.Commands;
using Spectre.Console;

#pragma warning disable IDE0028 // Collection expression can be simplified

RootCommand rootCommand = new("brim - toolchain for the brim language")
{
  new BuildCommand(),
  new LexCommand(),
  new ParseCommand(),
};

rootCommand.Options.Add(Common.VerbosityOption);

#pragma warning restore IDE0028

if (Environment.GetCommandLineArgs().First().EndsWith("hinky"))
  AnsiConsole.MarkupLine(Hinky.A);

// Attach console trace listener when BRIM_TRACE env set
if (Environment.GetEnvironmentVariable("BRIM_TRACE") == "1")
{
  System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.ConsoleTraceListener());
}

return await rootCommand.Parse(args).InvokeAsync();
