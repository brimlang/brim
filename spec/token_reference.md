---
id: reference.tokens
status: draft
title: Token & Surface Reference (Non-Canonical)
layer: core
updated: 2025-09-08
authors: ['trippwill','assistant']
---

# Token & Surface Reference (Non-Canonical)

Status: draft (informative). This document is a convenience index; canonical meanings reside in accepted specs (`fundamentals`, `aggregates`, `services`).

## Legend
- Cat: category (lexical, delim, keyword-ish, operator, aggregate, pattern-only, sugar-proposal)
- Canonical?: whether token form is present in an accepted spec (Y) or only in proposals (P) / drafts (D)

## Core Lexical Tokens
- Identifiers: Unicode (canonicalization rules TBD) — Cat: lexical — Canonical?: Y
- Integer literals: see [numeric literals](./core/syntax/numeric_literals.md) (may include type suffix) — Cat: literal — Canonical?: Y
- Rune literal quotes: `'` ... `'` — Cat: delim — Canonical?: Y
- String literal quotes: `"` ... `"` — Cat: delim — Canonical?: Y
- Comment start: `--` (to end of line) — Cat: comment — Canonical?: Y

## Aggregate Sigils & Forms
- Tuple type: `#(T1,...)` — Cat: aggregate — Canonical?: Y
  - Tuple term: `#{e1,...}`
  - Tuple pattern: `#(p1,...)`
- Struct type decl: `%{ f: T, ... }` — Cat: aggregate — Canonical?: Y
  - Struct term: `Type%{ f = e, ... }`
  - Struct pattern: `%(f: p, ...)`
- Union type decl: `|{ Variant: T?, ... }` — Cat: aggregate — Canonical?: Y
  - Union term: `Type|Variant{e?}`
  - Union pattern: `|Variant(p?)`
- Flags type decl: `&uN{ a, ... }` — Cat: aggregate — Canonical?: Y
  - Flags term: `Type&{ a, ... }`
  - Flags pattern: `&(a, ...)`
- List type: `*[T]` — Cat: aggregate — Canonical?: Y
  - List term: `*{e1, ...}` / `*[T]{}` (empty typed)
  - List pattern: `*(p1, ..., ..rest?)`

## Binding & Declarations
- Const bind: `name = expr` — Cat: operator — Canonical?: Y
- Var bind: `name := expr` — Cat: operator — Canonical?: Y
- Service bind: `name ~= expr` — Cat: operator — Canonical?: Y
- Module header: `[[pkg::ns::leaf]]` — Cat: module — Canonical?: Y
- Export: `<< Symbol` — Cat: module — Canonical?: Y
- Import (module alias): `alias = [[pkg::path]]` — Cat: module — Canonical?: Y

## Services & Protocols
- Service decl introducer: `^|recv|` — Cat: service — Canonical?: Y
- Protocol decl marker: `.{ ... }` — Cat: protocol — Canonical?: Y
- Service implements list: `:Proto (+Proto)*` — Cat: service — Canonical?: Y
- Destructor: `~()` inside service block — Cat: service — Canonical?: Y

## Control Flow & Patterns
- Match introducer: `expr =>` — Cat: control — Canonical?: Y
- Guard: `?(condition)` after pattern — Cat: pattern — Canonical?: Y
- Break: `<@ expr` — Cat: control — Canonical?: Y
- Continue: `@>` — Cat: control — Canonical?: Y
- Loop block: `@{ ... @}` — Cat: control — Canonical?: Y
- Rest pattern (lists): `..` / `..name` — Cat: pattern — Canonical?: Y (list only)
- Wildcard pattern: `_` — Cat: pattern — Canonical?: Y

## Function & Type Annotation
- Parameter annotation: `x :Type` (single space before colon) — Cat: type-anno — Canonical?: Y
- Return type separator: `) Ret` — Cat: type-anno — Canonical?: Y
- Generic constraints (syntax illustration): `[T :Proto + Other]` — Cat: generic — Canonical?: Y

## Builtin Preamble Types
- Optional: `opt[T]` — Cat: builtin — Canonical?: Y
- Result: `res[T]` — Cat: builtin — Canonical?: Y
- Error: `error` — Cat: builtin — Canonical?: Y

## Statement & Whitespace
- Statement separators: newline / `;` — Cat: separator — Canonical?: Y

## Reserved / Planned (Non-canonical)
- Pipes: `/>`, `</` — Cat: sugar-proposal — Status: proposal
- Early unwrap: postfix `!`, `?` — Cat: sugar-proposal — Status: proposal
- Selection helper tokens for concurrency (no special tokens in core) — Cat: draft

## Open Clarifications Needed
- Identifier normalization algorithm
- Generic parameter list grammar outside services
- Exhaustiveness definition for list patterns (in semantics layer)

## Notes
This index is informative. If a conflict arises, accepted core docs take precedence.
