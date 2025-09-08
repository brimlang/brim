---
id: core.fundamentals
title: Fundamentals
layer : core
authors: ['trippwill']
updated: 2025-09-08
status: accepted
version: 0.1.0
---

# Fundamentals

Brim is an experimental language with a minimal core and an explicit surface.

Brim has a mascot: A platypus named Hinky. Hinky embodies
the aesthetic of Brim: a unique, somewhat quirky, but
highly-adapted animal. Hinky likes to wear a brimmed hat.

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
  - There are no implicit exports, nor wildcard exports.
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

## Functions

```brim
add = (x :i32, y :i32) i32 { x + y }
```

- Functions are first-class values.
- Functions can only const bind with `=`. Return type follows `)` with one space: `(p :T) Ret`.
- Functions may nest inside other functions.

## Match

```brim
r := res[i32]|ok{42}

r =>
|ok(v) ?(v > 0) => v
|ok(_)          => 0
|err(e)         => { log(e); -1 }
```

- Introducer: `expr =>`
- **Arm:** `pattern ?(guard)? => expr`
- Matches are exhaustive with optional final `_`.

## Loops

```brim
acc := 0u8
@{
  acc += 1
  ?(acc > 10) <@ acc -- break returning acc
  @> -- continue
@}

- Block: `@{ ... @}`
- Break: `<@ expr`
- Continue: `@>`
```

- Loops are expressions that evaluate to the last evaluated expression or the value of a break.
- Only one type of loop: infinite with internal control.

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

- `bool`
- `i8, i16, i32, i64`, `u8, u16, u32, u64`
- `rune` — Unicode scalar
- `str` — UTF‑8 string
- Tuples    `#(T1, T2, …)`
- Structs   `%{ field: Type, ... }`
- Unions    `|{ Variant: Type?, ... }`
- Flags     `&uN{ a, b, c }`
- Lists     `*[T]`
- Functions `(p :Type, ...) Ret`

### Preamble Defined (global types)

Predefined aggregate types available without import.

Generic unions:

- `opt[T]` — union (`has`, `nil`)
- `res[T]` — union (`ok`, `err`)

Single structural error type:
- `error` — `{ module: str, domain: str, code: u32 }`

Core types do not carry methods; helpers live in the standard library.

## See also:
- Aggregate construction & pattern forms: see `spec/core/syntax/aggregates.md` (Aggregate Types).
- Service declarations & interfaces: see `spec/core/syntax/services.md` (Services, Interfaces, and Constraints).

