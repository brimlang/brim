# ARCHITECTURE.md

## Purpose
Document core architecture, invariants, and extension workflow separate from concise AGENTS.md. Provide enough detail for new contributors without duplicating transient backlog items.

## Repository Layout
- `Brim.Parse`: lexing, token windows, parsing, diagnostics production.
- `Brim.C0`: canonical unsugared core (validation foundation; minimal now).
- `Brim.S0`: surface sugar expander (CST → core) — idempotent & semantics-preserving.
- `Brim.Emit.WitWasm`: emission (WIT / Wasm GC).
- `Brim.Tool`: single CLI binary (`brim`).
- `spec/`: design docs & proposals.
- `tests/`: per-project test suites (prediction, diagnostics, regression).

## High-Level Goals
- Approachability over cleverness.
- Deterministic, allocation-lean parsing with LL(k≤4).
- Single immutable syntax tree layer (legacy Green* names retained; no red layer).
- Fast editor tooling: stable offsets + line/col + trivia.
- Diagnostics as structured value types; rendering deferred.

## Build & Tooling
- Toolchain: `mise trust && mise install` (dotnet 9 preview).
- Common: build `mise run build`; lint `mise run lint`; format `mise run format`; test `mise run test`.
- AOT publish: `mise run aot:linux-x64:publish`.
- Inner loop parse: `mise rx parse demo.brim [--diagnostics]`.

## Compilation Pipeline
```
SourceText
  → RawTokenProducer
  → SignificantTokenProducer (leading trivia attach)
  → Lookahead window (k≤4)
  → Parser (prediction tables)
  → Immutable syntax tree (node/value structs)
  → (Future) S0 sugar → C0 canonical → Emit
```

## Grammar Strategy
- Enforce LL(k≤4); if pressure to exceed 4, prefer grammar tweak.
- Prediction tables/maps instead of deeply nested switches.
- Future expression layer may use Pratt—still constrained by predictability.

## Tokens & Lexing Invariants
- Whitespace/comments = leading trivia only (current design).
- Consecutive newline/semicolon sequences collapse to single `Terminator`.
- Identifiers: `IsIdentifierStart/Part`; case not semantic (yet).
- No materialized substrings stored in tree; positions point into `SourceText`.

## Parser Principles
- Table-driven predictions per nonterminal.
- Guaranteed progress: on mismatch emit diagnostic then advance.
- Local recovery only; no multi-phase panic parse.
- Expression precedence isolated in future module.

## Syntax Tree Model
- Single layer; nodes/tokens immutable (span = first..last child).
- Tokens carry offset, width, line, column, leading trivia list.
- No red overlay; do not resurrect dual model.
- Structural nodes are strongly typed records/classes.

## Diagnostics System
- Value-type entries; max 512 (flood cap) → last slot = `TooManyErrors` sentinel.
- Fields: offset, line, column, width, code, phase (lex|parse|future semantic), severity.
- Stable sort (offset,line,col,code) ensures deterministic output.
- Rendering (color, formatting) deferred to CLI / future LSP.

## Performance & Memory
- Favor small `readonly struct` in hot paths; avoid capturing spans beyond lifetime.
- Stream tokens; never fully buffer without evidence.
- Benchmark before micro-optimizing; document heuristic changes.

## Formatting & Style Summary
- C# preview, nullable enabled, implicit usings; warnings = errors.
- Explicit usings sorted: System.* first, then project namespaces.
- Rely on `dotnet format`; braces always; one statement per line; no trailing whitespace.
- Avoid new ALL_CAPS constants unless consistent with existing pattern.

## Extensibility Workflow
1. Confirm LL(k≤4) for new grammar construct.
2. Add token kinds + prediction entries.
3. Implement parse node type / construction.
4. Emit diagnostics via factory; document codes.
5. Add tests: prediction collision, diagnostics, regression.
6. Update docs/spec minimally & meaningfully.

## Testing Strategy
- Prediction collision tests guard FIRST-set uniqueness.
- Flood cap and edge diagnostics tests in `Brim.Parse.Tests`.
- Future golden/snapshot tests for renderer (add once stable).
- Test naming: `Feature_Scenario_Expectation`.

## CLI Behavior
- Default parse: tree only (diagnostics off by default).
- Enable diagnostics with `--diagnostics`.
- Location format: `@line:col(width)`; avoid raw offset ranges in user output.
- Color categories: identifiers, generics, directives, declarations, errors, containers, fallback.

## Future Layers
- Semantic analysis (types, name resolution) after tree stabilizes.
- Warnings introduction (e.g., unused imports) post semantic pass.
- Potential incremental parsing (explicit non-goal until grammar stabilizes).

## Non-Goals (Current)
- Dual red/green infrastructure.
- Incremental parse caching.
- Full type inference / advanced semantics.

## Selected Backlog Snapshot
- Introduce first warning diagnostic to validate severity path.
- Monochrome CLI mode & theme configuration.
- Benchmark harness for large corpus & hotspot validation.
- Deduplicate repeated unexpected/missing diagnostic bursts.

## Key Invariants
- Never exceed lookahead of 4 significant tokens.
- No mutable global parser state.
- Diagnostics order stable across runs.
- Single immutable tree layer—changes require explicit design review.
