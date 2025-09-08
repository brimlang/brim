---
status: draft
---

# Brim v0.0.0 — WASM/WIT Interop

Brim lowers to the WebAssembly Component Model via WIT. The language targets GC-capable hosts exclusively; non-GC engines are out of scope.
This document is not authoritative with respect to the brim syntax.

## Core Principles

- **GC required.** Modules assume a WASM GC host.
- **No linear-memory strings or lists.** Brim values map directly to GC reference types.
- **No fallback.** There is no linear-memory mode.

## Surface → WIT Mapping

| Brim construct | WIT lowering |
| -------------- | ------------ |
| Module         | world        |
| Struct         | record       |
| Union          | variant      |
| Flags          | flags        |
| Service        | resource (constructors, methods, statics, destructor)|
| Free function  | interface fn |
| Shared type    | interface type |

## Imports and Exports

```brim
[[[acme::math]]]
<<< Adder
>> util = std::util
```

- Import: `alias = pkg::ns::path` → WIT `use`.
- Export: `<<< Symbol` exports the symbol’s entire surface.

## Mapping of Core Types

- `str` → `string`
- `list[T]` → `(array T)`
- `struct` → `struct { ... }`
- `unions` → GC reference variants
- `services` → WIT resources with constructors and ABI destructor
- `opt[T]`, `res[T]` → WIT `option` / `result`

## Identifiers

- Brim identifiers are Unicode; casing is not semantic.
- WIT identifiers are ASCII. The emitter applies a stable transform, overridable via export maps.

## Canonical ABI Alignment

Brim adheres to the canonical ABI with GC extensions. Example:

```brim
add = (xs :list<u8>) i32 { ... }
```

Lowers to WIT:

```wit
fn add(xs: list<u8>) -> s32
```

The adapter presents the core module with a GC `array<u8>`; pointer pairs are never exposed.

## Toolchain Notes

- Emitters always produce GC reference types.
- Toolchains reject targets lacking GC.
- `brimc` assumes `wasm-tools` / `wit-bindgen` versions with GC support.

## Example Lowering

Brim:

```brim
[[[acme::math]]]
<<< Adder

Adder = ^{ }

^Adder(self) {
  ^(init :i32) Adder { Adder{ } }
  add = (x :i32, y :i32) i32 { x + y }
  ~() unit { }
}
```

WIT:

```wit
world math {
  resource Adder {
    constructor(init: s32)
    fn add(self: borrow<Adder>, x: s32, y: s32) -> s32
  }
}
```

The destructor is emitted in the component’s ABI, not as textual WIT.

