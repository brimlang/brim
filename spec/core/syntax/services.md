---
id: core.services
title: Services, Interfaces, and Constraints
layer: core
authors: ['trippwill']
updated: 2025-09-13
status: accepted
version: 0.1.0
---

# Services, Protocols, and Constraints

## Services

Unified nominal form aligns services with other aggregates and protocols.

- **Declaration:** `ServiceName :^recv{ state_field :Type, ... } :Proto (+Proto)* = { members }`
  - `^recv{ ... }` declares a service with receiver identifier `recv` and named private state fields.
  - After the service type shape, a colon introduces implemented protocols: `:Proto + Other`.
  - Body block provides constructors, methods, destructor.

### Members
- **Constructors:** `^() = { ... }` or `^(params) = { ... }` returning implicit service instance.
- **Methods:**
  - Binding form: `name :(ParamTypes...) Ret = (params) { ... }`
  - Combined header (ergonomic, const‑only): `name :(ParamTypes...) Ret { ... }`
- **Destructor:** `~() unit = { ... }` (runs on `~=` scope exit, reverse lexical order of binding sites).

State fields are immutable unless explicitly reassigned within methods via the receiver (e.g., `recv.field = ...`). Direct bare field references outside `recv.` are disallowed.

```brim
io ::= std::io     -- import required for term-space access

Fmt := .{ to_string :() str }

Logger :^log{ target :str, hits :i32 } :Fmt + Flush = {
  ^(to : str) = { log.target = to; log.hits = log.hits + 1 }

  write :(str) unit { io:write(log.target, s) }
  flush :() unit { io:flush(log.target) }
  to_string :() str { log.target }
  ~() unit = { }
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
  logger:to_string()
}
```
