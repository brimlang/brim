---
id: core.expressions
layer: core
title: Expressions
authors: ['trippwill', 'assistant']
updated: 2025-01-28
status: draft
---

# Expressions

## Status & Evolution

This file is `draft` while parameter list mixing rules and future operator surface settle. Existing forms are stable; additions will extend (not break) this grammar.

## Introduction

Everything in Brim evaluates to a value (Core Law #4). Anywhere the language expects an expression you may write either:

- A simple expression (single value form)
- A block expression `{ ... }`

This file provides an overview of expression forms. For complete documentation of specific topics, see:
- **Functions:** `spec/functions.md` (authoritative)
- **Match expressions:** `spec/core/match.md` (authoritative)
- **Patterns:** `spec/core/patterns.md` (authoritative)
- **Constructors:** `spec/core/aggregates.md`, `spec/core/option_result.md`

## Simple Expression Forms

All current value‑producing simple expressions:
- Identifier
- Literal (integer, decimal, string, rune)
- Function (lambda) value: `|params|> expr` or `|params|> { ... }`
- Call: `callee(args)` (callee and each argument are expressions)
- Constructors:
  - Option / Result: `?{}`, `?{x}`, `!{x}`, `!!{e}`
  - Aggregates (see Aggregate Types): `Type%{ field = expr, ... }`, `Type|{ Variant }` or `Type|{ Variant = Expr }`, `Type#{ e1, e2, ... }`, `seq{ e1, e2, ... }`
- Propagation: `expr?`, `expr!` (contextual return propagation)
- Match: `scrutinee => arm+`
- Block: `{ ... }` (compound form listed for completeness)

## Block Expressions

A block executes its statements / nested expressions in order and yields the value of its final expression.

```brim
value = {
  a = 1
  b = 2
  a + b
}
```

## Function Expressions

Function literals use lambda syntax `|params|> body`.

```brim
adder = |x, y|> x + y
inc = |x|> x + 1
fact = |n|> { n => 0 => 1; _ => n * fact(n - 1) }
```

Parameter list forms:
- `||>` empty (no parameters)
- `|x, y|>` identifiers (names only; no types in literals)

**For complete function documentation, see `spec/functions.md`:**
- Function types, value declarations, combined declarations
- Three distinct declaration forms
- Generic function parameters
- Parsing disambiguation rules

## Call Expressions

```brim
result = add(1, 2)
next = fact(result)
obj.method()
```

**Rules:**
- Calls always supply parentheses; arguments are comma‑separated expressions
- Member access uses dot: `expr.member(args?)`
- Member access on literals is not allowed
- Module members must be accessed via an import alias; direct `pkg::ns.member` is disallowed in core

## Constructor Expressions

Each aggregate type has a corresponding constructor syntax using its shape sigil.

**Struct:**
```brim
Point := %{ x :i32, y :i32 }
point = Point%{ x = 10, y = 20 }
```

**Union:**
```brim
Reply[T] := |{ Good :T, Error :str }
ok = Reply|{ Good = 42 }
err = Reply|{ Error = "failed" }
```

**Tuple:**
```brim
Pair := #{i32, str}
pair = Pair#{42, "answer"}
```

**Sequence:**
```brim
nums = seq{1, 2, 3}
```

**Option / Result:**
```brim
none :i32? = ?{}
some :i32? = ?{42}
okv  :i32! = !{42}
fail :i32! = !!{ err_value }
```

**For complete constructor documentation:**
- Aggregates: `spec/core/aggregates.md`
- Option/Result: `spec/core/option_result.md`

## Propagation Expressions

Postfix `?` / `!` propagate Option / Result short‑circuit in a function returning an Option / Result type.

```brim
fetch_sum :(a :i32?, b :i32?) i32? = |a, b|> {
  x = a?  -- if a is nil, return nil immediately
  y = b?  -- if b is nil, return nil immediately
  x + y   -- both values present, compute sum
}
```

**For complete propagation documentation, see `spec/core/option_result.md`.**

## Match Expressions

A match evaluates a scrutinee then selects the first arm whose pattern matches and (if present) whose guard yields `true`.

```brim
value =>
  |(Good(v)) ?? v > 0 => process(v)
  |(Good(_))          => 0
  |(Error(e))         => handle(e)
```

**Basic structure:**
- Introducer: `expr =>` followed by one or more arms
- Arm: `Pattern GuardOpt => ExprOrBlock`
- Guards use `??` operator: `Pattern ?? boolean-expr => ...`
- Must be exhaustive (all cases covered)

**For complete match documentation:**
- Match semantics and evaluation: `spec/core/match.md`
- Pattern forms: `spec/core/patterns.md`
- Match grammar: `spec/grammar.md`

---

## Related Specs

- `spec/functions.md` — Function types, values, and declarations (authoritative)
- `spec/core/match.md` — Match expressions and arms (authoritative)
- `spec/core/patterns.md` — Pattern matching semantics (authoritative)
- `spec/core/aggregates.md` — Aggregate construction forms
- `spec/core/option_result.md` — Option/Result constructors and propagation
- `spec/grammar.md` — Expression grammar productions
