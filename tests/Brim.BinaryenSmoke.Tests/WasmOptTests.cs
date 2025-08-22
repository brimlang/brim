using System.Diagnostics;

namespace Brim.BinaryenSmoke.Tests;

public class WasmOptTests
{
  [Fact]
  public void WasmOptReportsVersion()
  {
    ProcessStartInfo psi = new("wasm-opt", "--version")
    {
      RedirectStandardOutput = true,
      RedirectStandardError = true
    };
    using Process proc = Process.Start(psi)!;
    proc.WaitForExit();
    Assert.Equal(0, proc.ExitCode);
    Assert.NotEmpty(proc.StandardOutput.ReadToEnd());
  }
}
