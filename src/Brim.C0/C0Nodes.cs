namespace Brim.C0;

public abstract record C0Node;

public sealed record C0Module(
    string CanonicalName,
    IReadOnlyList<C0Decl> Decls
) : C0Node;

public abstract record C0Decl(string Name) : C0Node;

public sealed record C0Function(string Name, string ReturnType) : C0Decl(Name);
