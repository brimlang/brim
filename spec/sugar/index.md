---
id: sugar.index
layer: sugar
title: S0 Surface Index
authors: ['assistant']
updated: 2025-09-14
status: draft
---

# S0 Surface Index

This index lists non-semantic sugars that desugar to core forms.

## Proposals
- Bracket List Literals: `[e1, e2, ...]` ⇒ `list{ e1, e2, ... }`
- Pipes: forward `/>` and reverse `</` (see proposal)
- Ternary (guard-based): `cond ?? then :: else` ⇒ match with guard (see proposal)

Notes
- Sugars carry no new semantics in core; they must desugar into existing core constructs.
- Acceptance of a sugar proposal does not affect LL(k≤4) guarantees.
