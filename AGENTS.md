
# Instructions for Agents

Dev dependencies are managed using mise. See https://mise.jdx.dev/

./mise.toml contains the configuration for mise.

To install the dependencies, run:

```
mise trust
mise install
```

Common dev tasks are also defined in ./mise.toml

# Purpose

This project is the single binary toolchain for the Brim programming language.

It supports:

* LSP
* DAP
* Compile and emit WIT/WASMGC

# Project Status

There is no stable release yet. The project is in active development.

# Brim Programming Language

* See the [Brim Syntax Summary](./brim_syntax_summary.md) for a summary on the Brim programming language syntax.
* See the [simple demo](./demo.brim) for a small example of a Brim program.
* See [parser pipeline overview](/docs/compiler_pipeline.md).

## Grammar Strategy

The brim grammar is designed to be LL(k) parsable for k <= 3. For expression parsing, we can use a Pratt parser.
Where LL(k) for k = 3 is not possible, prefer proposing a refactoring to the language to make it LL(k) for k <= 3.

Maps and tables for driving the parser are preferred over large switch statements.

Parser previously targeted a Roslyn-style red/green tree. We have since simplified to a single immutable syntax tree built by a fully streaming pipeline (see below). Any legacy references to “red/green” in code comments are candidates for cleanup.

Diagnostics are produced through a centralized factory (see docs/diagnostics.md) that creates compact value-type `Diagnostic` instances without allocating message strings. Human‑readable text is generated later by a renderer so that parsing/lexing hot paths remain allocation‑light.

---

# Compiler Architecture (Agent Quick Reference)

High-level goals:
* Approachability for contributors (avoid over-engineering or deep compiler jargon).
* Streaming from source → syntax tree (no eager materialization of all tokens).
* Strong LSP/DAP support: precise offsets, lines, columns; trivia preserved where useful.
* Grammar remains LL(k) with k ≤ 3; expression layer may use Pratt parser.
* Avoid allocating full source strings repeatedly; rely on offsets into a single backing buffer.

## Streaming Pipeline Overview

```
SourceText (ReadOnlyMemory<char>)
  → RawTokenSource (ref struct lexer; outputs Token incl. trivia & terminator runs)
  → SignificantTokenEnumerator (ref struct; attaches leading trivia only)
  → LookaheadWindow (ref struct; k ≤ 3 significant tokens)
  → Parser (ref struct; constructs SyntaxNode / SyntaxTokenNode tree)
  → Immutable Syntax Tree (nodes store offsets/lengths; no spans captured)
```

All span-based operations are confined to short-lived `ref struct` layers. Persistent objects (tokens, syntax nodes) store only scalar metadata (offset, length, line, column, kind, optional diagnostics).

## Tokenization Invariants

* Whitespace and comments are trivia; they never appear as distinct syntax nodes—only as leading trivia attached to significant tokens (currently we do not attach trailing trivia; add later if required).
* Newline ("\n") and semicolon (';') characters are both statement separators. Any contiguous run of one or more of these forms a single `Terminator` token (collapsed). The parser treats every `Terminator` uniformly.
* The composition (counts of newlines vs semicolons) is intentionally **not** precomputed or stored. Future tools (formatter, linter) can analyze the token slice on demand.
* Identifiers: Unicode rules per `Lexer.IsIdentifierStart/Part` helpers; casing not semantic.
* No lookahead beyond 3 significant tokens should be required—design new grammar constructs with this constraint in mind.

## Parser Design Principles

* Table/map driven prediction for top-level constructs (see existing predict tables) rather than large cascaded switch statements.
* Always guarantee progress: on failure emit an error token/node with diagnostic and advance at least one significant token.
* Keep error recovery localized—do not attempt complex panic-mode until language surface demands it.
* Expression parser (when implemented) should isolate precedence logic (Pratt) from the rest of the LL(k) structure.

## Syntax Tree Model

* Single immutable tree (no separate green/red layers).
* Leaf nodes are `SyntaxTokenNode` wrapping a `SignificantToken` (which itself holds the core token + leading trivia list).
* Internal nodes aggregate children; their span (offset/length) is derived from first/last child.
* Diagnostics attach to nodes or tokens as structural arrays / immutable lists; they reference offsets only.

## Source Handling

* `SourceText` owns the immutable backing `ReadOnlyMemory<char>` plus a precomputed line-start index for fast (offset ↔ line/column) mapping.
* No component stores `ReadOnlySpan<char>` beyond `ref struct` lifetimes.
* Reconstruct original lexeme text by slicing `SourceText.AsSpan().Slice(offset, length)`.

## Naming Conventions

| Concept | Name |
|---------|------|
| Raw lexer token (incl. trivia) | `Token` |
| Significant token + leading trivia | `SignificantToken` |
| Leaf syntax node | `SyntaxTokenNode` |
| Internal node | (Specific, e.g. `ModuleNode`, or generic `CompositeNode`) |
| Statement boundary token | `TokenKind.Terminator` |
| Parser lookahead window | `LookaheadWindow` |
| Trivia attach layer | `SignificantTokenEnumerator` |

Avoid reintroducing Roslyn-specific terms (e.g., “GreenNode”, “RedNode”) unless strictly required.

## Performance & Memory Guidelines

* Prefer value `record struct` for lightweight immutable tokens; keep them small (avoid large reference fields).
* `ref struct` only where holding a `ReadOnlySpan<char>` or tight hot-path state (lexer, enumerators, lookahead, parser).
* Do not prematurely cache terminator composition; measure first if style tools iterate heavily.
* Honor streaming: avoid collecting all tokens unless adding an explicit *batch* mode for tooling.

## Extensibility Guidelines for Agents

When adding or modifying grammar features:
1. Confirm they are LL(k) with k ≤ 3; if not, propose a language refactor before increasing k.
2. If a new token category is needed, update `TokenKind`, the lexer symbol table, and any predict tables.
3. Keep error diagnostics minimal but precise (unexpected token, unterminated literal, etc.).
4. Maintain the invariant that all trivia before a significant token is captured as leading trivia.

When introducing formatting or lint passes:
* Implement them as consumers of the syntax tree + original `SourceText` (an additional streaming walk is fine).
* Use utilities to analyze terminator token slices on demand.

## Common Agent Tasks (Cheat Sheet)

* Add language feature: extend lexer → add predict table entry → implement parse node → update docs.
* Improve diagnostics: add new `Diagnostic` factory method and attach where parse failure surfaces.
* Add formatting rule: create separate pass; never mutate existing tokens—produce edits / suggestions referencing offsets.
* LSP hover/completion: map from offset to leaf token via tree walk (can add simple index if hotspots arise later).

## Non-Goals (Current)

* Incremental parsing / incremental re-lexing across edits (can be added later if needed for performance).
* Full red/green dual tree layering.
* Semantic analysis layer (type checking, etc.)—not part of this syntactic pipeline doc.

---

If an agent encounters ambiguity (e.g., conflicting design comments), default to the rules in this section and suggest a cleanup PR to align outdated comments.
