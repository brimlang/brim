namespace Brim.Parse;

public sealed record ParsedUnit(
    string? ModuleName,
    string? TomlFrontMatter,
    IReadOnlyList<string> Exports,
    bool HasTopLevelVarDecl
);

