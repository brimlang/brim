using Brim.Parse;

namespace Brim.C0;

public static class C0FromParsed
{
  public static C0Module From(ParsedUnit u) => new(u.ModuleName ?? "main", Array.Empty<C0Decl>());
}
