---
level: C0+std+S0
title: res and opt early returns and sugar
authors: ['trippwill']
date: 2025-09-08
status: draft
---

# Early-Return Unwrappers (`!`, `?`) — S0 Proposal (v0.8-draft)

**Status:** Proposal (S0 sugar)
**Motivation:** Eliminate boilerplate `match` when working with builtin `res[T]`/`opt[T]` by allowing linear, pipeline-friendly code that “unwraps or early-returns.” Core semantics remain unchanged; this is pure desugaring to C0.

---

## Summary

- `expr!` — unwraps a `res[T]`; on `ok(v)` yields `v`, on `err(e)` **early-returns** `res:err(e)` from the *enclosing function*.
- `expr?` — unwraps an `opt[T]`; on `has(v)` yields `v`, on `nil` **early-returns** `opt:nil` from the *enclosing function*.

These sugars rely on the existing builtin unions `res[T]`/`opt[T]` and their variants.

---

## Typing constraints

- `!` is only valid when the *nearest enclosing function’s* return type is `res[R]` for some `R`.
- `?` is only valid when the nearest enclosing function’s return type is `opt[R]`.

Using them outside the required return context is a compile error (see Diagnostics).

---

## Desugaring (S0 → C0)

### `res` unwrap
```brim
x!         // where x : res[T]
```

⇢
```brim
x =>
| res:ok(__v)   |> __v
| res:err(__e)  | { return res:err(__e) }
```

### `opt` unwrap
```brim
y?         // where y : opt[T]
```

⇢
```brim
y =>
| opt:has(__v) |> __v
| opt:nil      | { return opt:nil }
```

(Names `__v`, `__e` are hygienic temporaries.)

---

## Examples

### Happy‑path linear flow
```brim
read_user = (id :str) res[User] {
  u   = db::get_user(id)!        // err → early return res:err(e)
  acc = check_access(u)!         // err → early return
  res:ok(u)
}
```

### Optional search
```brim
first_admin = (xs :list[User]) opt[User] {
  u = std::list::find(xs, is_admin)?   // nil → early return opt:nil
  opt:has(u)
}
```

### With forward pipe `/>` (if adopted)
```brim
load_profile = (id :str) res[Profile] {
  db::get_user(id)! /> map_user_to_profile! /> hydrate()
}
```
Desugars to nested calls with the same early‑return behavior.

---

## Interaction notes

- **Matches & guards:** Desugars *into* a match; nests with existing match syntax without ambiguity.
- **RAII/guards (`~=`):** Unrelated; `!`/`?` do not affect destructor invocation. If an unwrap triggers an early return, any guards in scope still run in LIFO order at scope exit per normal rules.
- **No new runtime:** Pure sugar; core semantics and WIT/ABI unaffected.

---

## Parsing & precedence

- `!` and `?` are **postfix operators** with higher precedence than function application, so:
  - `f(x!)` parses as `f( (x!) )`.
  - `x! /> f()` (if pipes exist) parses as `(x!) /> f()`.
- Both bind to the closest primary expression (`x`, `call()`, `(expr)`, etc.).

---

## Diagnostics (illustrative)

- **BRIM‑S0‑E001:** `!` requires enclosing `res[...]` return.
  *Help:* “Wrap the value in `res:ok(...)` or change the function return type to `res[...]`.”
- **BRIM‑S0‑E002:** `?` requires enclosing `opt[...]` return.
  *Help:* “Wrap the value in `opt:has(...)` or change the function return type to `opt[...]`.”

---

## Edge cases & clarifications

- **Nesting:** `foo(bar()!)!` is valid; each unwrap targets the innermost suitable context.
- **Anonymous/nested fns:** The “enclosing function” is lexical. Inside a nested function returning `res[...]`, `!` refers to that nested function.
- **Non‑returning contexts:** Inside blocks that don’t belong to any function body (e.g., global initializers), `!`/`?` are invalid.
- **Exhaustiveness:** The desugared `match` is total over `res`/`opt` variants.

---

**Governance:** As S0 sugar, this follows the S0 charter: proposal → experiment → `std.s0` inclusion → versioned release. No impact on C0 or emitter.

