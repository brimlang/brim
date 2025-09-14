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
logger:to_string()
io:write("stdout", "hi")
```

Calls always supply parentheses; arguments are comma‑separated expressions.
Member access uses a colon: `expr:member(args?)`. Module members must be accessed via an import alias; direct `pkg::ns:member` is disallowed in core.

## Constructor Expressions
See `Aggregate Types` spec for structural details. For full grammar, see `spec/core/grammar.md` (Expressions, Aggregates, Patterns).

```brim
Reply[T] := |{ Good :T, Error :str }

ok  :Reply[i32] = Reply|{ Good = 42 }
err :Reply[i32] = Reply|{ Error = "nope" }

point = Point%{ x = 1i32, y = 2i32 }
nums  = list{1i32, 2i32, 3i32}
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
    Good(v) ?? v > 0 => v
    Good(_)          => 0
    Error(e)         => { log(e); -1 }
}
```

- Introducer: `expr =>` followed by one or more arms.
- Arm: `Pattern GuardOpt => ExprOrBlock` where a guard, if present, is introduced by `??` immediately after the pattern: `Pattern ?? boolean-expr => ...`.
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
  | CastExpr
  | AscribeExpr
FunctionExpr    ::= "(" ParamList? ")" "=>" (Expr | BlockExpr)
ParamList       ::= Param ("," Param)*
Param           ::= Identifier (":" Type)?
CallExpr        ::= Expr "(" ArgList? ")"
Postfix         ::= Primary ( ":" Identifier ( "(" ArgList? ")" )? )*
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
MatchArm        ::= Pattern GuardOpt "=>" (Expr | BlockExpr)
GuardOpt        ::= /* empty */ | "??" Expr

-- Compile‑time cast/assertion (type‑directed)
CastExpr        ::= Expr ":>" Type
AscribeExpr     ::= ParenExpr ":" Type | ExprAscribeUnambiguous
ExprAscribeUnambiguous ::= Expr ":" Type   -- only when not parseable as member access; see Notes

StmtSep         ::= NEWLINE | ";"
```

Notes:
- `Statement` is syntactically either a binding or expression; bindings appear only inside blocks.
- Precedence is currently trivial: forms are distinguished by leading tokens; there are no chained infix operators yet.
- Future operator introduction will refine `Expr` and precedence rules without invalidating existing forms.
- Statement start prediction inside blocks: Immediately after `{` or any Terminator, the parser is at statement start. If the next significant tokens are `Identifier ':'`, parse a binding header; otherwise parse an expression statement. This keeps `expr:member(...)` unambiguous in expression space while allowing `name :Type = expr`/`.= expr` at line start in any scope.
- Ascription vs member access disambiguation: `expr:member` is member access. To ascribe to a bare named type without additional type tokens, parenthesize the operand: `(expr) : Type`. Ascriptions that begin with a non-member head (e.g., `list[...]`, function type `(A) R`) are unambiguous.

## Examples

  ```brim
  -- Function body is a single expression (block literal)
  double :(i32) i32 = (x) { x + x }
  
  -- Using a block for sequencing
  with_log :(x :i32) i32 = (x) {
    log(x)
    x + 1i32
  }

-- Match inside a block, final expression value
  respond :(r :Reply[i32]) i32 = (r) {
  tmp = r =>
    Good(v) ?? v > 0 => v
    Good(_)          => 0
    Error(e)         => -1
  tmp + 1i32
}

  -- Compile‑time cast/assertion examples
  coerce_widen :(i32) i64 = (x) { x :> i64 }
  
  -- Narrowing requires explicit `:>` and remains compile‑time checked
  coerce_narrow :(i64) i32 = (x) { x :> i32 }
  
  -- Alias/nominal example (semantics deferred; checked at compile time)
  to_user_id :(i64) UserId = (x) { x :> UserId }
  
  -- Header ascription example
  xs :list[i32] = []
  ```

## See Also
- Fundamentals (`spec/core/fundamentals.md`) — core laws & overview
- Aggregate Types (`spec/core/syntax/aggregates.md`) — constructors & patterns
- Numeric Literals (`spec/core/syntax/numeric_literals.md`)
- Services (`spec/core/syntax/services.md`)
