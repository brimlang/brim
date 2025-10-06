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
    • Module members: TypeDeclaration (Name + ':=' + TypeExpr)
      where TypeExpr = StructShape | UnionShape | FlagsShape | NamedTupleShape | Identifier | GenericType
  → (Future) S0 sugar → C0 canonical → Emit
```

## Grammar Strategy
- Enforce LL(k≤4); if pressure to exceed 4, prefer grammar tweak.
- Prediction tables/maps instead of deeply nested switches.
- Future expression layer may use Pratt—still constrained by predictability.

## Tokens & Lexing Invariants
- Greedy compound glyphs: operators and delimiters are lexed with longest-match semantics using a fixed table of ASCII sequences. Sequences up to 3 characters (e.g., `::=`, `<<`, `|{`, `*{`, `!!{`, `[[`, `]]`, `.{`, `??`) are matched greedily as single tokens. A future extension to 4 characters is acceptable, but never shorter-than-longest. Example: `[[` must lex as a single `LBracketLBracket` token, never two `LBracket` tokens.
- Single-run tokens: non-terminator whitespace runs lex to one `WhitespaceTrivia` token; newline runs lex to one `Terminator` token.
- Comments: `-- …\n` lex to a single `CommentTrivia` token.
- Trivia shaping: SignificantProducer attaches all trivia as leading trivia to the following significant token (or to `Eob`). There is no trailing trivia.
- Identifiers: one `Identifier` token per identifier; `IsIdentifierStart/Part` follows Unicode categories; case is not semantic (yet).
- Determinism: the lexer never backtracks; the table is authoritative; unknown characters yield a single `Error` token and an `InvalidCharacter` diagnostic.
- Positions: no materialized substrings stored in the tree; node/token positions reference `SourceText`.

## Parser Principles
- Table-driven predictions per nonterminal.
- Guaranteed progress: on mismatch emit diagnostic then advance.
- Local recovery only; no multi-phase panic parse.
- Expression precedence isolated in future module.
- Predictability: Because lexing is greedy and run-collapsing, prediction tables can rely on small k (≤4) and stable first tokens (e.g., `<<`, `::=`, `:=`, `.[{]`, `^{`, `[[`, etc.). Design grammar around these stable starters rather than whitespace-sensitive sequences.

### Token Preservation Rule
- Every token produced by SignificantProducer must appear in the Green tree, in lexical order.
- Leading trivia attaches to the following GreenToken; there is no trailing trivia. Terminators and comments are standalone GreenTokens at their source positions.
- All delimited forms must keep their delimiters as explicit GreenTokens on the owning node.
- All comma- or plus-separated lists must preserve separators by using typed element nodes that carry an optional trailing separator token. Never drop separators by storing raw children as `GreenNode` only.
- A token-order regression in `tests/Brim.Parse.Tests/TokenOrderRegressionTests.cs` verifies that the flattened GreenToken stream matches the Significant token stream.

## Syntax Tree Model
- Single layer; nodes/tokens immutable (span = first..last child).
- Tokens carry offset, width, line, column, leading trivia list.
- No red overlay; do not resurrect dual model.
- Structural nodes are strongly typed records/classes.

### Delimited List Model
- All comma-delimited productions share a unified generic container: `CommaList<TElement>`.
- Each element type `TElement` is a GreenNode (e.g., `FunctionParameter`, `GenericArgument`, `NamedTupleElement`, `ProtocolRef`). Element-internal trailing separators are represented structurally by a preceding comma token captured on the following element (`LeadingComma`) and optional inter-element terminator (`LeadingTerminator`).
- The list node preserves:
  - Open token
  - Optional leading terminator
  - Ordered element sequence (each element may carry LeadingComma / LeadingTerminator)
  - Optional trailing comma (at list level)
  - Optional trailing terminator
  - Close token
- Parsing is centralized in `CommaList<T>.Parse(Parser, openKind, closeKind, parseElement)`; avoid bespoke loops.
- Legacy `Delimited.ParseCommaSeparatedTypes` is being removed; do not reintroduce per-site ad hoc parsing. Prefer extending `CommaList<T>` if a new delimiter policy appears.
- For heterogeneous or keyed member sets (e.g., struct fields) continue to model a distinct element node per production; only use `CommaList<T>` when the production grammar matches `CommaListOpt<T>` or `CommaList<T>` forms.

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
