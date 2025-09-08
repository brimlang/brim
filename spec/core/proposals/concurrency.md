---
level: C0+std+S0
title: Concurrency surfaces
authors: ['trippwill']
date: 2025-09-08
status: draft
---

# Concurrency — Tasks, Futures, Channels

**Status:** Proposal (stdlib + S0 sugar); C0 unchanged

This consolidates Brim’s concurrency surface *outside* the core. Futures/streams and message passing are expressed as **services** and **interfaces** in the standard library.

- Core types: `fut[T]`, `strm[T]`
- Stdlib (`std::task`): `spawn`, `await`, `join_all`, `select`, `with_cancel`
- `@local` placeholder annotation forbids crossing task boundaries (compile-time)

## 1) Futures & Streams as stdlib services

```brim
[[[std::task]]]
<<< Fut
<<< Strm
<<< spawn
<<< await
<<< join_all
<<< select

Fut[T] = ^{ }
Strm[T] = ^{ }

// Start a task; runs fn on the host scheduler and returns a handle
spawn = (fn :() T) Fut[T] { /* host-backed task */ }

// Wait for completion; suspends current task cooperatively
await = (ft :Fut[T]) T { /* blocks/suspends until ready */ }

join_all = (xs :list[Fut[T]]) T { /* returns list[T] or folds in place */ }

// Multi-wait over futures/channels (see §4)
select = (...) unit { /* see signature in §4 */ }

^Fut[T](self) {
  map  = (f :(T) U) Fut[U] { /* transform completion */ }
  ~() unit { /* cancel-on-drop semantics if adopted */ }
}

^Strm[T](self) {
  next = () Fut[opt[T]] { /* pull next; nil on end */ }
}
```

**Notes**
- No new core types; `Fut`/`Strm` are plain services with behavior.
- `await` is a free function; no special syntax. (S0 can layer pretty forms later.)

---

## 2) Channels as services + capability interfaces

```brim
[[[std::chan]]]
<<< bounded
<<< unbounded
<<< Tx
<<< Rx
<<< Sender
<<< TrySender
<<< AsyncSender
<<< Receiver
<<< TryReceiver
<<< Closeable

Tx[T] = ^{ }   // send handle
Rx[T] = ^{ }   // receive handle

Sender[T]     = *{ send :(v :T) Fut[res[unit]] }
TrySender[T]  = *{ try_send :(v :T) res[unit] }     // err(full|closed)
AsyncSender[T]= *{ send_async :(v :T) Fut[unit] }   // optional flavor
Receiver[T]   = *{ recv :() Fut[opt[T]] }
TryReceiver[T]= *{ try_recv :() opt[T] }
Closeable     = *{ close :() unit }

bounded   = (cap :u32) (Tx[T], Rx[T]) { /* bounded queue */ }
unbounded = ()        (Tx[T], Rx[T]) { /* unbounded queue */ }

^Tx[T](self) : Sender[T], TrySender[T], Closeable {
  send     = (v :T) Fut[res[unit]] { /* backpressured */ }
  try_send = (v :T) res[unit]      { /* non-blocking */ }
  close    = () unit { /* idempotent */ }
  ~() unit { self.close() }
}

^Rx[T](self) : Receiver[T], TryReceiver[T], Closeable {
  recv     = () Fut[opt[T]] { /* await item or nil on close+drain */ }
  try_recv = () opt[T]      { /* non-blocking */ }
  close    = () unit { /* drop + close */ }
  ~() unit { self.close() }
}
```

**Rationale**
- APIs consume *capabilities* (`Sender[T]`/`Receiver[T]`) instead of concrete types.
- `send` models backpressure via `Fut[res[unit]]` to unify with `try_send`.

---

## 3) Mobility guard (`@local`)

- Values copy by default; services are handles.
- `@local` (annotation) forbids crossing task boundaries. The checker rejects `send(tx, x)` if `x` or any captured handle is `@local`.

---

## 4) Selection over heterogeneous waitables

`std::task::select` polls a *small set* of futures/receives/timeouts and dispatches to continuations. The shape uses ordinary calls and Brim’s match‑arm syntax for continuations.

**Shape (illustrative)**
```brim
select(
  case rx1.recv() =>
  | opt:has(v) |> on_v(v)
  | opt:nil    |> on_done(),

  case rx2.recv() =>
  | opt:has(w) |> on_w(w),

  case timeout_ms(250) => |> on_timeout()
)
```
- Deterministic tie-break: cases are polled in declaration order for readiness.
- Futures (`Fut[T]`), channel ops (`recv()`), and helper futures like `timeout_ms` share the same surface.

---

## 5) S0 sugars (optional, separate proposals)

These are non-semantic and desugar to the APIs above.

- **Pipes**
  - Forward: `E /> f(args)` ⇒ `f(E, args)`
  - Reverse: `f(args) </ E` ⇒ `f(args, E)`

- **Early-return unwrappers**
  - `x!` for `res[T]` ⇒ match + `return res:err(e)`
  - `y?` for `opt[T]` ⇒ match + `return opt:nil`

- **Send/receive conveniences (deferred)**
  - `tx <- v` ⇒ `tx.send(v)`
  - `use v = rx.recv()` ⇒ `v = std::task::await(rx.recv())?`

---

## 6) Examples

### Linear receive with pipes
```brim
handle = (rx :Receiver[str]) res[unit] {
  rx.recv() /> std::task::await /> handle_message
  res:ok(())
}
```

### Producer/consumer with backpressure
```brim
run = () res[unit] {
  tx, rx = std::chan::bounded[str](128)

  prod = std::task::spawn(() unit {
    @ {
      line = next_line()?                   // opt unwrap sugar
      std::task::await(tx.send(line))!      // res unwrap sugar
      @>
    }
  })

  cons = std::task::spawn(() unit {
    @ {
      m = std::task::await(rx.recv())?      // nil → break
      handle_message(m)
      @>
    }
  })

  std::task::await(prod)
  std::task::await(cons)
  res:ok(())
}
```

### Fan-in with `select`
```brim
merge = (a :Receiver[str], b :Receiver[str]) Fut[list[str]] {
  acc := []
  std::task::spawn(() list[str] {
    @ {
      std::task::select(
        case a.recv() =>
        | opt:has(v) |> { acc := acc + [v] }
        | opt:nil    |> <@ acc,

        case b.recv() =>
        | opt:has(w) |> { acc := acc + [w] }
        | opt:nil    |> <@ acc
      )
      @>
    }
  })
}
```

---

## 7) Lowering sketch (WIT/Component Model)

- `Fut[T]`, `Strm[T]`, `Tx[T]`, `Rx[T]` are WIT **resources**; methods (`await`, `next`, `send`, `recv`, `close`) are resource fns.
- Constructors (`spawn`, `bounded`, `unbounded`) are `world` fns returning resources.
- `select` is a helper `world` fn that polls multiple waitables.

---

## 8) Decisions vs deferrals

**Decide**
- Futures/streams move to stdlib (`std::task`).
- Channels are services with capability interfaces (`Sender`, `Receiver`, etc.).
- Backpressure via `send : T -> Fut[res[unit]]`.

**Defer**
- Cancellation surface (tokens vs. scoped guards).
- Placeholder/sections for pipe args; tap/spy pipeline operators.
- Fairness strategy in `select` beyond declaration order.

---

## 9) Style guidance (stdlib)

- Prefer **data‑first** for transform functions (good with `/>`).
- Prefer **data‑last** for sinks/consumers (reads well with `</`).
- Keep error/option returns in `res`/`opt` to compose with `!`/`?` easily.

