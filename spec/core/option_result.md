---
id: core.option-result
layer: core
title: Option/Result & Return Lifting
authors: ['trippwill', 'gpt-5-thinking']
updated: 2025-01-28
status: accepted
version: 0.1.0
---
# Option/Result & Return Lifting

## Summary

Defines symbolic encodings for **option** and **result** and **type-directed return lifting** and **postfix short-circuit propagation** in core.

- **Types:** `T?` (option), `T!` (result).
- **Constructors (expr):** `?{}` (nil), `?{x}` (has), `!{x}` (ok), `!!{e}` (err).
- **Propagation (expr):** `e?` (propagate nil), `e!` (propagate err) to the nearest matching context.
- **Return lifting:** In bodies returning `T?`/`T!`, a tail `T` auto-lifts to `?{…}` / `!{…}`; an existing `T?`/`T!` passes through.

For pattern matching with option/result values, see `spec/core/patterns.md`. For match expression details, see `spec/core/match.md`.

## Specification (Normative)

### 1. Types

- If `T` is a type, then `T?` and `T!` are types.

### 2. Constructors (Expressions)

- `?{}` constructs the **nil** value of some `T?` (requires an expected type `T?`).
- `?{x}` constructs **has(x)** of type `T?` when `x :T`.
- `!{x}` constructs **ok(x)** of type `T!` when `x :T`.
- `!!{e}` constructs **err(e)** of type `T!` when `e :err`.

These forms are primary expressions and may appear anywhere an expression is allowed.

### 3. Postfix Short-Circuit Propagation

#### Parsing & Precedence

- The postfix propagation operators `?` and `!` have higher precedence than function application.
  - Example: `f(x!)` parses as `f((x!))`.
  - Example: `x! /> f()` (if pipes exist) parses as `(x!) /> f()`.
- Both bind to the closest primary expression (`x`, `call()`, `(expr)`, etc.).

#### Edge Cases & Clarifications

- **Nesting:** `foo(bar()!)!` is valid; each unwrap targets the innermost suitable context.
- **Anonymous/nested functions:** The “enclosing function” is lexical. Inside a nested function returning `T!`, `!` refers to that nested function.
- **Non‑returning contexts:** Inside blocks that don’t belong to any function body (e.g., global initializers), `!`/`?` are invalid.
- **Exhaustiveness:** The propagation desugars to a total match over all variants.

#### Desugaring

Explicit desugaring for propagation:

```brim
x!         // where x :T!
```

⇢
```brim
x =>
!(__v)  => { __v }
!!(__e) => { !!{__e} }
```

```brim
y?         // where y :T?
```

⇢
```brim
y =>
?(__v) => { __v }
?()    => { return ?{} }
```

- If `e :U?`, then `e?` evaluates `e`; if `nil`, returns  from the nearest context expecting a `T?`; otherwise yields the inner `U`.
- If `e :U!`, then `e!` evaluates `e`; if `err`, **returns that error** from the nearest context expecting a `T!`; otherwise yields the inner `U`.
- It is an error to apply `?`/`!` to a non‑option/non‑result expression.
- Propagation is permitted anywhere an expected `T?`/`T!` type exists (e.g., function bodies, initializers, return-position expressions).

### 4. Canonical Function Declarations & Return Lifting

Canonical nominal/value split applies:

```brim
name :(ParamTypes...) ReturnType = |params|> { body }
-- or shorthand header with parameter names & types (const only):
name :(x :T, y :U) ReturnType { body }
```

Rules when `ReturnType` is `T?` or `T!` (unchanged semantically):

- If the tail expression of a function body has type `T`, implicitly lift to `?{tail}` / `!{tail}`.
- If the tail already has type `T?` / `T!`, return it unchanged (no double wrapping).
- Propagation (`e?` / `e!`) inside the body targets the nearest enclosing function whose declared return type is an option/result of the matching kind.

### 5. Patterns

The same symbolic shapes are valid in pattern contexts:

```brim
?()      -- matches nil
?(v)     -- matches has(v)
!(v)     -- matches ok(v)
!!(e)    -- matches err(e)

-- simplified forms with contextual target type
o :T?
o =>
  ? => {...} -- matches nil
  v => {...} -- matches has(v)

f :T!
f =>
  !!(e) => {...} -- matches err(e)
  v     => {...} -- matches ok(v)
```

Patterns are allowed in `match` arms and destructuring binds.

### 6. Truthiness

- Conditionals do **not** accept `T?`/`T!` directly. Users must match or test explicitly.

### 7. Lexical & Formatting

- `!{` and `!!{` begin constructors with no whitespace allowed between `!` and `{`.
- Same for `?{`.
- Postfix propagation tokens are single glyphs placed **immediately** after the operand: `expr?`, `expr!`.

## Examples (Normative)

### Canonical functions

```brim
a : () i32? = () => 56                 -- lifts to ?{56}

b : () i32? = () => ?{}                -- nil

c : () str!  = () => "hello"          -- lifts to !{"hello"}

d : () str!  = () => !!{err("domain", 45)}  -- explicit error

try_do : () unit! = () => unit{}             -- lifts to !{()}
```

### Propagation in a fallible function

```brim
make_user : (text) user! = (s) => {
  id   := parse_id(s)!            -- propagate err if parse_id fails
  name := parse_name(s)?          -- propagate nil if option context; here we map nil to error
  name =>
    n   => { make_user_core(id, n)! }
    ?() => { !!{err("missing-name")} }
}
```

### Patterns

```brim
maybe_port =>                       -- :i32?
  p    => { use_port(p) }
  ?()  => { use_default() }

outcome =>                          -- :str!
  v      => { log(v) }
  !!(e)  => { handle(e) }

ping() =>                           -- : ()!
  !()    => { ok_path() }           -- matches ok(()) (unit success)
  !!(e)  => { err_path(e) }
```

## Notes (Non‑Normative)

- The constructors behave like literals; the success‑path terseness comes from return lifting.
- An explicit typed‑nil can be written by ascription if needed in expression positions without expectation, e.g., `(: i32?) ?{}`.

## Diagnostics (Informative)
- Using `?`/`!` on non‑option/non‑result: “propagation requires `U?`/`U!`; got `{τ}`”.
- Tail mismatch: “tail produces `{U?}` but function returns `{T?}`; map or match to `{T?}`”.
- Untyped nil: “`?{}` requires an expected type `T?` or a type ascription”.
+ Using `!` outside a function returning `T!`: “`!` requires enclosing `T!` return. Help: Wrap the value in `!{...}` or change the function return type to `T!`.”
+ Using `?` outside a function returning `T?`: “`?` requires enclosing `T?` return. Help: Wrap the value in `?{...}` or change the function return type to `T?`.”
- Using `?`/`!` on non‑option/non‑result: “propagation requires `U?`/`U!`; got `{τ}`”.
- Tail mismatch: “tail produces `{U?}` but function returns `{T?}`; map or match to `{T?}`”.
- Untyped nil: “`?{}` requires an expected type `T?` or a type ascription”.

## Interactions

- **Error type:** `err` is a nominal type defined elsewhere. `!!{e}` requires `e :err`.
- **Emit/ABI:** Lowering to WIT/wasm‑GC follows canonical option/result encodings (see emitter spec).

---

## Related Specs

- `spec/core/patterns.md` — Pattern matching semantics for option/result patterns
- `spec/core/match.md` — Match expressions with option/result arms
- `spec/core/expressions.md` — Expression forms and propagation overview
- `spec/functions.md` — Function return types and lifting
- `spec/grammar.md` — Option/result syntax grammar productions
