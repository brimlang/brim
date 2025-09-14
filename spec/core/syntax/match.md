---
id: core.match
layer: core
title: Match Expressions
authors: ['trippwill', 'assistant']
updated: 2025-09-14
status: accepted
version: 0.1.0
---

# Match Expressions

A match evaluates a scrutinee then selects the first arm whose pattern matches and (if present) whose guard (introduced with `??`) yields `true`.

## Form

- Introducer: `expr =>` followed by one or more arms.
- Arm: `Pattern GuardOpt => ExprOrBlock` where `GuardOpt` is either empty or a `??` followed by a boolean expression.
- Exhaustive with optional final `_` wildcard.
- Patterns are type‑directed (see Aggregate Types and Option/Result for pattern forms).

## Example

```brim
Reply[T] := |{ Good :T, Error :str }

handle :(r :Reply[i32]) i32 = (r) => {
  r =>
    Good(v) ?? v > 0 => v
    Good(_)          => 0
    Error(e)         => { log(e); -1 }
}
```

## Notes

- Union variant patterns omit the leading `|` and use parentheses: `Variant(p?)`.
- Struct patterns bind by name: `(field = pat, ...)` with order-insensitivity; shorthand `(f1, f2)` binds by field name.
- List patterns use parentheses and support rest: `(h, ..t)`, `(h)`, `()`.
- Option/Result patterns are available; see the Option/Result spec.
