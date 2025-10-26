---
id: core.match
layer: core
title: Match Expressions
authors: ['trippwill', 'assistant']
updated: 2025-01-28
status: accepted
version: 0.1.0
---

# Match Expressions

## Overview

A match expression evaluates a scrutinee value then selects the first arm whose pattern matches and (if present) whose guard expression yields `true`. Match expressions are the primary way to destructure values and handle different cases.

## Syntax

**Form:**
```
scrutinee =>
  Pattern1 GuardOpt? => Expression1
  Pattern2 GuardOpt? => Expression2
  ...
```

**Components:**
- **Introducer:** `expr =>` followed by one or more arms
- **Arm:** `Pattern GuardOpt => ExprOrBlock`
- **Guard:** Optional `??` followed by a boolean expression
- **Body:** Expression or block that executes when the arm matches

## Basic Examples

### Simple Matching

```brim
status =>
  0   => "success"
  404 => "not found"
  _   => "other"
```

### Union Matching

```brim
Reply[T] := |{ Good :T, Error :str }

handle :(r :Reply[i32]) i32 = |r|> {
  r =>
    |(Good(v)) => v
    |(Error(e)) => { log(e); -1 }
}
```

### With Guards

```brim
value =>
  |(Good(v)) ?? v > 0 => process(v)
  |(Good(v)) ?? v == 0 => handle_zero()
  |(Good(_)) => handle_negative()
  |(Error(e)) => handle_error(e)
```

## Evaluation Semantics

1. **Scrutinee evaluation:** The matched expression evaluates to a value once
2. **Top-to-bottom selection:** Arms are tested in order from first to last
3. **Pattern matching:** Each arm's pattern is tested against the value
4. **Guard evaluation:** If the pattern matches and a guard exists, the guard is evaluated
5. **Arm execution:** The first arm with matching pattern and successful guard (or no guard) executes
6. **Result:** The match expression yields the value of the executed arm's body

### Guard Behavior

Guards refine pattern matches with boolean conditions:

```brim
user =>
  %(age = a) ?? a >= 18 => adult_path()
  %(age = _) => minor_path()
```

**Rules:**
- Guards appear after the pattern, before `=>`
- Guard expressions must have type `bool`
- Failed guards continue to the next arm (not an error)
- Guards are evaluated only if the pattern matches
- Multiple arms can have the same pattern with different guards

## Exhaustiveness

All match expressions must be exhaustive—every possible value of the scrutinee type must match at least one arm. The compiler checks exhaustiveness and reports errors for missing cases.

**Common exhaustiveness patterns:**

**Wildcard fallback:**
```brim
value =>
  |(Good(v)) => v
  _ => default_value
```

**Binding fallback:**
```brim
value =>
  0 => special_case()
  n => general_case(n)
```

**Complete case coverage:**
```brim
bool_val =>
  true => "yes"
  false => "no"
```

## Pattern Forms Quick Reference

Match expressions support all pattern forms. For complete pattern documentation, see `spec/core/patterns.md`.

**Common patterns in match:**
- **Wildcard:** `_` — matches anything, no binding
- **Binding:** `name` — matches anything, binds value
- **Literal:** `42`, `"text"`, `true` — matches exact value
- **Union:** `|(Variant(p?))` — matches variant with optional payload
- **Struct:** `%(field = p, ...)` — matches struct, binds fields
- **Tuple:** `#(p1, p2)` — matches tuple positionally
- **Sequence:** `(h, ..t)` — matches sequence with head and tail
- **Option:** `?(v)` or `?()` — matches has/nil
- **Result:** `!(v)` or `!!(e)` — matches ok/err
- **Flags:** `&(...)` — matches flag sets (exact or constrained)
- **Service:** `@(name :Proto)` — matches service with protocol binding

## Advanced Examples

### Nested Patterns

```brim
pair =>
  #(#(a, b), #(c, d)) => a + b + c + d
  _ => 0
```

### Struct with Guards

```brim
user =>
  %(id = i, age = a) ?? a >= 18 => adult(i, a)
  %(id = i, age = a) ?? a >= 13 => teen(i, a)
  %(id = i, age = a) => child(i, a)
```

### Sequence Patterns

```brim
xs =>
  ()         => "empty"
  (x)        => single(x)
  (x, y)     => pair(x, y)
  (h, ..t)   => cons(h, process(t))
```

### Service Patterns

```brim
svc =>
  @(auth :Auth, log :Logger) => {
    log.write("login attempt")
    auth.login(user, pass)
  }
  @(log :Logger) => {
    log.write("no auth")
  }
  _ => unit{}
```

## Notes

- Match arms are evaluated sequentially; order matters when guards are present
- Patterns bind variables that are in scope for both the guard and the arm body
- Nested matches create new binding scopes; inner bindings shadow outer ones
- The scrutinee is evaluated exactly once, regardless of the number of arms
- Match expressions always yield a value (the result of the matched arm)

---

## Related Specs

- `spec/core/patterns.md` — Pattern matching semantics (authoritative for patterns)
- `spec/core/aggregates.md` — Aggregate construction and pattern forms
- `spec/core/option_result.md` — Option/Result patterns
- `spec/core/services.md` — Service patterns and protocol binding
- `spec/core/expressions.md` — Expression forms overview
- `spec/grammar.md` — Match expression grammar productions
