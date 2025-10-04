---
id: core.expressions
layer: core
title: Expressions
authors: ['trippwill', 'assistant']
updated: 2025-09-14
status: draft
---

# Expressions

## Status & Evolution

This file is `draft` while parameter list mixing rules and future operator surface settle. Existing forms are stable; additions will extend (not break) this grammar.

## Introduction

Everything in Brim evaluates to a value (Core Law #4). Anywhere the language expects an expression you may write either:

- A simple expression (single value form)
- A block expression `{ ... }`

## Simple Expression Forms

All current value‑producing simple expressions:
- Identifier
- Literal (integer, decimal, string, rune)
- Function (lambda) value: `(params) => expr` or `(params) => { ... }`
- Call: `callee(args)` (callee and each argument are expressions)
- Constructors:
  - Option / Result: `?{}`, `?{x}`, `!{x}`, `!!{e}`
  - Aggregates (see Aggregate Types): `Type%{ field = expr, ... }`, `Type|{ Variant }` or `Type|{ Variant = Expr }`, `Type#{ e1, e2, ... }`, `seq{ e1, e2, ... }`, `buf[T; N]{ e1, … }`
- Propagation: `expr?`, `expr!` (contextual return propagation)
- Match: `scrutinee => arm+`
- Block: `{ ... }` (compound form listed for completeness)

## Block Expressions
A block executes its statements / nested expressions in order and yields the value of its final expression.

```brim
value = {
  a = 1i32
  b = 2i32
  a + b
}
```

## Function Expressions

```brim
-- Parameters (names only) and block body
adder = (x, y) { x + y }

-- Named function with header type
inc :(i32) i32 = (x) { x + 1 }

-- Block body
fact = (n) {
  n =>
    0 => 1
    _ => n * fact(n - 1i32)
}
```

Parameter list forms allowed today in function literals:
- `()` empty
- `(x, y)` identifiers (names only; no types in literals)

## Call Expressions

```brim
io ::= std::io          -- import required for term-space access

result = add(1i32, 2i32)
next   = fact(result)
logger.to_string()
io.write("stdout", "hi")
```

Calls always supply parentheses; arguments are comma‑separated expressions.
Member access uses a dot: `expr.member(args?)`. Member access on literals is not allowed. Module members must be accessed via an import alias; direct `pkg::ns.member` is disallowed in core.

## Constructor Expressions
See `Aggregate Types` spec for structural details. For full grammar, see `spec/grammar.md` (Expressions, Aggregates, Patterns).

```brim
Reply[T] := |{ Good :T, Error :str }

ok  :Reply[i32] = Reply|{ Good = 42 }
err :Reply[i32] = Reply|{ Error = "nope" }

point = Point%{ x = 1i32, y = 2i32 }
nums  = seq{1i32, 2i32, 3i32}
```

Option / Result:
```brim
none :i32? = ?{}
some :i32? = ?{42}
okv  :i32! = !{42}
fail :i32! = !!{ %{ module = "io", domain = "fs", code = 5u32 } }
```

## Propagation Expressions
Postfix `?` / `!` propagate Option / Result short‑circuit in a position whose implicit return type is an Option / Result.

```brim
fetch_sum :(a :i32?, b :i32?) i32? = (a, b) {
  x = a?
  y = b?
  x + y
}
```

## Match Expressions
A match evaluates a scrutinee then selects the first arm whose pattern matches and (if present) whose guard expression (prefixed with `??`) yields `true`.

```brim
handle :(r :Reply[i32]) i32 = (r) {
  r =>
    |(Good(v)) ?? v > 0 => v
    |(Good(_))          => 0
    |(Error(e))         => { log(e); -1 }
}
```

- Introducer: `expr =>` followed by one or more arms.
- Arm: `Pattern GuardOpt => ExprOrBlock` where a guard, if present, is introduced by `??` immediately after the pattern: `Pattern ?? boolean-expr => ...`.
- Exhaustive with optional final `_` wildcard.
- Patterns are type‑directed (see Aggregate Types for pattern forms).
