using Brim.C0;

namespace Brim.S0;

public static class S0Pass
{
  // For now, just identity: pretend we converted CST->C0
  public static C0Module Run(string moduleName, IEnumerable<string> inputs)
      => new(moduleName, Array.Empty<C0Decl>());
}
