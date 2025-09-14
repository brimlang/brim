---
id: core.functions
layer: core
title: Functions
authors: ['trippwill', 'assistant']
updated: 2025-09-14
status: accepted
version: 0.1.0
---

# Functions

Defines the canonical forms for function types and values, and the allowed named function declarations and bindings.

Principle: Functions are first-class values. A function type is written with parameter types in parentheses followed by a return type. A function value is written with a parameter list and an arrow to either a simple expression or a block expression.

## Function Types & Values

- Function type shape: `(Type, ...) Ret` (types only).
- Function value: `(params) => expr` or `(params) => { ... }`.

Parameter list forms allowed today:
- `()` empty
- `(x, y)` identifiers
- `(x : T, y : U)` typed identifiers (mixing typed and untyped allowed for now)

## Named Functions

Named functions are declared via const (`=`) or var (`:=`) binding. Only const-bound functions may be exported.

Examples:
```brim
-- Named function with explicit type (const)
add : (i32, i32) i32 = (x, y) => x + 1

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
- Named binding: `name : (Type, ...) Ret = (params) => expr` (const) or `:=` (var).
- Split header form: declare `name : (Type, ...) Ret` then later bind with `=` or `:=` to a function value.
- Only const-bound functions may be exported.
- Parameters are only named in the value, not the type shape (except shorthand header form which binds names directly).

