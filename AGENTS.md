
﻿# Instructions for Agents

Dev dependencies are managed using mise. See https://mise.jdx.dev/

`./mise.toml` contains the configuration for mise.

Install / update toolchain:

```
mise trust
mise install
```

Common dev tasks are also defined in `./mise.toml`.

Test lexer - `mise rx lex ./demo.brim`
Test parser - `mise rx parse ./demo.brim`
Test parser with diagnostics - `mise rx parse --diagnostics ./demo.brim`

All outputs go to stdout.

# Purpose

Single binary toolchain for the Brim programming language.

It supports:

* LSP
* DAP
* Compile and emit WIT/WASMGC
* Rich CLI developer introspection (parse tree, diagnostics, formatting helpers)

# Project Status

No stable release yet. Active development; APIs / grammar will change.

# Brim Programming Language

* See the [Brim Syntax Summary](./brim_syntax_summary.md) for a brief syntax overview for early development.
* See the [demo program](./demo.brim) for a small example.
* See the [Spec corpus](./spec) for detailed design documentation.


## Grammar Strategy

Grammar is designed for LL(k) with k ≤ 4. Expressions may later use a Pratt parser. If a construct appears to require k > 4, prefer a language tweak before increasing k.

Prediction tables/maps are preferred over large nested switches.

Parser previously targeted a Roslyn-style red/green tree. We now use a single immutable syntax tree; any legacy “green/red” terminology is historical—do not reintroduce a dual layer. Current type names such as `GreenNode` remain but represent the single layer.

Diagnostics use a centralized factory producing compact value-type instances with no message string allocation. Rendering happens later (CLI, LSP). Each diagnostic stores Phase (lex/parse/semantic) and Severity (error/warning/info). A flood cap (512) replaces the final entry with a `TooManyErrors` sentinel; further diagnostics are suppressed.

---

# Compiler Architecture (Agent Quick Reference)

High-level goals:
* Approachability over cleverness.
* Streaming source → syntax tree; no full token buffering.
* Strong LSP/DAP fidelity: offsets + line/col + trivia.
* LL(k) (k ≤ 4) grammar discipline.
* Minimize repeated string slicing / allocations.

## Streaming Pipeline

```
SourceText (ReadOnlyMemory<char>)
  → RawTokenProducer (lexer)
  → SignificantTokenProducer (attaches leading trivia only)
  → LookAheadWindow (k ≤ 4)
  → Parser (produces immutable nodes + diagnostics)
  → Immutable Syntax Tree
```

## Tokenization Invariants

* Whitespace/comments = trivia (leading only for now).
* Runs of newline / semicolon collapse into one `Terminator` token.
* Terminator composition (counts) intentionally not stored.
* Identifiers follow `Lexer.IsIdentifierStart/Part` rules; case not semantic.
* No lookahead beyond 4 significant tokens for any prediction.
* Materialized strings never stored in tree; only positions.

## Parser Principles

* Table-driven predictions for top-level forms.
* Guaranteed progress: on mismatch emit diagnostic + advance.
* Local recovery (no complex panic strategies yet).
* Expression layer (future) isolates precedence logic.

## Syntax Tree Model

* Single immutable tree; node span = first-to-last child.
* Leaf tokens carry line/column/offset/width + leading trivia list.
* Diagnostics kept externally sorted (offset, line, column, code) for stable output.

## Source Handling

* `SourceText` retains full buffer + line index for O(1) offset→line/col mapping.
* Slicing only when producing lexemes for display/formatting.

## Naming Conventions (Current Codebase)

| Concept | Name |
|---------|------|
| Raw lexer token (incl. trivia) | `RawToken` |
| Significant token + leading trivia | `SignificantToken` |
| Leaf syntax node | `GreenToken` |
| Internal composite | Specific record (e.g. `StructDeclaration`) |
| Statement boundary | `TerminatorToken` |
| Lookahead buffer | `LookAheadWindow` |
| Trivia attach layer | `SignificantProducer` |
| Parse tree renderer | `GreenNodeFormatter` |
| Diagnostic sink | `DiagSink` |

Avoid resurrecting obsolete “red” layer concepts.

## Performance & Memory Guidelines

* Favor small value types; avoid capturing slices beyond transient `ref struct` lifetimes.
* Measure before micro‑optimizing (inlining, packing) except in trivially hot loops.
* Flood cap prevents pathological diagnostic explosions; keep it configurable only if profiling demands.

## Diagnostics Model Enhancements

Currently emitted phases: lex, parse. Future: semantic. Severities presently all errors—add at least one warning soon to validate pipeline.

Flood Cap Logic:
* Max 512 diagnostics.
* Upon overflow, last slot replaced by `TooManyErrors` diagnostic; `IsCapped` flag set; further adds ignored.

Binary Search Index:
* `BrimModule.FindFirstDiagnosticAtOrAfter(offset)` enables quick mapping from offset → diagnostics sequence (used for future editor features).

## CLI Tree View (`parse` Command)

* Default output: parse tree + original source echo (diagnostics suppressed by default).
* Enable diagnostics with `--diagnostics`.
* Comment trivia rendered as child lines beginning with `#` in grey.
* Colors: identifiers cyan; generic tokens grey; directives green; declarations (types/functions/import/export) magenta; errors red; containers blue/purple/teal; misc yellow fallback.

Examples:
```
brim parse demo.brim
brim parse demo.brim --diagnostics
```

## Extensibility Guidelines

When adding grammar:
1. Verify LL(k ≤ 4). If not, propose grammar alteration.
2. Extend lexer token kinds + prediction tables.
3. Emit diagnostics via factory (`DiagFactory.*`).
4. Write tests (prediction collisions, diagnostic correctness).

Formatting / Lint passes:
* Consume existing tree; do not mutate nodes—produce edits referencing offsets.
* Avoid dependence on offset ranges for readability; prefer line:col in user output.

## Common Agent Tasks

* New feature: lexer → prediction table → parse node → docs/tests.
* Diagnostics: add code + factory + renderer case + tests (respect cap).
* Output tweaks: adjust `GreenNodeFormatter` (keep location format compact, avoid raw offset ranges).
* LSP integration: use binary search + node token lookup for position mapping.

## Non‑Goals (Current)

* Incremental / partial reparse.
* Dual red/green layering.
* Full semantic/type system (future layer will build atop current tree).

---

Ambiguities: default to rules here; mark stale comments for cleanup PR.

---

## Backlog (Supersedes Older List)

1. Prediction / Grammar
  - [x] Split per-nonterminal prediction arrays.
  - [x] FIRST collision validator.
  - [ ] Collapse identical prediction prefixes (micro perf).

2. Tree & Nodes
  - [ ] Centralize `FullWidth` calc in base.
  - [ ] Consider `ITokenNode` to unify leaf access.

3. Diagnostics
  - [x] Phase & severity fields added.
  - [x] Stable sort (offset,line,col,code).
  - [x] Flood cap (512 + sentinel).
  - [ ] Emit first warning (e.g., trailing whitespace) to validate severity path.
  - [ ] Dedupe consecutive identical unexpected/missing bursts.

4. CLI / Tooling
  - [x] Compact location format `@line:col(width)`.
  - [x] `--diagnostics` flag (default suppress).
  - [ ] Monochrome fallback mode.

5. Docs & Tests
  - [ ] Golden output tests for tree renderer.
  - [ ] Diagnostic renderer message snapshot tests.

6. Future / Semantic
  - [ ] Introduce semantic analysis pass (types, name resolution).
  - [ ] Semantic diagnostics (phase=semantic) integration.

7. Localization & Formatting
  - [ ] Resource string scaffolding for diagnostic messages.
  - [ ] Configurable color theme / no-color env detection.

8. Performance
  - [ ] Benchmark harness (lexer, parser, renderer) w/ large corpus.
  - [ ] Investigate expected-kind dedupe branch-free variant.

---

Contribute improvements incrementally; keep hot paths allocation‑lean; favor clarity over premature compression. Profile before deep optimization.

