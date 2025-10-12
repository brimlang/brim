---
id: sugar.list-literals
layer: sugar
title: Bracket Sequence Literals (`[ ... ]`)
authors: ['assistant']
updated: 2025-09-14
status: proposal
---

# Bracket Sequence Literals (`[ ... ]`)

Status: S0 sugar proposal. Core remains unchanged; this syntax desugars to the canonical `seq{ ... }` constructor and introduces no new semantics.

## Summary

Introduce a compact `seq` literal form in term space:

- Form: `[e1, e2, ...]`
- Desugars to: `seq{ e1, e2, ... }`

Empty sequences:
- `[]` requires an expected type `seq[T]` or a binding header ascription: `xs :seq[i32] = []`.

## Rationale

- Improves readability for common sequence construction while preserving the core’s sigil+brace pattern (`seq{ ... }`).
- Avoids confusion with types: bracket sequence literals live only in term space; type generics already use brackets in type space `seq[T]`.
- Respects global comma policy (optional single trailing comma; empty lists contain none).

## Grammar (S0)

```
SeqLiteralSugar ::= '[' Elements? ']'
Elements         ::= Expr (',' Expr)* (',')?
```

## Desugaring

```
[e1, e2, ..., en]  ⇢  seq{ e1, e2, ..., en }
[]                 ⇢  seq{}   -- requires expected type or binding ascription
```

## Notes

- This sugar does not add indexing, slicing, or any other bracket-based operators.
- `[[` continues to denote module headers; this sugar uses single brackets only in term space.
- Trailing comma rules follow the global policy; at most one optional trailing comma is permitted.
