---
id: core.fundamentals
title: Fundamentals
layer: core
authors: ['trippwill']
updated: 2025-09-13
status: accepted
version: 0.1.0
---

# Fundamentals

Brim is an experimental language with a minimal core and an explicit surface.

Brim has a mascot: A platypus named Hinky. Hinky embodies
the aesthetic of Brim: a quirky, highly-adapted animal.
Hinky likes to wear a brimmed hat.

## Language Design Guidelines

What a language contributor should strive for.

1. Brutally consistent
2. Minimal core
3. Explicit surface
4. Extremely limited use of human-language keywords
  - Consistent visual taxonomy eases pattern recognition
  - Encourages small core language
  - Avoids English-centric bias
  - Domain concepts (as identifiers) stand out
  - The ideal would be zero key "words", but impractical for now


## Core Laws

What a new brimmian should know immediately.

1. New declared symbols are always first on the line.
2. Symbols are bound when declared:
  - const with `=`
  - var with `:=`
  - service with `~=`
3. Value semantics (immutable-by-copy):
  - values copy
  - services cannot expose internal state
4. Everything is an expression.
5. Casing is never semantic.
6. No nulls.
7. Aggregates are data:
  - All elements are exported. No private fields.
8. Services are behavior:
  - All methods are exported. No private methods.
  - All state is private. No public fields.


## Modules

Modules are single files with a `.brim` extension.
They are the compilation unit.

```brim
[[acme::io::temp]]
<< TempFile
fs = [[std::fs]]
limit := 0
```

- **Header:** `[[pkg::ns::leaf]]` on line 1. Required.
- **Exports:** `<< Symbol` per line exports the symbol’s entire surface.
  - Only const bound symbols may be exported.
  - There are no implicit exports, and no wildcard exports.
- **Imports:** `alias = [[pkg::ns::path]]` (const binding) anywhere at top level.
  - Re-export is not allowed.
- **State:** top‑level `:=` bindings with literal/aggregate initializers.
- **Shadowing**: const bound symbols cannot be shadowed.
  - var bound symbols may be shadowed in nested scopes.

### Packages

- Packages are collections of modules.

## Binding rules

- `name = expr` → const; rebinding with `=` is error.
- `name := expr` → var; rebinding must use `:=`; using `=` is error.
- `name ~= expr` → bound service; destructor runs at scope end.

## Expressions

An expression produces a value. Anywhere the language expects an expression you may write either:

- A simple expression (single value form)
- A block expression `{ ... }`

Simple expression forms (value-producing):
- Identifier
- Literal (integer, decimal, string, rune)
- Function (lambda) value: `(params) => expr` or `(params) => { ... }`
- Call: `expr(args)` (callee and each argument are expressions)
- Constructors:
  - Option / Result: `?{}`, `?{x}`, `!{x}`, `!!{e}`
  - Aggregates: `Type%{ field = expr, ... }`, `Type|{ Variant }` or `Type|{ Variant = Expr }`, `Type#{ e1, e2, ... }`, `list{ e1, e2, ... }`
- Propagation: `expr?`, `expr!`
- Match: `scrutinee => arm+` (see Match section below for arm form)
- Block: `{ ... }` (listed separately below as the compound form)

Block expressions evaluate their statements left-to-right and yield the value of their final expression.

## Functions

Examples:
```brim
-- Named function with explicit type (const)
add : (i32, i32) i32 = (x, y) => x + y

-- Split header then body (const)
sum : (list[i32]) i32
sum = (xs) => {
  xs =>
    (h, ..t) => h + sum(t)
    ()       => 0
}

-- Split header then var body (allows rebinding)
now : () i64
now := () => host_clock.now()
now := () => 1_700_000_000i64  -- test override

-- Shorthand header with parameter names & types (const only)
inc : (x : i32) i32 = { x + 1 }

-- Generic function
map[T, U] : ((T) U, list[T]) list[U] = (f, xs) => {
  xs =>
    (h, ..t) => std::list:concat(list{ f(h) }, map(f, t))
    ()       => list{}
}
```

Rules:
- Function type shape: `(Type, ...) Ret` (types only).
- Function value: `(name, ...) => expr`.
- Named binding: `name : (Type, ...) Ret = (params) => expr` (const) or `:=` (var).
- Split header form: declare `name : (Type, ...) Ret` then later `name = (params) => ...` or `name := (params) => ...`.
- Only const-bound functions may be exported.
- Parameters are only named in the value, not the type shape (except shorthand header form which binds names directly).

## Match

```brim
r : Reply[i32] = Reply|{ Good = 42 }

r =>
  Good(v) v > 0 => v
  Good(_)       => 0
  Error(e)      => { log(e); -1 }
```

Match syntax:
- Introducer: `expr =>` followed by one or more arms.
- Arm: `Pattern [ guard-expr ] => Expr` (guard is any boolean expression placed after the pattern and before `=>`).
- Patterns are type-directed (no tuple/union sigil repetition, no guard sigil `?(...)`).
- Exhaustive with optional final `_` wildcard.

## Statement Separator
- `\n` and `;` are **statement separators**.

## Comments
- **Form:** `--` to end of line.

### Inline comments
- Permitted as plain `--` after code.

## Runes
- **Literal form:** single-quoted Unicode scalar value.
- **Escapes:**
  - `\n`, `\t`, `\r`, `\'`, `\\`.
  - `\u{HEX}` for Unicode code points (1–6 hex digits).
- **Exactly one scalar** allowed per literal; multiple scalars = compile error.

## Core Types

Primitive & built-in:
- `void` — empty (no inhabitants)
- `unit` — single value
- `bool` — `true`, `false`
- `i8, i16, i32, i64`, `u8, u16, u32, u64`
- `rune` — Unicode scalar
- `str` — UTF‑8 string
- `list[T]` — homogeneous sequence
- Option `T?` / Result `T!`
- `error` — `%{ module: str, domain: str, code: u32 }`

Nominal aggregates (declared elsewhere):
- Named tuples: `Type : #{T1, T2, ...}`
- Structs: `Type : %{ field: Type, ... }`
- Unions:  `Type : |{ Variant: Type?, ... }`
- Flags:   `Type : &uN{ a, b, c }`
- Functions: `Type : (Type, ...) Ret`

Option / Result constructors:
- `?{}` (nil), `?{x}` (has)
- `!{x}` (ok), `!!{e}` (err) where `e : error`
Postfix propagation: `expr?` / `expr!` in matching return contexts.

Core types do not carry methods; helpers live in the standard library.

## See also:
- Aggregate construction & pattern forms: see `spec/core/syntax/aggregates.md` (Aggregate Types).
- Service declarations & protocols: see `spec/core/syntax/services.md` (Services, Protocols, and Constraints).
