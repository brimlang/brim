---
level: S0
title: Pipe Operators
authors: ['trippwill']
date: 2025-09-08
status: proposal
---

# Pipe Operators (`/>`, `</`) — S0 Proposal (v0.8‑draft)

**Status:** Proposal (S0 sugar)
**Goal:** Provide linear, readable function application chains that stay faithful to Brim’s expression‑only core. Pipes are *pure sugar* that desugar to ordinary function calls in C0. No new runtime or semantics.


## Summary

- **Forward pipe** `A /> call(args...)`
  Desugars to `call(A, args...)`. Feeds the left value as the **first** argument of the right‑hand call.

- **Reverse pipe** `call(args...) </ A`
  Desugars to `call(args..., A)`. Feeds the right value as the **last** argument of the left‑hand call.

- Pipes are **left‑associative** and may be **freely mixed**. They live in **S0** and always reduce to nested applications in **C0**.


## Desugaring (S0 → C0)

Let `E`, `F`, `G` be expressions; `f`, `g` be function expressions; and `args` be zero or more arguments.

### Forward pipe
```
E /> f(args)
```
⇢
```
f(E, args)
```

Chaining:
```
E /> f(a) /> g(b, c)
```
⇢
```
g(f(E, a), b, c)
```

### Reverse pipe
```
f(args) </ E
```
⇢
```
f(args, E)
```

Chaining:
```
f(a) </ E </ G
```
⇢
```
f(a, E, G)
```

### Mixing
```
E /> f(a) </ h
```
⇢
```
h(f(E, a))
```

## Syntax constraints

- **RHS/LHS must form a call or callable:**
  - `E /> f` is permitted and treated as `E /> f()` ⇒ `f(E)`.
  - `f </ E` is permitted and treated as `f() </ E` ⇒ `f(E)`.
  - Parentheses can force grouping: `(E1, E2) /> pair` ⇒ `pair((E1, E2))`.
- **No placeholder/sections in this proposal.** Argument holes (e.g., `_` positions) are out of scope; keep S0 minimal. (Can be a future sugar.)
- **No method dispatch:** Core types have no methods; pipes always target free functions or statics.

## Precedence & associativity

- `/>` and `</` have **lower precedence than function application** and **higher than `?:` ternary** (if introduced later). In practice:
  - `f(x) /> g()` parses as `(f(x)) /> (g())` ⇒ `g(f(x))`.
  - `x! /> f()` (with the unwrap sugar) parses as `(x!) /> f()`; unwrap happens before the pipe.
- **Left‑associative:** `A /> f() /> g()` ≡ `g(f(A))`. Similarly, `f() </ A </ B` ≡ `f(A, B)`.


## Typing rules (intent)

- Pipes do not change types themselves; they require that the desugared call type‑checks. Any arity/typing errors are reported after desugaring, at the callsite.
- Overloads (if any) are resolved on the desugared form.


## Examples (Brim)

### Data‑first with forward pipe
```brim
sum_evens = (xs :list[i32]) i32 {
  xs /> std::list::filter(is_even)
     /> std::list::map(double)
     /> std::list::sum()
}
```

### Data‑last with reverse pipe
```brim
log_then_write = (data :str) res[unit] {
  std::log::info("writing") </ data
  std::fs::write_all(data, "out.txt")
}
```

### Mixed
```brim
normalize = (xs :list[str]) list[str] {
  xs /> std::list::map(trim)
     /> std::list::filter(not_empty)
     </ std::list::dedupe    // pass result as last arg
}
```

### With early‑return unwrappers (`!`, `?`)
```brim
load_profile = (id :str) res[Profile] {
  db::get_user(id)!           /> map_user_to_profile!
                        /> hydrate()
}
```
(Each `!` unwraps before its pipe step; on `err`, the function early‑returns.)


## Stdlib conventions (guidance, not a rule)

- Prefer **data‑first** parameter order for transformation functions to align with `/>`.
- Prefer **data‑last** for sinks, consumers, or DSL‑style builders that read naturally with `</`.
- When both readings make sense, provide two helpers (e.g., `map(xs, f)` and `map_with(f, xs)`) or accept named arguments later (out of scope here).


## Parsing & tokens

- The tokens `/>` and `</` are two‑character operators.

## Diagnostics (illustrative)

- **BRIM‑S0‑P001:** Pipe target is not callable.
  *Help:* “Write `f()` if you meant a no‑arg call, or wrap in a lambda.”
- **BRIM‑S0‑P002:** Mixed pipe chain groups ambiguously.
  *Help:* “Add parentheses to disambiguate precedence.”


## Out of scope (future work)

- Argument placeholders/sections (e.g., `map(_ , f)` combined with pipes).
- Pipeline tap/spy operators for debugging (e.g., `dbg />` style).
- Partial application sugar.

---

**Governance:** As S0 sugar, follows the S0 charter: proposal → experiment → `std.s0` inclusion → versioned release. No impact on C0 or emitter.

