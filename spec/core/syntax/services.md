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

### Implementation block
- **Form:** `Service[T?] <recv> { StateBlock Member* }`
- Receiver binder is explicit and mandatory; use `<_>` if intentionally unused.
- Exactly one `StateBlock`, and it must be the first item.

State and members:
- `StateBlock ::= '<' FieldDecl (',' FieldDecl)* (',')? '>' StmtSep` (empty `<>` allowed for stateless)
- `FieldDecl  ::= ('@')? Ident ':' TypeExpr` (no initializers; ctors must assign)
- `CtorImpl   ::= '^(' ParamDeclList? ')' BlockExpr`
- `MethodImpl ::= Ident '(' ParamDeclList? ')' ReturnType BlockExpr`
- `DtorImpl   ::= '~()' BlockExpr`

Rules:
- All declared fields must be assigned exactly once in every constructor before any read.
- Field writes are allowed only inside service impls on the bound receiver: `recv.field = expr`. Fields declared with `@` are mutable post‑construction; non‑`@` fields are writable only in constructors.
- Outside impls, the language remains whole‑value rebinding only (no field mutation).

Example:
```brim
IntService[T]<i>{
  < @accum :T, @call_count :u64, >

  ^(seed :T) {
    @i.accum = seed
    @i.call_count = 0u64
  }

  add(x :T, y :T) T {
    r = x + y
    @i.accum = i.accum + r
    @i.call_count = i.call_count + 1
    r
  }

  to_str() str { sfmt.itoa(i.accum) }
  fmt(s :str) str { panic("not implemented") }
  ~() { }
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
  ^(to :str) = { log.target = to }
  to_string :() str { log.target }
  ~() unit = { }
}

main :() str = {
  logger ~= Logger("stdout")
  logger.to_string()
}
```
