---
id: core.aggregates
layer: core
title: Aggregate Types
authors: ['trippwill']
updated: 2025-09-14
status: accepted
version: 0.1.0
---

# Aggregate Types

Defines the canonical declaration, construction, and pattern forms for aggregates. All aggregates are nominal.

Comma policy: Every comma-delimited list in aggregate declarations, constructions, and patterns permits at most one optional trailing comma (before the closing delimiter). Empty lists contain no comma. All commas are preserved as tokens.

Principle: Construction (term formation) for every aggregate uses the aggregate’s sigil or type head immediately followed by `{ ... }`. Patterns are type-directed and use parentheses.

## Named Tuples — positional aggregates

- **Declaration (type):** `Pair[T, U] := #{T, U}`
- **Construction:** `Pair#{e1, e2}`
- **Pattern:** `(p1, p2)`

```brim
Pair[T, U] := #{T, U}
pair :Pair[i32, i32] = Pair#{1, 2}
pair => (a, b) => a + b
```

## Lists — homogeneous variable‑length aggregates

- **Type:** `list[T]`
- **Construction:** `list{e1, e2, …}` (non‑empty infers `T`)
- **Empty:** `list{}` (requires contextual type) or explicit ascription
- **Pattern:** `(p1, p2, …, ..rest?)` (parentheses; `()` for empty)

Examples:
```brim
xs :list[i32] = list{1, 2, 3}

head_or :(v :list[i32], d :i32) i32 = {
  v =>
    (h)     => h      -- a list with 1 element
    (h, h2) => h
    ()      => d
}

empty  :list[str] = list{}
single :list[str] = list{"hi"}

all    :list[list[i32]] = list{ list{1, 2}, list{3} }
```

## Structs — named aggregates

- **Declaration (type):** `Type := %{ field :Type, ... }`
- **Construction (term):** `Type%{field = expr, ... }`
- **Pattern:** `(field = pat, ...)` (order-insensitive; shorthand `(f1, f2)` binds by field name)

```brim
User := %{ id :str, age :i32 }

mk :(id :str, age :i32) User = {
  User%{ id = id, age = age }
}

show :(u :User) str = {
  u =>
    (id = i, age = a) ?? a > 18 => { "($i), ($a)" }
    (id = _, age = a)           => "underage"
}
```

## Unions — choice aggregates

- **Declaration (type):** `Name := |{ Variant : Type?, ... }`
- **Construction (term):** `Type|{ Variant }` or `Type|{ Variant = Expr }`
- **Pattern:** `Variant(pat?)` (no leading `|` in pattern space)

Parse-time shape: Union constructor terms use the union sigil at the type head followed by a single-brace body containing exactly one variant entry. The body must contain exactly one element; multiple elements or a trailing comma are syntax errors and are rejected by the parser. The single element may be either a bare variant identifier (no payload) — `Type|{ Variant }` — or a variant with an explicit payload expression supplied via `=` — `Type|{ Variant = Expr }`.

The parser enforces this shape syntactically without performing name-binding or type lookup; whether the identifier names a declared variant is validated later during semantic analysis. Enforcing the single-entry shape at parse-time avoids ambiguity with other aggregate list forms and keeps the surface compact.

```brim
Reply[T] := |{ Good :T, Error :str }

emit : () Reply[i32] = Reply|{ Good = 42 }
handle :(r :Reply[i32]) i32 = {
  r =>
    Good(v) ?? v > 0 => v
    Good(_)          => 0
    Error(e)         => -1
}
```

## Flags — bitset aggregates

- **Declaration (type):** `Name := &uN{ a, b, ... }`
- **Construction (term):** `Name&{ a, ... }`
- **Pattern:** Two explicit modes (no bitwise operators):
  - Exact set: `(a, b, ...)` — matches exactly the listed flags (no extras, no missing). `()` matches the empty set.
  - Require/forbid: `(+a, -b, ...)` — all `+` flags must be present; all `-` flags must be absent; others unconstrained. Do not mix bare and signed names.

```brim
Perms := &u8{ read, write, exec }
mask  = Perms&{ read, exec }

check : (p : Perms) bool = {
  p =>
    (read)             => true        -- exact: only 'read'
    (+write, +exec)    => true        -- require: both present (others allowed)
    _                  => false
}
```

Flags are fixed enumerated bitsets chosen for efficient embedding in numeric fields.
Patterns do not use bitwise operators. Do not mix bare and signed names; duplicate names or contradictory constraints like `(+read, -read)` are rejected.
