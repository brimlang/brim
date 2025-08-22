using System.Diagnostics;

namespace Brim.BinaryenSmoke.Tests;

public class WasmOptTests
{
  [Fact]
  public void WasmOptReportsVersion()
  {
    string home = Environment.GetEnvironmentVariable("BINARYEN_HOME")
      ?? throw new InvalidOperationException("BINARYEN_HOME not set");
    string exe = OperatingSystem.IsWindows() ? "wasm-opt.exe" : "wasm-opt";
    string wasmOpt = Path.Combine(home, "bin", exe);
    Assert.True(File.Exists(wasmOpt), $"Missing Binaryen tool: {wasmOpt}");

    ProcessStartInfo psi = new(wasmOpt, "--version")
    {
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false
    };
    using Process proc = Process.Start(psi)!;
    string stdout = proc.StandardOutput.ReadToEnd();
    proc.WaitForExit();
    Assert.Equal(0, proc.ExitCode);
    Assert.NotEmpty(stdout);
  }
}
