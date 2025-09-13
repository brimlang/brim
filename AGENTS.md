# AGENTS.md (Concise Ops & Style Reference)

> Pre-release Policy: Brim is in its initial pre-release phase. All language, toolchain, and library surfaces are subject to change without deprecation cycles or migration shims. Specs and this file always describe the current state; do not preserve legacy syntax or behaviors. Remove outdated guidance instead of marking it deprecated. Once a formal 1.0 stabilization plan begins, an explicit compatibility policy will be added here.

1. Setup: `mise trust && mise install` (installs dotnet 9 + tasks).
2. Build: `mise run build` (or `dotnet build`); AOT: `mise run aot:linux-x64:publish`.
3. Lint (verify): `mise run lint`; Auto-format: `mise run format` (CI fails on unformatted code).
4. Test all: `mise run test`; Single test project: `dotnet test tests/Brim.Parse.Tests -c Release -f net9.0 --filter FullyQualifiedName~ParserPredictionTests`.
5. Fast inner-loop examples: lex `mise rx lex ./demo.brim`; parse `mise rx parse ./demo.brim`; diagnostics `mise rx parse --diagnostics ./demo.brim`.
6. Language/Compiler rules: grammar must remain LL(k≤4); no lookahead >4; prefer table-driven predictions over nested switches.
7. Tree model: single immutable layer (names with Green* are historical—do NOT add a red layer).
8. Diagnostics: allocate-light value types, capped at 512; last slot becomes TooManyErrors sentinel.
9. Coding style: C# preview, `<Nullable>enable</Nullable>`, implicit usings on; treat warnings as errors; keep analyzer warnings at zero.
10. Naming: prefer explicit, PascalCase types, camelCase locals, ALL_CAPS only for const static readonly primitives if strongly justified (avoid new ones unless pattern exists).
11. Imports/usings: rely on implicit usings; place explicit usings sorted System.* first, then project namespaces; remove unused (lint enforces).
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

