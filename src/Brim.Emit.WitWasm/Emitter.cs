using System.Text;
using Brim.C0;

namespace Brim.Emit.WitWasm;

public static class WitEmitter
{
  public static string EmitWit(C0Module module)
  {
    // Tiny placeholder — just enough to smoke test CLI.
    StringBuilder b = new StringBuilder()
    .AppendLine($"// wit for {module.CanonicalName}")
    .AppendLine("package brim:auto")
    .AppendLine($"world {module.CanonicalName} {{}}");
    return b.ToString();
  }
}
