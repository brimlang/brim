---
level: E
title: Interop Imports and Linkage
authors: ['trippwill']
date: 2025-09-08
status: draft
---

# Brim Interop Imports & Linkage

## Binding Forms

- **Internal Brim modules**
  ```brim
  util = std::util
  ```
  → Compiled into the same component, no world import.

- **External WIT interfaces (host/WASI)**
  ```brim
  monotonic = [wasi:clocks/monotonic]
  stdio     = [wasi:io/stdio]
  ```
  → Lowers to `use` in WIT; becomes a required **world import**.

## Rules

- **One alias per binding.** No wildcard imports.
- **Square brackets** (`[pkg:iface(/member)?]`) are the canonical syntax.
- **Versions** are not in C0; must be set in the **project configuration** (`brim.toolchain.toml`).
  - Deferral: optional inline form `[pkg@0.2:iface]` may be added later.
- Imported alias is a **namespace** exposing all functions and types of the interface.
  ```brim
  clk = monotonic::MonotonicClock
  now = monotonic::now()
  ```

## Lowering

- Brim `str` → WIT `string` → WasmGC `(ref null (array i8))`.
- Brim `res[T]` → WIT `result<T>` → core integer tag + payload.
- Brim `T!` → WIT `result<T>` → core integer tag + payload.
- Services → WIT `resource` (constructors, methods, destructors).
- External calls → **world imports**; internal Brim helpers compile in.

## Worked Example

### Brim
```brim
[[[acme::hello]]]
<<< greet

text  = std::text
stdio = [wasi:io/stdio]

greet = (name :str) res[unit] {
  msg = text::concat("hi, ", name, "\n")
  stdio::print(msg)
  !{()}
}
```

### WIT
```wit
package acme:hello

world hello {
  use wasi:io/stdio

  interface hello-api {
    greet: func(name: string) -> result
  }

  export hello-api
}
```

### Core WasmGC (sketch)
```wasm
(import "wasi:io/stdio" "print"
  (func $wasi_stdio_print (param (ref null $list_u8))))

(func $greet (export "acme:hello/hello-api#greet")
             (param $name (ref null $list_u8))
             (result i32) ;; 0=ok, 1=err
  ;; msg = concat("hi, ", name, "\n")
  (local $msg (ref null $list_u8))
  ;; ...
  (call $wasi_stdio_print (local.get $msg))
  (i32.const 0)
)
```

---

✅ **Decision locked (2025-08-22):**
- Versions resolved in **project config only**.
- Lowering path is **Brim → WIT world/use → WasmGC GC refs**.
