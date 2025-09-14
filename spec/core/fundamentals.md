---
id: core.fundamentals
title: Fundamentals
layer: core
authors: ['trippwill']
updated: 2025-09-14
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

Expressions produce values. Use a simple expression or a block `{ ... }`.
See canonical surface and grammar: `spec/core/syntax/expressions.md`.

## Functions

See core Functions spec: `spec/core/syntax/functions.md`.

## Match

See core Match spec: `spec/core/syntax/match.md`.

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

## Basics Primer

```brim
[[acme::demo]]
<< Main

fs = [[std::fs]]         -- import alias

limit := 10              -- var binding (rebinding with :=)
answer = 42i32           -- const binding

Point : %{ x : i32, y : i32 }
Reply[T] : |{ Good : T, Error : str }

inc : (i32) i32 = (x) => x + 1

main : () i32 = {
  pt = Point%{ x = 1, y = 2 }           -- struct ctor
  ok : Reply[i32] = Reply|{ Good = 5 }  -- union ctor
  ok =>
    Good(v) => inc(v)
}
```

Links: modules/imports above use Fundamentals; constructors/patterns link to Aggregates; functions link to Functions; expressions and match link to their respective specs in Spec Map.

## Spec Map
- Expressions: `spec/core/syntax/expressions.md` — simple forms, constructors, grammar.
- Functions: `spec/core/syntax/functions.md` — types, values, named functions.
- Aggregate Types: `spec/core/syntax/aggregates.md` — structs, unions, named tuples, flags, lists.
- Generics: `spec/core/syntax/generics.md` — parameters, constraints, use sites.
- Option/Result & Propagation: `spec/core/syntax/option_result.md` — `T?`/`T!`, constructors, postfix.
- Services & Protocols: `spec/core/syntax/services.md` — protocols and service declarations.
