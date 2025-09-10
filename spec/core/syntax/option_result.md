---
id: core.option-result
layer: core
title: Option/Result & Return Lifting
authors: ['trippwill', 'gpt-5-thinking']
updated: 2025-09-10
status: accepted
---
# Option/Result & Return Lifting

## Summary

Introduce symbolic encodings for **option** and **result** and define **type-directed return lifting** and **postfix short-circuit propagation** in core.

- **Types:** `T?` (option), `T!` (result).
- **Constructors (expr):** `?{}` (nil), `?{x}` (has), `!{x}` (ok), `!!{e}` (err).
- **Propagation (expr):** `e?` (propagate nil), `e!` (propagate err) to the nearest matching context.
- **Return lifting:** In bodies returning `T?`/`T!`, a tail `T` auto-lifts to `?{…}` / `!{…}`; an existing `T?`/`T!` passes through.

This spec replaces the "pre-amble" built-ins opt[T] and res[T], and makes `T?` and `T!` core types
instead of instances of the union aggregate.

## Specification (Normative)

### 1. Types

- If `T` is a type, then `T?` and `T!` are types.

#### 1.1 Unit type alias

- The **unit type** is spelled `unit` and **canonically** as `()`.
- In **type space**, these are interchangeable:
  - `() ≡ unit`
  - `()? ≡ unit?`
  - `()! ≡ unit!`
- No whitespace is permitted between `()` and the postfix `?`/`!` in type positions.

### 2. Constructors (Expressions)

- `?{}` constructs the **nil** value of some `T?` (requires an expected type `T?`).
- `?{x}` constructs **has(x)** of type `T?` when `x : T`.
- `!{x}` constructs **ok(x)** of type `T!` when `x : T`.
- `!!{e}` constructs **err(e)** of type `T!` when `e : error`.

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
x!         // where x : T!
```

⇢
```brim
x =>
!(__v)  => { __v }
!!(__e) => { !!{__e} }
```

```brim
y?         // where y : T?
```

⇢
```brim
y =>
?(__v) => { __v }
?()    => { return ?{} }
```

- If `e : U?`, then `e?` evaluates `e`; if `nil`, returns  from the nearest context expecting a `T?`; otherwise yields the inner `U`.
- If `e : U!`, then `e!` evaluates `e`; if `err`, **returns that error** from the nearest context expecting a `T!`; otherwise yields the inner `U`.
- It is an error to apply `?`/`!` to a non‑option/non‑result expression.
- Propagation is permitted anywhere an expected `T?`/`T!` type exists (e.g., function bodies, initializers, return-position expressions).

### 4. Canonical Function Declarations & Return Lifting

Canonical form:

```brim
name = (params) ReturnType { body }
```

Rules when `ReturnType` is `T?` or `T!`:

- If the **tail expression** of `body` has type `T`, implicitly return `?{tail}` / `!{tail}`.
- If the tail already has type `T?` / `T!`, return it unchanged (no double wrapping).
- Propagation (`e?` / `e!`) inside the body targets this nearest `T?` / `T!` context.

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

### 6. Control Flow & Truthiness

- `if`, `while`, and other conditionals do **not** accept `T?`/`T!` directly. Users must match or test explicitly.

### 7. Lexical & Formatting

- `!{` and `!!{` begin constructors with no whitespace allowed between `!` and `{`.
- Same for `?{`.
- In **type space**, `()!` and `()?` are exactly `unit!` and `unit?`; no whitespace is permitted between `)` and the postfix `!`/`?`.
- Postfix propagation tokens are single glyphs placed **immediately** after the operand: `expr?`, `expr!`.

## Examples (Normative)

### Canonical functions

```brim
a = () i32? { 56 }                      -- lifts to ?{56}

b = () i32? { ?{} }                     -- nil

c = () str!  { "hello" }               -- lifts to !{"hello"}

d = () str!  { !!{err("domain", 45)} } -- explicit error

try_do = () ()! { () }                -- lifts to !{()} (i.e., ok(()))
```

### Propagation in a fallible function

```brim
make_user = (s: text) user! {
  id   := parse_id(s)!;          -- propagate err if parse_id fails
  name := parse_name(s)?;        -- if nil here, function must map to an error or return nil in an option context
  -- mapping nil → error explicitly:
  name =>
  n => { make_user_core(id, n)! }
  ?  => { !!{error("missing-name")} }
}
```

### Patterns

```brim
match maybe_port {                  -- : i32?
  p => { use_port(p) }
  ?  => { use_default() }
}

match outcome {                     -- : str!
  v      => { log(v) }
  !!(e)  => { handle(e) }
}

match ping() {                      -- : ()!
  !()    => { ok_path() }           -- matches ok(()) (unit success)
  !!(e)  => { err_path(e) }
}
```

## Notes (Non‑Normative)

- The constructors behave like literals; the success‑path terseness comes from return lifting.
- An explicit typed‑nil can be written by ascription if needed in expression positions without expectation, e.g., `(: i32?) ?{}`.

## Diagnostics (Informative)
- Using `?`/`!` on non‑option/non‑result: “propagation requires `U?`/`U!`; got `{τ}`”.
- Tail mismatch: “tail produces `{U?}` but function returns `{T?}`; map or match to `{T?}`”.
- Untyped nil: “`?{}` requires an expected type `T?` or a type ascription”.
- Misuse of `!()` in **expression** context: “`!()` is not a constructor; did you mean to return `()` in a `()!` context (will lift to ok(())) or write `!{()}`?”
+ Using `!` outside a function returning `T!`: “`!` requires enclosing `T!` return. Help: Wrap the value in `!{...}` or change the function return type to `T!`.”
+ Using `?` outside a function returning `T?`: “`?` requires enclosing `T?` return. Help: Wrap the value in `?{...}` or change the function return type to `T?`.”

- Using `?`/`!` on non‑option/non‑result: “propagation requires `U?`/`U!`; got `{τ}`”.
- Tail mismatch: “tail produces `{U?}` but function returns `{T?}`; map or match to `{T?}`”.
- Untyped nil: “`?{}` requires an expected type `T?` or a type ascription”.
- Misuse of `!()` in **expression** context: “`!()` is not a constructor; did you mean to return `()` in a `()!` context (will lift to ok(())) or write `!{()}`?”

## Interactions

- **Error type:** `error` is a nominal type defined elsewhere. `!!{e}` requires `e : error`.
- **Emit/ABI:** Lowering to WIT/wasm‑GC follows canonical option/result encodings (see emitter spec).

