using System.CommandLine;
using Brim.Tool.Commands;
using Spectre.Console;

RootCommand rootCommand = new("brim - toolchain for the brim language")
{
  new BuildCommand(),
  new LexCommand(),
  new ParseCommand(),
};

rootCommand.Options.Add(Common.VerbosityOption);

if (Environment.GetCommandLineArgs().First().EndsWith("hinky"))
  AnsiConsole.MarkupLine(Hinky.A);

// Attach console trace listener when BRIM_TRACE env set
if (Environment.GetEnvironmentVariable("BRIM_TRACE") == "1")
{
  _ = Trace.Listeners.Add(new ConsoleTraceListener(useErrorStream: true));
}

return await rootCommand.Parse(args).InvokeAsync();
