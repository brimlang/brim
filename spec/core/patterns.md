---
id: core.patterns
layer: core
title: Pattern Matching Semantics
authors: ['trippwill', 'assistant']
updated: 2025-01-28
status: accepted
version: 0.1.0
---

# Pattern Matching Semantics

## Overview

Patterns are type-directed constructs used to destructure values and bind names. They appear in match expressions, destructuring binds, and function parameters. Every pattern form corresponds to a type constructor or literal form.

## Pattern Forms Summary

| Pattern | Syntax | Matches | Example |
|---------|--------|---------|---------|
| Wildcard | `_` | Anything (no binding) | `_ => default` |
| Binding | `name` | Anything (binds name) | `x => x + 1` |
| Literal | `42`, `"text"`, `true` | Exact literal value | `0 => "zero"` |
| Tuple | `#(p1, p2, ...)` | Named tuple values | `#(x, y) => x + y` |
| Struct | `%(field = p, ...)` | Struct values | `%(id = i, age = a) => i` |
| Union | `\|(Variant(p?))` | Union variant | `\|(Good(v)) => v` |
| Flags | `&(...)` | Flag set (exact or constrained) | `&(read, write) => ...` |
| Service | `@(name :Proto, ...)` | Service with protocols | `@(auth :Auth) => ...` |
| Sequence | `(p1, p2, ..rest?)` | Sequence elements | `(h, ..t) => h` |
| Option | `?(p?)` | Option value | `?(v) => v` or `?() => nil` |
| Result | `!(p)` or `!!(p)` | Result value | `!(v) => ok` or `!!(e) => err` |

## Core Semantics

### Pattern Matching Process

1. **Scrutinee evaluation:** The matched expression evaluates to a value
2. **Pattern selection:** Arms are tested top-to-bottom
3. **Guard evaluation:** If present, guard must evaluate to `true`
4. **Binding:** Successful match binds pattern variables
5. **Arm execution:** The arm's expression/block executes with bindings in scope

### Exhaustiveness

All match expressions must be exhaustive—every possible value must match at least one arm. The compiler checks exhaustiveness and reports errors for missing cases.

**Common exhaustiveness patterns:**
- Final wildcard arm: `_ => default`
- Final binding arm: `other => handle(other)`
- Complete case coverage for finite types (flags, unions with all variants)

### Guards

Guards refine pattern matches with boolean conditions using the `??` operator:

```brim
value =>
  |(Good(x)) ?? x > 0 => positive_path(x)
  |(Good(x))          => non_positive_path(x)
  |(Error(e))         => error_path(e)
```

**Rules:**
- Guards appear after the pattern, before `=>`
- Guard expressions must have type `bool`
- Failed guards continue to the next arm (not an error)
- Guards are evaluated only if the pattern matches

### Precedence and Binding Scope

- Pattern variables bind for the guard expression and arm body
- Guards are evaluated left-to-right if multiple arms could match
- Inner patterns shadow outer bindings in nested matches

---

## Pattern Forms (Detailed)

### Wildcard Pattern

**Syntax:** `_`

Matches any value without binding a name. Use for ignored values or as exhaustiveness fallback.

```brim
result =>
  |(Good(v)) => v
  _          => default_value
```

### Binding Pattern

**Syntax:** `identifier`

Matches any value and binds it to the identifier for use in the arm body.

```brim
value =>
  x => x * 2
```

**Contextual shorthand for Option/Result:**
When the scrutinee type is `T?` or `T!`, a bare identifier matches the success case:

```brim
opt :i32?
opt =>
  v   => v + 1  -- matches ?(v), binds inner value
  ?() => 0      -- matches nil explicitly

res :str!
res =>
  s      => s         -- matches !(s), binds inner value
  !!(e)  => show(e)   -- matches err explicitly
```

### Literal Pattern

**Syntax:** `42`, `"text"`, `true`, `false`, `'x'`

Matches exact literal values. Uses value equality.

```brim
code =>
  0    => "success"
  404  => "not found"
  500  => "server error"
  _    => "unknown"
```

### Tuple Pattern

**Syntax:** `#(pattern1, pattern2, ...)`

Matches named tuple values. Positional order matters.

```brim
Point := #{i32, i32}
pt :Point = Point#{10, 20}

pt =>
  #(x, y) => x + y
```

**Nested patterns:**
```brim
#(#(a, b), c) => a + b + c
```

### Struct Pattern

**Syntax:** `%(field = pattern, ...)`

Matches struct values. Field order doesn't matter. Fields can be omitted (matches any value for that field).

```brim
User := %{ id :str, age :i32, active :bool }

user =>
  %(id = i, age = a) ?? a >= 18 => adult_path(i)
  %(active = false)              => inactive_path()
  %(id = i, age = a)             => minor_path(i, a)
```

**Shorthand binding:** `%(field1, field2)` binds by field name:
```brim
%(id, age) => id  -- equivalent to %(id = id, age = age)
```

### Union Pattern

**Syntax:** `|(Variant)` or `|(Variant(pattern))`

Matches union variant with optional payload destructuring.

```brim
Reply[T] := |{ Good :T, Error :str }

reply =>
  |(Good(v))  => process(v)
  |(Error(e)) => log(e)
```

**Variants without payload:**
```brim
Status := |{ Pending, Complete, Failed :str }

status =>
  |(Pending)    => "waiting"
  |(Complete)   => "done"
  |(Failed(msg)) => msg
```

### Flags Pattern

**Syntax (exact set):** `&(flag1, flag2, ...)`

Matches exact flag combination. No more, no fewer.

```brim
Perms := &{ read, write, exec }

perms =>
  &(read)              => "read-only"
  &(read, write)       => "read-write"
  &(read, write, exec) => "full"
  &()                  => "none"
```

**Syntax (constrained):** `&(+required, -forbidden, ...)`

Matches flag sets with constraints. Other flags are unconstrained.

```brim
perms =>
  &(+read, +write) => "has read and write (maybe others)"
  &(+exec, -write) => "executable but not writable"
  &(-exec)         => "not executable"
```

**Rules:**
- Exact mode: `&(a, b)` matches if exactly `{a, b}` are set
- Constrained mode: All `+` flags must be present, all `-` flags must be absent
- Cannot mix bare and signed flags: `&(read, +write)` is invalid
- Duplicate or contradictory constraints rejected: `&(+read, -read)` is invalid

### Service Pattern

**Syntax:** `@(name :Protocol, ...)`

Matches service instances and binds protocol-typed handles.

```brim
Auth := .{ login :(str, str) User! }
Logger := .{ write :(str) unit }

svc =>
  @(auth :Auth, log :Logger) => {
    log.write("attempting login")
    auth.login(user, pass)
  }
  @(log :Logger) => {
    log.write("no auth available")
  }
```

**Rules:**
- Each entry binds a name with a protocol type annotation
- Protocol type must be implemented by the service
- Only protocol methods are accessible through the bound name
- Service patterns are the only pattern form that permits `:Type` ascriptions

### Sequence Pattern

**Syntax:** `(pattern1, pattern2, ..., ..rest?)`

Matches sequence elements positionally. Supports rest pattern for remaining elements.

```brim
xs :seq[i32]

xs =>
  ()         => "empty"
  (x)        => "one element"
  (x, y)     => "two elements"
  (h, ..t)   => "head and tail"
  (a, b, ..rest) => "at least two"
```

**Rest pattern:** `..name` or `..` (no binding)
- Captures remaining elements as a `seq[T]`
- Must appear last in the pattern
- `..` alone matches rest without binding

### Option Pattern

**Syntax (explicit):** `?()` (nil), `?(pattern)` (has value)

Matches option values.

```brim
opt :i32?

opt =>
  ?(v) => v + 1
  ?()  => 0
```

### Result Pattern

**Syntax:** `!(pattern)` (ok), `!!(pattern)` (err)

Matches result values.

```brim
res :str!

res =>
  !(s)   => s
  !!(e)  => show_error(e)
```

**Unit result:** `!(unit)` matches `ok(unit)`
```brim
outcome :unit!

outcome =>
  !(unit)    => "success"
  !!(e)  => "failed"
```

---

## Pattern Nesting

Patterns compose recursively. Any pattern position can contain a nested pattern.

**Examples:**

**Nested tuples:**
```brim
pair := Pair#{Pair#{1, 2}, Pair#{3, 4}}
pair =>
  #(#(a, b), #(c, d)) => a + b + c + d
```

**Struct with union field:**
```brim
data =>
  %(status = |(Good(v)), count = n) => process(v, n)
  %(status = |(Error(_)), count = _) => handle_error()
```

**Sequence of structs:**
```brim
users =>
  (%(id = i1), %(id = i2), ..rest) => first_two(i1, i2, rest)
  ()                               => empty_case()
```

---

## Irrefutable Patterns

Some patterns always match (irrefutable). These can be used in destructuring binds outside match expressions.

**Irrefutable patterns:**
- Wildcard: `_`
- Binding: `name`
- Tuple (if all sub-patterns irrefutable): `#(x, y)`
- Struct (if all sub-patterns irrefutable): `%(a, b)`

**Refutable patterns (require match):**
- Literal: `42`
- Union variant: `|(Good(v))`
- Flags: `&(read, write)`
- Option/Result with discriminator: `?(v)`, `!(v)`
- Sequence with fixed length: `(x, y)`

**Destructuring bind example:**
```brim
point := Point#{10, 20}
#(x, y) = point  -- irrefutable, always succeeds

result :i32! = get_value()
!(v) = result    -- ERROR: refutable, requires match
```

---

## Related Specs

- `spec/core/match.md` — Match expressions, arms, and evaluation
- `spec/core/aggregates.md` — Aggregate types and their construction forms
- `spec/core/option_result.md` — Option and Result types, constructors, propagation
- `spec/core/services.md` — Service patterns and protocol binding
- `spec/grammar.md` — Pattern grammar productions