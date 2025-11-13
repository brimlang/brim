# AGENTS.md (Concise Ops & Style Reference)

> Pre-release Policy: Brim is in its initial pre-release phase. All language, toolchain, and library surfaces are subject to change without deprecation cycles or migration shims. Specs and this file always describe the current state; do not preserve legacy syntax or behaviors. Remove outdated guidance instead of marking it deprecated. Once a formal 1.0 stabilization plan begins, an explicit compatibility policy will be added here.

**CRITICAL:** Use the repo-local `tmp/` directory for all temporary files. NEVER use global `/tmp/` or other system temp directories.

1. Setup: `mise trust && mise install` (installs dotnet 10 preview + tasks).
2. Build: `mise run build` (or `dotnet build`); AOT: `mise run aot:linux-x64:publish`.
3. Lint (verify): `mise run lint`; Auto-format: `mise run format` (CI fails on unformatted code).
4. Test all: `mise run test`; Single test projects:
    - Parser predictions: `dotnet test tests/Brim.Parse.Tests -c Release -f net10.0 --filter FullyQualifiedName~ParserPredictionTests`.
    - Core primitives: `dotnet test tests/Brim.Core.Tests -c Release -f net10.0`.
5. Fast inner-loop examples: lex `mise rx lex ./demo.brim`; parse `mise rx parse ./demo.brim`; diagnostics `mise rx parse --diagnostics ./demo.brim`.
6. Language/Compiler rules: grammar must remain LL(k≤4); no lookahead >4; prefer table-driven predictions over nested switches. Keep `spec/sample.brim`, `spec/grammar.md`, `spec/fundamentals.md`, `spec/unicode.md`, and `spec/functions.md` in agreement; review the entire set whenever one changes, and if you find a mismatch or unclear direction, stop and ask the requester which version to follow before continuing.
6.1 **Brim function syntax is unique and has three distinct forms** (see spec/functions.md):
    - **Type declaration**: `Name := (Type, ...) Ret` creates a function type alias (no param names)
    - **Value declaration**: `name :(Type, ...) Ret = expr` binds a function value (often a lambda)
    - **Combined declaration**: `name :(param :Type, ...) Ret { body }` shorthand with named params (block required, NOT YET IMPLEMENTED)
    - Do NOT conflate these forms or assume conventions from other languages apply
6.2 Lexer policy (for prediction stability):
    - Greedy matching of compound glyphs using an ASCII table; longest-match wins. Sequences up to 3 characters (e.g., `::=`, `<<`, `|{`, `*{`, `!!{`, `[[`, `]]`, `.{`, `??`) are single tokens. A 4th char may be added in future; never regress to shorter fragments. Example: `[[` is a single token, never two `[` tokens.
    - Runs collapse to one token: non-terminator whitespace → one `WhitespaceTrivia`; newline runs → one `Terminator`.
    - Identifiers lex as single tokens (Unicode-aware start/part rules). Comments (`-- …\n`) lex as single `CommentTrivia`.
    - `src/Brim.Lex/CharacterTable.cs` owns the operator trie/longest-match table; keep glyph definitions there in sync with the spec before touching parser tables.
    - `LexTokenSource` is the single producer of lexed tokens (including `Eob`); `CoreTokenSource` adapts them into `CoreToken`s by attaching accumulated leading trivia only (no trailing trivia) and replays the cached `Eob` exactly once. Do not resurrect `SignificantProducer`-style flows.
    - Shared primitives (diagnostics, tokens, SourceText, structural collections) now live in `Brim.Core`; add new building blocks there instead of under `Brim.Parse`/`Brim.Lex`.
    - Design predictions against these stable first tokens (e.g., `<<`, `::=`, `:=`, `.[{]`, `^{`, `[[`, etc.) and ignore trivia — this keeps LL(k) small and tables robust.
7. Tree model: single immutable layer (names with Green* are historical—do NOT add a red layer).
8. Diagnostics: allocate-light value types, capped at 512; last slot becomes TooManyErrors sentinel.
9. Coding style: C# 13 (preview where applicable), `<Nullable>enable</Nullable>`, implicit usings ON; treat warnings as errors; keep analyzer warnings at zero. Agents should not assume modern C# syntax is incorrect — verify by building (`mise run build` or `dotnet build`) and rely on build errors before diagnosing syntax issues.
10. Naming: prefer explicit, PascalCase types, camelCase locals, ALL_CAPS only for const static readonly primitives if strongly justified (avoid new ones unless pattern exists).
11. Imports/usings: implicit usings are enabled; prefer relying on them. When adding explicit `using` directives place `System.*` first, then project namespaces; remove unused usings (lint enforces). Agents must not remove or change modern implicit usings without reproducing a build error that justifies the change.
12. Types & allocations: use `readonly struct` where beneficial; avoid capturing spans beyond lifetime; favor value semantics in hot paths.
13. Error handling: fail fast with diagnostics—not exceptions—for syntax issues; reserve exceptions for truly exceptional states (I/O, invariant violation).
14. Performance: measure before optimizing; avoid premature micro-opts; keep parsing streaming (no full token buffering).
15. Formatting: rely on `dotnet format`; do not hand-align; no trailing whitespace; one statement per line; braces always present.
16. Tests: add prediction collision + diagnostic coverage when extending grammar; keep test names descriptive (`Feature_Scenario_Expectation`).
17. CLI output: maintain compact location form `@line:col(width)`; avoid raw offset ranges in user-facing text.
18. Do not introduce semantic layer yet; future pass will add warnings (e.g., unused imports) — leave placeholders minimal.
19. Backlog reference: see backlog in prior AGENTS.md history (retain until separately documented); do not duplicate here.
20. If ambiguity: choose simpler design, document briefly, add/adjust tests, update this file ONLY if rule materially changes.
21. Commas: All comma-delimited lists across syntax forms preserve each comma token in the tree and permit at most one optional trailing comma before the closing delimiter. Empty lists never contain a comma.
22. Commits: follow Conventional Commits 1.0.0. Use `type[scope]: subject` in imperative mood; prefer focused commits. See `COMMITTING.md` for types, scopes, breaking-change notation, and examples.
