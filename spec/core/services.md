---
id: core.services
title: Services, Protocols, and Constraints
layer: core
authors: ['trippwill', 'assistant']
updated: 2025-01-28
status: accepted
version: 0.1.0
---

# Services, Protocols, and Constraints

## Overview

This document defines services (stateful behavioral handles), protocols (interface specifications), and generic constraints. For pattern matching with services, see `spec/core/patterns.md`. For service patterns in match expressions, see `spec/core/match.md`.

## Services

Services are nominal handles that combine private state with protocol implementations. Each service declaration names its state layout explicitly; behavior is supplied through a lifecycle block plus one or more protocol method blocks.

### Type declaration (state layout)
- **Form:** `Service[T?] := @{ Field (',' Field)* (',')? }`
- `Field ::= Ident ':' TypeExpr`
- Every field is instance-private and currently mutable. For immutable values, lift them to module-level bindings and reference them from the service.

Example:
```brim
AuthService[T] := @{
  foo :u32,
  bar :bool,
  xtra :T,
}
```

### Lifecycle block
- **Form:** `Service[T?] { LifecycleMember* }`
- Members:
  - Constructor: `(ParamList) @! FunctionBody`
  - Destructor:  `~(alias :@) unit FunctionBody`
- Exactly one constructor is required; at most one destructor may appear. Constructors are always fallible (`@!`) and must return exactly one `@{ ... }` literal on every successful path, covering each declared field once. Destructors are infallible and run whenever construction succeeded; the bound alias gives read/write access to the service handle during teardown.
- Constructors operate before any handle exists—they can only compute and return the literal. Destructors receive the concrete handle as their parameter; no other code can observe the handle until construction completes.

### Protocol method blocks
- **Form:** `Service[T?]<ProtoList>(alias :@) { MethodDecl* }` or `Service[T?]<ProtoList> { MethodDecl* }` for no receiver.
- `ProtoList ::= ProtoRef (',' ProtoRef)*` and the angle brackets are mandatory; use `< >` for helper blocks that publish no protocol. `ProtoRef` is any protocol type reference (`Ident` plus optional generic arguments).
- Each method follows the ordinary declaration form `name :(ParamTypes) ReturnType FunctionBody`, and may read or write state through the aliased handle (`alias.field`).
- Multiple blocks may target distinct protocol sets for the same service. Helper blocks with `< >` expose internals only to code holding the concrete service handle; protocol-typed callers never see those members.
- Method names across all blocks must be unique for a given service type.

### State access and invariants
- State fields can only be accessed through the aliased handle inside lifecycle and method blocks; there is no dotless access and no visibility outside these blocks.
- Since fields are mutable, method code must re-establish invariants after every write. The spec may later add field-level immutability; until then, prefer module-level constants for read-only data.
- The compiler verifies that constructor success paths match the declared field list; missing or duplicate assignments are rejected.

### Example
```brim
AuthService[T] := @{
  foo  :u32,
  bar  :bool,
  xtra :T,
}

AuthService[T] {
  (a :u32, b :bool, c :T) @! {
    start(c) =>
      !(_)  => @{
        foo  = a,
        bar  = b,
        xtra = c,
      }
      !!(e) => !!{ e }
  }

  ~(svc :@) unit {
    started(svc.xtra) =>
      true  => stop(svc.xtra)
      false => unit
  }
}

AuthService[T]<Auth>(svc :@) {
  login :(user :str, pass :str) User! {
    svc.bar = !svc.bar
    std::atoi(pass) == svc.foo =>
      true  => User%{
        id   = user,
        auth = svc.bar,
      }
      false => !!{ mkerr("Not Authorized") }
  }
}

AuthService[T]<>(svc :@) {
  reset :() unit {
    svc.bar = false
  }
}

AuthService[T]<log.Logger> {
  log :(msg :str) unit {
    log.write(msg)
  }
}

## Matching on Services

Service instances participate in pattern matching through the `@(` sigil. Patterns bind protocol handles by keyword, enabling method calls on the bound alias immediately inside the arm.

```brim
svc =>
  @(auth : AuthProtocol, log : Logger) => auth.login("demo", "pw")
  @(metrics : Metrics)                 => metrics.increment()
  _                                    => unit{}
```

Service pattern entries always annotate the bound alias with a protocol type. Other pattern forms do not admit `:Type` ascriptions; only services permit this substitution surface.
```

## Protocols

- **Declaration:** `Proto[T?] := .{ method :(ParamTypes) Ret, … }`
- The leading `.` denotes behavioral shape (protocol) and appears as a type aggregate shape.

```brim
Fmt := .{ to_string :() str }
```

## Generic constraints

- **Form:** `T :Proto (+ Proto)*` (any generic parameter may carry zero or more protocol constraints).

```brim
map[T :Iterable, U :Eq] :((T) U, seq[T]) seq[T] = (f, xs) { ... }

Box[T :Show] := %{ value : T }
```


## Examples

```brim
Fmt := .{ to_string :() str }

Logger := @{
  target :str,
}

Logger {
  (sink :str) @! {
    @{
      target = sink,
    }
  }

  ~(svc :@) unit {
    std::log::release(svc.target)
  }
}

Logger<Fmt>(svc :@) {
  to_string :() str { svc.target }
}
```

---

## Related Specs

- `spec/core/patterns.md` — Pattern matching semantics for service patterns
- `spec/core/match.md` — Match expressions with service pattern arms
- `spec/core/generics.md` — Generic constraints and protocol requirements
- `spec/core/expressions.md` — Expression forms overview
- `spec/grammar.md` — Service and protocol grammar productions
