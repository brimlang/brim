---
id: core.aggregates
version: 0.1.0
layer : core
title: Aggregate Types
authors: ['trippwill']
updated: 2025-09-08
status: accepted
---

# Aggregate Types

Defines the canonical declaration, construction, and pattern forms for aggregates.

Principle: Construction (term formation) for every aggregate always uses the aggregate’s sigil immediately followed by `{ ... }`, while patterns always use the same sigil followed by `( ... )`.

## Tuples — positional aggregates

- **Type:** `#(T1, T2, …)`
- **Construction:** `#{e1, e2, …}`
- **1‑tuple:** `#{e}` — unambiguous because of the `#` sigil
- **Pattern:** `#(p1, p2, …)`

```brim
pair : #(i32, i32) = #{1, 2}
one  : #(i32)      = #{42}

pair => #(a, b) => a + b
```

## Lists — homogeneous variable‑length aggregates

- **Type:** `*[T]`
- **Construction:** `*{e1, e2, …}` (non‑empty infers `T`)
- **Empty:** `*{}` (requires contextual type) or explicit `*[T]{}`
- **Pattern:** `*(p1, p2, …)` (optional trailing rest `..` or `..name` reserved; rest semantics deferred)

Examples:
```brim
xs : *[i32] = *{1, 2, 3}

head_or = (v :*[i32], d :i32) i32 {
  v => *(h)      => h      -- a list with 1 element
       *(h, h2)  => h
       *()       => d
}

empty  : *[str] = *{}
single : *[str] = *{"hi"}

all    : *[*[i32]] = *{ *{1, 2}, *{3} }  -- list of lists
```

The star sigil evokes repetition (Kleene star) for homogeneous sequences; brackets distinguish the single element type from tuple positional typing.

## Structs — named aggregates

- **Declaration (type):** `Type = %{ field :Type, ... }`
- **Construction (term):** `Type%{field = expr, ... }`
- **Pattern:** `%(field: pat, ...)`

```brim
User = %{ id: str, age: i32 }

mk = (id :str, age :i32) User {
  User%{ id = id, age = age }
}

show = (u :User) str {
  u =>
  %(id: i, age: a) ?(a > 18)  => { "($i), ($a)" } -- interpolation is not specified
  %(id: _, age: a)            => "underage"
}
```

## Unions — choice aggregates

- **Declaration (type):** `Name = |{ Variant: Type?, ... }`
- **Construction (term):** `Name|Variant{expr?}`
- **Pattern:** `|Variant(pat?)`

```brim
Reply[T] = |{ Good: T, Error: str }

emit = () Reply[i32] { Reply|Good{42} }
handle = (r :Reply[i32]) i32 {
  r =>
  |Good(v)  => v
  |Error(e) => -1
}
```

Variants are not constructors, but types. Empty variants are permitted, but must be explicitly constructed with `{}`.

## Flags — bitset aggregates

- **Declaration (type):** `Name = &uN{ a, b, ... }`
- **Construction (term):** `Name&{ a, ... }`
- **Pattern:** `&(a, ...)` (presence checks)

```brim
Perms = &u8{ read, write, exec }
mask  = Perms&{ read, exec }

check = (p :Perms) bool {
  p => &(read) => true
       _             => false
}
```

Flags are fixed enumerated bitsets chosen for efficient embedding in numeric fields.
