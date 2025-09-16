---
id: core.union-plus-and-amp
layer: core
title: Union Sigil `+{` and `&` Separators in Constraints/Implements
authors: ['trippwill', 'assistant']
updated: 2025-09-15
status: draft
---

# Proposal: Union Sigil `+{` and `&` Separators in Constraints/Implements

## Summary

- Change the union type/constructor sigil from `|{` to `+{` to reflect “sum type”.
- Change the separator in generic constraint lists and service implements lists from `+` to `&` to reflect “intersection/both”.

These changes preserve LL(k≤4), maintain greedy compound glyph lexing, and improve semantic coherence of symbols across the language.

## Rationale

- Semantics alignment:
  - Union = sum type → `+{ … }` reads naturally.
  - Constraints/implements express conjunction (“both requirements”) → `&` conveys intersection.
- Future headroom:
  - Frees `|` for later use (e.g., pattern alternation or pipes) without ambiguity with aggregate syntax.
- Lexer compatibility:
  - `+{` lexes as a single compound token (greedy match) and cannot be confused with binary `+` followed by `{`.

## Syntax (Before → After)

Unions (types and constructors)

- Type declaration: `Name := +{ Variant : Type?, ... }` (was `|{`)
- Construction: `Type+{ Variant }` or `Type+{ Variant = Expr }` (was `Type|{ … }`)

Constraints (generic parameters)

- `T: C1 & C2` (was `C1 + C2`)

Service implements list

- `Service :^ recv {} : P & Q[R]` (was `P + Q[R]`)

All other aggregate sigils remain unchanged: `%{` (struct), `#{` (named tuple), `& prim {}` (flags), `.{` (protocol), `^{` (service).

## Lexing

- Add compound glyph `+{` as a single token in the ASCII table.
  - RawKind: `PlusLBrace` (name internal to lexer; parser maps to `UnionToken`).
  - Rule: Longest-match wins; `+{` must be recognized before single `+`.
- Keep single `+` (RawKind.Plus) for binary uses and list separators in legacy contexts only.

## Parsing

- Map `SyntaxKind.UnionToken` to `RawKind.PlusLBrace`.
- TypeExpr first-token dispatch: recognize `+{` as the union opener.
- Prediction tables: any entries keyed on union type heads updated from `|{` → `+{`.
- Union constructor terms: unchanged structurally; continue to require exactly one variant element.
- ConstraintList and ServiceDecl.Implements lists: use `&` as the element separator. Grammar stays LL(k≤4) and table-driven.

## Formal Grammar Deltas (EBNF fragments)

Types and constructors

```ebnf
UnionShape      ::= '+{' VariantTypes? '}'
UnionCtor       ::= TypeName '+{' VariantInit '}'
```

Constraints (within generic parameter lists)

```ebnf
ConstraintList  ::= ':' ConstraintRef ('&' ConstraintRef)*
```

Services (implements list)

```ebnf
ServiceDecl     ::= Ident ':^' Ident '{' '}' (':' ProtoRef ('&' ProtoRef)*)? Terminator
```

## Token Preservation

All tokens remain preserved in-order in the green tree. Delimiters (`+{`, `}`, etc.) are explicit tokens on their owning nodes; list separators (`&`, `,`) are captured as trailing tokens on typed list elements per the project’s token preservation rule.

## Diagnostics

- Legacy union opener:
  - “Legacy union sigil `|{`; use `+{` (sum type).”
- Suspicious spacing for constructors:
  - If the lexer sees `+` followed by `{` with whitespace, the parser may issue: “Unexpected `+`; for union constructor use `+{` with no space.”
- Constraint/implements separators:
  - Where `+` appears in these lists, prefer an advisory diagnostic: “Use `&` for multiple constraints/implements.”

## Examples

```brim
-- Unions (types and construction)
Reply[T] := +{ Good :T, Error :str }
ok :Reply[i32] = Reply+{ Good = 42 }

-- Constraints
Box[T: Clone & Show] := %{ inner :T }

-- Service implements
Svc :^ recv {} : P & Q[R]
SvcT := ^{ P, Q[R] }
```

## Compatibility and Migration

- Recommended path:
  - Provide a pre-parse rewrite that maps `|{` → `+{` and `C1 + C2` / `P + Q` → `C1 & C2` / `P & Q` in the relevant syntactic contexts.
  - Temporarily accept `|{` behind a build flag for experimentation; default to `+{` in specs and tests.
- The tokenizer change is additive (introduces `+{`); the parser mapping flips union recognition to `+{`.

## Acceptance Checklist

- Lexer: `+{` token added; longest-match preserved.
- Parser: `UnionToken` mapped to `+{`; tables updated.
- Constraint/Implements lists: separators are `&`.
- Docs/specs updated across grammar and examples.
- Tests updated: parsing, construction, patterns, token-order regression.
- Diagnostics in place for legacy forms and spacing pitfalls.

