---
id: core.services
title: Services, Interfaces, and Constraints
layer: core
authors: ['trippwill']
updated: 2025-09-08
status: accepted
version: 0.1.0
---

# Brim C0 — Services, Protocols, and Constraints

- **Qualified service field access.** In C0, service fields must be accessed as `recv.field` (no bare names).

## Services

- **Declaration Block:** `Type = ^|recv| :Proto (+ Proto)* { … }`
  - `^|recv|` introduces a service with named receiver `recv`.
  - `:Proto (+Proto)*` lists implemented protocols (constraint form unified).

Service block members are ordered as follows:

- **Block contents:**
  - **Implicit fields:** maximal prefix of `:=` bindings → per-instance state.
  - **Constructors:** `^(...) { … }` (return type = enclosing service implicitly).
  - **Methods:** `name =(…) Ret { … }`.
  - **Destructor:** `~() unit { … }` (runs on `~=` scope exit, reverse lexical order).

```brim
Logger = ^|log| :Fmt + Flush {
  target := "stderr"
  hits := 0i32

  ^(to :str) { log.target := to; log.hits := log.hits + 1 }

  write = (s :str) unit { std::io::write(log.target, s) }
  flush = () unit { std::io::flush(log.target) }
  ~() unit { }
}
```

## Protocols

- **Declare:** `Proto[T?] = .{ method :(params) Ret, … }`
- The leading `.` denotes behavioral shape (protocol).

```brim
Fmt = .{ to_string :() str }
```

## Generic constraints

- **Form:** `T :Proto (+ Proto)*` (any generic parameter may carry zero or more protocol constraints).

```brim
map[T :Iterable, U :Eq] = (f :(T) U, xs :*[T]) *[T] { ... }

Box[T :Show] = %{ value: T }
```


## Examples

```brim
Fmt = .{ to_string :() str }

Logger = ^|log| :Fmt {
  target := "stderr"
  ^(to :str) { log.target := to }
  to_string = () str { log.target }
  ~() unit { }
}

main = () str {
  logger ~= Logger("stdout")
  logger.to_string()  // OK
}
```

