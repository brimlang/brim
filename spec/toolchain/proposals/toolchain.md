---
title: Brim tool
authors: ['trippwill']
date: 2025-09-08
status: draft
---
# Brim toolchain (proposal)

Brim’s toolchain is layered and plugin-driven. This document summarizes the pipeline and supporting infrastructure.

## Architecture & Layers

- **C0:** core compiler that parses, validates, and produces canonical AST.
- **S0:** standard syntactic sugar layer; desugars to C0.
- **E:** emitter stage producing WIT/WASM.
- **V:** validator/analysis stages.
- **Plugins:** optional transforms that run between layers.

## Pipeline Configuration

`brim.toolchain.toml` declares active layers (listed last-to-first):

```toml
[pipeline]
layers = ["C0", "S0", "E"]
```

A lockfile pins plugin versions for reproducibility. The pipeline must end with a core-only stage.

## Plugin Model

- Manifest JSON describes name, version, entry point.
- Plugins run in a sandboxed WASM host.
- Deterministic output and structured diagnostics are required.
- Hosts and IDEs integrate via LSP/DAP, exposing pipeline info and diagnostics.

## S0 Charter

- Provides ergonomic sugar only; no new semantics.
- Each sugar must desugar to C0.
- Governance: proposal → experiment → `std.s0` → versioned release.
- S0 versions track Brim core versions.

## WASM GC Emission

- Emitters always target WASM GC reference types.
- Toolchains reject configurations targeting non-GC hosts.
- `brimc` depends on `wasm-tools` / `wit-bindgen` with GC support.

## Decisions vs Deferrals

**Decided**
- Plugin model and config format.
- C0/S0 boundary.

**Deferred**
- Typed transforms across modules.
- Cross-file plugin passes.
- Registry/distribution specification.
- Pretty export name annotations.

