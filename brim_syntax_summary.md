# Brim C0 — Syntax-Only Summary

A concise reference to **C0** (core) syntax only. No sugar (S0), no interop, no proposals beyond those locked down in the C0 addenda.

---

## Core Laws


- **Bindings:** const `=`, var `:=`, service `~=` (service destructor runs at scope exit).
- **Data vs behavior:** Structs carry data; services carry behavior. Only services satisfy protocols.
- **Identifiers:** Unicode; casing is not semantic.

---

## Modules

```brim
[[acme::io::temp]]
<< TempFile
fs = [[std::fs]]            // import alias (const binding)
limit := 0              // module state (literal/aggregate init only)
```

- **Header:** first line `[[pkg::ns::leaf]]`.
- **Exports:** `<< Symbol` (one per line) exports the symbol’s surface.
- **Imports:** `alias = [[pkg::ns::path]]` anywhere at top level (const binding).
- **State:** top-level `:=` with literal/aggregate initializers; no execution beyond initializers.

---

## Types (C0 forms)

- **Scalars:** `bool`, `i8, i16, i32, i64`, `u8, u16, u32, u64`, `rune`, `str`.
- **Tuples:** `#(T1, T2, …)`.
- **Structs:** `Type = %{ field: Type, … }`.
- **Unions:** `Name = |{ Variant: Type?, … }`.
- **Flags:** `Name = &uN{ a, b, … }`.
- **Special:** `opt[T]`, `res[T]`, `error`.

---

## Terms & Declarations

### Tuples (positional aggregates)

```brim
pair :#(i32, i32) = #(1, 2)
one  :#(i32)      = #(42)
```

- **Expr:** `#(e1, e2, …)`
- **Pattern:** `#(p1, p2, …)`

### Structs (named aggregates)

```brim
User = %{ id: str, age: i32 }
u  :User = User%{ id = "a", age = 39 }
```

- **Construct:** `Type%{ field = expr, … }`
- **Pattern:** `Type%{ field = pat, … }`

### Unions (choice aggregates)

```brim
Reply[T] = |{ Good: T, Error: str }
emit = () Reply[i32] { Reply:Good(42) }
```

- **Construct:** `Name:Variant(expr?)`
- **Pattern:** `Name:Variant(pat?)`

### Flags (bitsets)

```brim
Perms = &u8{ read, write, exec }
mask  = Perms{ read, exec }
```

- **Construct & pattern:** `Name{ a, … }` (presence checks in patterns)

---

## Protocols

```brim
Fmt = *{ to_string :() str }
```

- Declare with `*{ … }`; methods use `name :(params) Ret`.
- Only **services** can implement protocols.

---

## Services (locked C0 form)

```brim
Logger = ^|log| :* Fmt {
  // implicit fields (instance state)
  target := "stderr"
  hits   := 0i32

  // constructor (returns enclosing service implicitly)
  ^(to :str) { log.target := to; log.hits := log.hits + 1 }

  // methods
  to_string = () str { log.target }

  // destructor (runs on guard exit)
  ~() unit { }
}

use = () unit {
  l ~= Logger^("stderr")  // guard bind; ~() runs on scope exit
}
```

- **Declare:** `Type = ^|recv| :* Iface (+ Iface)* { … }`
- **Fields:** leading `:=` bindings become per-instance state.
- **Constructors:** `^(...) { … }` (implicit return `Type`).
- **Methods:** `name =(…) Ret { … }`.
- **Destructor:** `~() unit { … }`.
- **Construction (term):** `Type^( args )`.

---

## Functions

```brim
add = (x :i32, y :i32) i32 { x + y }
```

- Bind with `=`; return type follows `)`.
- Nested functions are permitted.

---

## Match (block‑only arms)

```brim
val =>
| res:ok(v) ?(v > 0) | { v }
| res:ok(_)          | { 0 }
| res:err(e)         | { log(e); -1 }
```

- **Introducer:** `expr =>`
- **Arms:** always `"|" Pattern [ "?(" guard ")" ] "| {" block "}"` (no thin arms in C0).
- **Guards:** `?(expr)` immediately after the pattern.
- **Body:** must begin with `{` and is a block expression; single‑expr bodies still use braces.
- **Exhaustiveness:** `_` wildcard allowed as the final arm.

---

## Loops

```brim
sum_to = (n :i32) i32 {
  acc := 0
  @{
    acc := acc + 1
    (acc == n) ? <@ acc : @>
  @}
}
```

- Loop: `@{ … @}`
- Continue: `@>`
- Break with value: `<@ expr`

---

## Binding Rules (errors implied)

- `name = expr` → const; rebinding with `=` is an error.
- `name := expr` → var; subsequent reassign must use `:=`.
- `name ~= expr` → scope-bound **service**; destructor `~()` runs at scope end.

---

## Spacing Convention

- One space before, none after colon: `(x :i32)`, `Box[T :*Show]`.

---

