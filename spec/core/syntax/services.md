---
id: core.services
title: Services, Protocols, and Constraints
layer: core
authors: ['trippwill']
updated: 2025-09-13
status: accepted
version: 0.1.0
---

# Services, Protocols, and Constraints

## Services

Services are nominal types that implement one or more protocols. The service type declaration lists protocol references only. Private state and behavior are defined in a separate implementation block.

### Type declaration (protocol refs only)
- **Form:** `Service[T?] := ^{ ProtoRef (',' ProtoRef)* (',')? }`
- `ProtoRef ::= Ident GenericArgs?`

Example:
```brim
Adder[T] := .{ add :(T, T) T }
Fmt      := .{ to_str :() str, fmt :(str) str, }

IntService[T] := ^{ Adder[T], Fmt, }
```

### Implementation block (combined)
- **Form:** `^Service[T?] <recv> (params)? { InitDecl* DtorOpt Method* }`
- Receiver binder is explicit and mandatory; use `<_>` if intentionally unused.
- Initialization comes first and contains field declarations with initializers. Zero‑state services omit the section entirely.

State and members:
- `InitDecl    ::= ('@')? Ident ':' TypeExpr '=' Expr` — `@` marks mutable post‑ctor; init required
- `MethodImpl  ::= Ident '(' ParamDeclList? ')' ReturnType BlockExpr`
- `DtorImpl    ::= '~()' ReturnType BlockExpr` — explicit destructor (no params): e.g., `~() unit { ... }`. If present, it must appear immediately after the init section and before any methods.

Rules:
- If any fields exist, they must be declared and initialized exactly once in the init section.
- Fields are only accessible through the receiver (e.g., `i.field`); bare field names are not in scope.
- All writes require write‑intent: `@recv.field = expr` (including init and methods).
- Fields declared with `@` are mutable after construction; non‑`@` fields are readonly after construction.
- Init parameters are only in scope within the init section. Store any values needed later into fields during initialization.

Example:
```brim
^IntService[T]<i>(seed :T) {
  accum :T = seed
  @call_count :u64 = 0u64

  add(x :T, y :T) T {
    r = x + y
    @i.call_count = i.call_count + 1
    r
  }

  to_str() str { sfmt.itoa(i.accum) }
  fmt(s :str) str { panic("not implemented") }
  ~() unit { }
}
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
map[T :Iterable, U :Eq] :((T) U, list[T]) list[T] = (f, xs) { ... }

Box[T :Show] := %{ value : T }
```


## Examples

```brim
Fmt := .{ to_string : () str }

Logger :^log{ target :str } :Fmt = {
  ^Logger<_>(to :str) {
    target :str = to
  }
  to_string :() str { target }
  ~ { }
}

main :() str = {
  logger ~= Logger("stdout")
  logger.to_string()
}
```
