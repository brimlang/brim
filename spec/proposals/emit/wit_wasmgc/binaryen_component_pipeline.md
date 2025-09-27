---
id: emit.binaryen_component_pipeline
layer: emit
title: Core Build with Binaryen & Component Composition
authors: ['trippwill']
updated: 2025-09-08
status: draft
---

# Brim Toolchain Appendix — Core Build with Binaryen & Component Composition

Describes how we **build & optimize core WasmGC with Binaryen**, and how we **wrap/compose** components with the WIT/Component toolchain. It complements the GC-only and pipeline decisions already in the corpus.

## 1) Roles at a Glance

- **BrimC (C0 → IR):** parses, validates, lowers to an internal IR.
- **Binaryen (via libbinaryen P/Invoke):** builds **core WasmGC** and runs optimization passes on the core module.
- **wasm-tools / component model (WIT):** wraps the core as a **component**, performs **composition/linking** with other components, resolves `use`d packages, validates worlds, and emits final component artifacts.
- **BLA (Brim Library Artifact):** a prebuilt, composable library packaging `component.wasm` + `package.wit` + metadata for caching and reuse.


## 2) End-to-End Build Pipeline

```
Brim source (C0)
      │
      ▼
BrimC front-end (AST, checks)
      │
      ▼
Core builder (libbinaryen via P/Invoke)
  - Emit WasmGC only (arrays, structs, refs)
  - Export raw funcs for canonical-ABI lift/lower
      │
      ▼
Binaryen optimize (O2/Oz) on **core module**
      │
      ▼
WIT emission + component wrap (wasm-tools)
  - Map public surface to WIT
  - Define world + `use`d external interfaces
  - Wrap core with canonical ABI (GC mode)
      │
      ├─► Compose with BLAs (vendored deps)  ← no source rebuild
      │
      ▼
Validate + write final component.wasm
      │
      └─► Publish as BLA (optional) for reuse
```

**Key:** Binaryen optimizes **core WasmGC**; `wasm-tools` links at the **component** layer.


## 3) Canonical ABI Edges (GC mode)

- `str` ⇄ `string` ⇄ `(ref null (array i8))` (UTF-8 bytes)
- `list<T>` ⇄ `list<T>` ⇄ `(ref null (array T))`
- `T!` ⇄ `result<T>` ⇄ `(i32 tag, payload?)`
- **Resources (services):** methods take `self` as `borrow<Resource>` by default; destructors are part of the component ABI, not a textual WIT function.
- No linear-memory pointer/length pairs anywhere in Brim’s target set.


## 4) Binaryen Pass Recommendations (Core WasmGC)

Choose per-profile; safe defaults shown:

- **Size-first (Oz):**
  - `--dce` (dead code elimination)
  - `--inlining-optimizing`
  - `--simplify-locals`
  - `--vacuum`
  - `--precompute`
  - `--merge-blocks`
  - `--optimize-instructions`
  - `--flatten`
  - `--gsi` (if available for GC shapes)
- **Speed-first (O2):**
  - all of the above plus:
  - `--licm`, `--reorder-locals`, `--reorder-functions`

**Sequencing tip:** run DCE after export metadata is fixed; do not strip any symbol the component wrapper expects to lift/lower.

---

## 5) Resolver & Composition (ASCII)

```
[Project Config + Lockfile]
        │
        ▼
  Resolver decides per import
        │
        ├─ Internal Brim module  ─► compile from source (→ core → component)
        │
        ├─ Vendored BLA          ─► compose BLA.component into app component
        │                           (no rebuild; uses BLA's package.wit)
        │
        └─ External host (WASI)  ─► stays a world `use`; must be present at instantiation
```

**Square-bracket WIT bindings in source:**
`stdio = [wasi:io/stdio]` → world import; `util = std::util` → internal (or vendored) module.

---

## 6) BLA (Brim Library Artifact) — Minimal Layout

```
/BLA/
  component.wasm     # compiled component (WasmGC inside)
  package.wit        # public WIT surface (package + world/interface types)
  abi.json           # {{ "gc": true, "canonical": "gc", "toolchain": "...", ... }}
  export-map.json    # stable naming for pretty Brim ↔ WIT mapping (optional)
  fingerprint        # content hash for cache & reproducibility
  debug.map          # optional, source maps / name maps
```

**Caching key:** hash(component.wasm + package.wit + abi.json) + toolchain version/options.

---

## 7) Minimal C# P/Invoke Shape (Illustrative)

```csharp
// PSEUDOCODE — signatures depend on your binding.
var m = Binaryen.ModuleCreate();

// Types (GC)
var i8 = Binaryen.TypeInt8();
var arr_i8 = Binaryen.ArrayType(i8);              // array i8
var strref = Binaryen.RefNull(Binaryen.HeapType(arr_i8));

// Import: print(string)
Binaryen.AddFunctionImport(
    m, "wasi:io/stdio", "print",
    new[] { strref }, Binaryen.TypeNone()
);

// Exported raw function: greet_raw(name: string) -> i32 (result tag)
var fn = Binaryen.AddFunction(
    m, "greet_raw",
    new[] { strref }, Binaryen.TypeInt32(),
    body: BuildCallPrintThenReturnOk(m)
);

Binaryen.AddFunctionExport(m, "greet_raw", "acme:hello/hello-api#greet");
Binaryen.ModuleOptimize(m);
File.WriteAllBytes("core.wasm", Binaryen.ModuleWrite(m));

// Later: use wasm-tools to wrap 'core.wasm' into a component per the WIT world.
```

---

## 8) Failure Modes & Guardrails

- **Missing host import:** component instantiation fails if a bracketed WIT binding (`[wasi:…]`) isn’t provided by the host.
- **Adapter mismatch:** provide a tiny adapter component or vendor a shim BLA to reconcile interface names/shapes.
- **Over-aggressive DCE:** keep raw exports used by canonical-ABI wrappers; mark them as preserved before Binaryen runs.

---

## 9) What Binaryen Does *Not* Do

Binaryen does **not** compose/link WIT components, resolve worlds, or trim WIT-level items. Use the component toolchain for that step.

---

## 10) Quick Checklist

- [ ] Emit only GC reference types from core.
- [ ] Optimize core with Binaryen before wrapping.
- [ ] Wrap with WIT world; leave external `[pkg:iface]` as world imports.
- [ ] Compose vendored BLAs at the component layer.
- [ ] Cache by content hash + toolchain version.
- [ ] Validate final component; publish BLA if it’s a library.
