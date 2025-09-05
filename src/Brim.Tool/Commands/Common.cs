using System.CommandLine;

namespace Brim.Tool.Commands;

static class Common
{
  public enum Verbosity
  {
    Quiet,
    Minimal,
    Normal,
    Detailed,
    Diagnostic,
  }

  public static Option<Verbosity> VerbosityOption = new("--verbosity", "-v")
  {
    Recursive = true,
    DefaultValueFactory = static a => Verbosity.Normal,
  };
}
