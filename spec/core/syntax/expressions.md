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
  - Aggregates (see Aggregate Types): `Type%{ field = expr, ... }`, `Type|{ Variant }` or `Type|{ Variant = Expr }`, `Type#{ e1, e2, ... }`, `list{ e1, e2, ... }`
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
-- Parameters with names only
adder = (x, y) => x + y

-- Parameters with names and types
inc : (i32) i32 = (x : i32) => x + 1

-- Block body
fact = (n) => {
  n =>
    0 => 1
    _ => n * fact(n - 1i32)
}
```

Parameter list forms allowed today:
- `()` empty
- `(x, y)` identifiers
- `(x : T, y : U)` typed identifiers (mixing typed and untyped allowed for now)

## Call Expressions

```brim
result = add(1i32, 2i32)
next   = fact(result)
```

Calls always supply parentheses; arguments are comma‑separated expressions.

## Constructor Expressions
See `Aggregate Types` spec for structural details.

```brim
Reply[T] : |{ Good : T, Error : str }

ok  : Reply[i32] = Reply|{ Good = 42 }
err : Reply[i32] = Reply|{ Error = "nope" }

point = Point%{ x = 1i32, y = 2i32 }
nums  = list{1i32, 2i32, 3i32}
```

Option / Result:
```brim
none : i32? = ?{}
some : i32? = ?{42}
okv  : i32! = !{42}
fail : i32! = !!{ %{ module = "io", domain = "fs", code = 5u32 } }
```

## Propagation Expressions
Postfix `?` / `!` propagate Option / Result short‑circuit in a position whose implicit return type is an Option / Result.

```brim
fetch_sum : (a : i32?, b : i32?) i32? = (a, b) => {
  x = a?
  y = b?
  x + y
}
```

## Match Expressions
A match evaluates a scrutinee then selects the first arm whose pattern matches and (if present) whose guard expression yields `true`.

```brim
handle : (r : Reply[i32]) i32 = (r) => {
  r =>
    Good(v) v > 0 => v
    Good(_)       => 0
    Error(e)      => { log(e); -1 }
}
```

- Introducer: `expr =>` followed by one or more arms.
- Arm: `Pattern [ guard-expr ] => ExprOrBlock` (guard is any boolean expression placed after the pattern, before the arrow).
- Exhaustive with optional final `_` wildcard.
- Patterns are type‑directed (see Aggregate Types for pattern forms).

## Grammar (Current Surface)
Intentional, minimal, operator‑free (no infix operators yet). This grammar is partial; lexical and pattern grammars are defined elsewhere.

```ebnf
Expr            ::= SimpleExpr | BlockExpr
BlockExpr       ::= "{" BlockBody "}"
BlockBody       ::= (Statement StmtSep+)* Expr?            -- final expression optional
SimpleExpr      ::= Identifier
                  | Literal
                  | FunctionExpr
                  | CallExpr
                  | ConstructorExpr
                  | PropagateExpr
                  | MatchExpr
                  
                  | BlockExpr
FunctionExpr    ::= "(" ParamList? ")" "=>" (Expr | BlockExpr)
ParamList       ::= Param ("," Param)*
Param           ::= Identifier (":" Type)?
CallExpr        ::= Expr "(" ArgList? ")"
ArgList         ::= Expr ("," Expr)*
ConstructorExpr ::= OptionResultCtor
                  | AggregateCtor
                  | UnionVariantCtor
                  | ListCtor
OptionResultCtor::= "?{" Expr? "}" | "!{" Expr "}" | "!!{" Expr "}"
AggregateCtor   ::= TypeName "%{" FieldInits? "}" | TypeName "#{" ExprList? "}"
UnionVariantCtor::= TypeName "|{" VariantName ("=" Expr)? "}"
ListCtor        ::= "list" "{" ExprList? "}"
FieldInits      ::= FieldInit ("," FieldInit)*
FieldInit       ::= Identifier "=" Expr
ExprList        ::= Expr ("," Expr)*
PropagateExpr   ::= Expr "?" | Expr "!"
MatchExpr       ::= Expr "=>" MatchArm+
MatchArm        ::= Pattern Guard? "=>" (Expr | BlockExpr)
Guard           ::= Expr

StmtSep         ::= NEWLINE | ";"
```

Notes:
- `Statement` is syntactically either a binding or expression; bindings appear only inside blocks.
- Precedence is currently trivial: forms are distinguished by leading tokens; there are no chained infix operators yet.
- Future operator introduction will refine `Expr` and precedence rules without invalidating existing forms.

## Examples

```brim
-- Function body is a single expression
double : (i32) i32 = (x) => x + x

-- Using a block for sequencing
with_log : (x : i32) i32 = (x) => {
  log(x)
  x + 1i32
}

-- Match inside a block, final expression value
respond : (r : Reply[i32]) i32 = (r) => {
  tmp = r =>
    Good(v) v > 0 => v
    Good(_)       => 0
    Error(e)      => -1
  tmp + 1i32
}
```

## See Also
- Fundamentals (`spec/core/fundamentals.md`) — core laws & overview
- Aggregate Types (`spec/core/syntax/aggregates.md`) — constructors & patterns
- Numeric Literals (`spec/core/syntax/numeric_literals.md`)
- Services (`spec/core/syntax/services.md`)
