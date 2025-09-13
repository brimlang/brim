---
id: reference.tokens
status: draft
title: Token & Surface Reference (Non-Canonical)
layer: core
updated: 2025-09-13
authors: ['trippwill','assistant']
---

# Token & Surface Reference (Non-Canonical)

Status: draft (informative). Canonical meaning is defined only in accepted core specs (fundamentals, aggregates, services, generics, option-result). This file is an index / quick lookup.

## Legend
- Cat: category (lexical, delim, operator, aggregate, control, pattern, type, module, service, protocol, builtin, generic, comment)
- Canonical?: Y (in accepted core), P (proposal), D (draft / reserved)

## Core Lexical Tokens
- Identifiers (Unicode; normalization TBD) — Cat: lexical — Canonical?: Y
- Integer literals (with optional type suffix) — Cat: literal — Canonical?: Y
- Rune literal quotes: `'` … `'` — Cat: delim — Canonical?: Y
- String literal quotes: `"` … `"` — Cat: delim — Canonical?: Y
- Comment line start: `--` (to end of line) — Cat: comment — Canonical?: Y

## Aggregate Declarations & Forms
(All aggregates are nominal; no anonymous tuple or anonymous structural list types.)

- Named tuple type decl: `Type : #{T1, T2, ...}` — Cat: aggregate — Canonical?: Y
  - Construction: `Type#{e1, e2, ...}`
  - Pattern: `Type#(p1, p2, ...)`
- Struct type decl: `Type : %{ field: Type, ... }` — Cat: aggregate — Canonical?: Y
  - Construction: `Type%{ field = expr, ... }`
  - Pattern: `(field = pat, ...)` (order-insensitive; shorthand `(field1, field2)` binds by name)
- Union type decl: `Type : |{ Variant: Type?, ... }` — Cat: aggregate — Canonical?: Y
  - Construction: `Type|Variant{expr?}`
  - Pattern: `Variant(p?)` (no leading sigil in pattern space)
- Flags type decl: `Type : &uN{ a, b, ... }` — Cat: aggregate — Canonical?: Y
  - Construction: `Type&{ a, b }`
  - Pattern: flag algebra inside parens: `(read & write)`, `(+write & -exec)`, `(read | write)`
- List type: `list[T]` — Cat: aggregate — Canonical?: Y
  - Construction: `list{e1, e2, ...}` / `list{}` (empty needs context or ascription)
  - Pattern: `(p1, p2, ..., ..rest?)` / `()` for empty (rest optional)

## Option / Result Types & Constructors
- Option type: `T?` — Cat: builtin — Canonical?: Y
  - Constructors: `?{}` (nil), `?{x}` (has)
  - Propagation operator (postfix): `expr?`
  - Patterns: `?()` (nil), `?(v)` or just `v` (has)
- Result type: `T!` — Cat: builtin — Canonical?: Y
  - Constructors: `!{x}` (ok), `!!{e}` (err; `e : error`)
  - Propagation operator (postfix): `expr!`
  - Patterns: `!(v)` or just `v` (ok), `!!(e)` (err)

## Binding & Declarations
- Const bind: `name = expr` — Cat: operator — Canonical?: Y
- Var bind: `name := expr` — Cat: operator — Canonical?: Y
- Service (lifecycle) bind: `name ~= expr` — Cat: operator — Canonical?: Y
- Split function header: `name : (Type, ...) Ret` then `name = value` or `name := value` — Cat: binding — Canonical?: Y
- Combined function declaration & definition (shorthand with parameter names & types): `name : (x :T, y :U) Ret = { ... }` — Cat: binding — Canonical?: Y
- Generic parameter list (after symbol): `Name[T, U]` or `Name[T :Proto + Other]` — Cat: generic — Canonical?: Y

## Function Forms
- Function type: `(Type, ...) Ret` — Cat: type — Canonical?: Y
- Function value: `(x, y, ...) => expr` or block body — Cat: value — Canonical?: Y
- Named function (const): `f : (A, B) C = (x, y) => expr` — Cat: binding — Canonical?: Y
- Named function (var):  `f : (A, B) C := (x, y) => expr` — Cat: binding — Canonical?: Y

## Services & Protocols
- Protocol decl: `Proto[T?] : .{ method : (Type, ...) Ret, ... }` — Cat: protocol — Canonical?: Y
- Service decl (nominal): `Service : ^recv{ state : Type, ... } : Proto (+Proto)* = { members }` — Cat: service — Canonical?: Y
  - Constructor inside service: `^() = { ... }`
  - Method inside service: `name : (Type, ...) Ret = (params) => expr` (or block)
  - Destructor: `~() unit = { ... }`

## Control Flow
- Match introducer: `expr => { ... }` (braces optional for single arm sets) — Cat: control — Canonical?: Y

## Patterns (Type-Directed)
- Wildcard: `_` — Cat: pattern — Canonical?: Y
- List: `(h, ..t)`, `(h)`, `()` — Cat: pattern — Canonical?: Y
- Struct: `(field = pat, ...)` / `(field1, field2)` shorthand — Cat: pattern — Canonical?: Y
- Named tuple: `Type#(p1, p2, ...)` — Cat: pattern — Canonical?: Y
- Union variant: `Variant(p?)` — Cat: pattern — Canonical?: Y
- Flags: `(flag & other)`, `(+flag & -other)` — Cat: pattern — Canonical?: Y
- Option: `?()` / `?(v)` / `v` — Cat: pattern — Canonical?: Y
- Result: `!!(e)` / `!(v)` / `v` — Cat: pattern — Canonical?: Y
- Rest list pattern: `..` / `..name` (list only) — Cat: pattern — Canonical?: Y
- Guard form: `Pattern boolean-expr =>` (guard is any bool expression placed after pattern) — Cat: pattern — Canonical?: Y

## Builtin & Primitive Types
- `void`, `unit`, `bool`, `error`, `list[T]`, `T?`, `T!` — Cat: builtin — Canonical?: Y

## Module System
- Module header (must be first line): `[[pkg::ns::leaf]]` — Cat: module — Canonical?: Y
- Export: `<< Symbol` — Cat: module — Canonical?: Y
- Import alias: `alias = [[pkg::ns::path]]` — Cat: module — Canonical?: Y

## Statement & Whitespace
- Statement separators: newline or `;` — Cat: separator — Canonical?: Y

## Reserved / Future (Non-Canonical)
- Backtick `` ` `` (macro / quoting future) — Cat: reserved — Canonical?: D
- Leading `.` ident forms outside protocol decls — Cat: reserved — Canonical?: D
- Triple colon `:::` — Cat: reserved — Canonical?: D
- Pipes: `/>`, `</` — Cat: sugar-proposal — Canonical?: P
- Attribute prefix: `@ident` — Cat: reserved — Canonical?: D

## Removed / Historical (For Migration Only)
- Anonymous tuple types/patterns: `#(T, ...)`, `#(p, ...)`
- Anonymous tuple terms: `#{...}`
- Star list syntax: `*[T]`, `*{...}`, `*(...)`
- Guard sigil wrapper: `?(cond)`
- Union pattern sigil: `|Variant(p)`
- Old function inline form: `f = (x :T) U { ... }`

## Open Clarifications
- Identifier normalization algorithm
- Exhaustiveness & rest semantics for list patterns
- Flag pattern algebra formal typing & precedence write-up
- Service declaration final grammar (multi-protocol ordering)

## Notes
This index is informative. If conflicts arise, the accepted core specs override this list.
