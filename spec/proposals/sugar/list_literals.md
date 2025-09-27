---
id: sugar.list-literals
layer: sugar
title: Bracket List Literals (`[ ... ]`)
authors: ['assistant']
updated: 2025-09-14
status: proposal
---

# Bracket List Literals (`[ ... ]`)

Status: S0 sugar proposal. Core remains unchanged; this syntax desugars to the canonical `list{ ... }` constructor and introduces no new semantics.

## Summary

Introduce a compact list literal form in term space:

- Form: `[e1, e2, ...]`
- Desugars to: `list{ e1, e2, ... }`

Empty lists:
- `[]` requires an expected type `list[T]` or a binding header ascription: `xs :list[i32] = []`.

## Rationale

- Improves readability for common list construction while preserving the core’s sigil+brace pattern (`list{ ... }`).
- Avoids confusion with types: bracket list literals live only in term space; type generics already use brackets in type space `list[T]`.
- Respects global comma policy (optional single trailing comma; empty lists contain none).

## Grammar (S0)

```
ListLiteralSugar ::= '[' Elements? ']'
Elements         ::= Expr (',' Expr)* (',')?
```

## Desugaring

```
[e1, e2, ..., en]  ⇢  list{ e1, e2, ..., en }
[]                 ⇢  list{}   -- requires expected type or binding ascription
```

## Notes

- This sugar does not add indexing, slicing, or any other bracket-based operators.
- `[[` continues to denote module headers; this sugar uses single brackets only in term space.
- Trailing comma rules follow the global policy; at most one optional trailing comma is permitted.
