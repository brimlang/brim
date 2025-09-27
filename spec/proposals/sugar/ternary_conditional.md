---
id: sugar.conditional
layer: sugar
title: Ternary Conditional (`?? ::`)
authors: ['assistant']
updated: 2025-09-14
status: proposal
---

# Ternary Conditional (`?? ::`)

Status: S0 sugar proposal. Core remains unchanged; this feature desugars to a canonical `match` with guards and does not introduce new semantics.

## Summary

Introduce a compact conditional expression using the existing guard prefix `??` and a symmetric else delimiter `::`.

Form:
- `CondExpr ::= Expr '??' Expr '::' Expr`

Examples:
- `n > 0 ?? inc(n) :: 0`
- `ok ?? { work() } :: { fallback() }`

## Semantics & Desugaring

Constraints:
- The condition expression must have type `bool`.
- The condition is evaluated exactly once.

Desugaring (informative):
```
cond ?? thenExpr :: elseExpr
```
⇢
```brim
{
  __c = cond
  __c =>
    _ ?? __c => thenExpr
    _          => elseExpr
}
```

Notes:
- Uses a block to ensure `cond` is evaluated once.
- Reuses core guard form `??` for the true-branch selection.

## Grammar (S0)

```
CondExpr ::= Expr '??' Expr '::' Expr
```

## Formatting
- Spaces around `??` and `::` follow normal operator spacing.
- Branch expressions may be simple expressions or blocks; no special casing.

## Alternatives Considered
- Delimited branches without an else token: `cond ?? (then) (else)` — accepted as unambiguous but less readable.
- Else delimiter `:>` — arrow-like, but adds another operator form.
- Reusing `||` — rejected due to conflict with future boolean OR and readability.

