# brim

> Pre-release: The Brim language and toolchain are in an early, unstable phase. Breaking changes may land at any time without deprecation. Specs in `spec/` describe only the current accepted state; historical/migration notes are intentionally omitted until stabilization.

## Layout

- `Brim.Parse` — lex/parse & concrete surface concerns
- `Brim.C0` — canonical unsugared core (analysis/validation)
- `Brim.S0` — sugar expander (CST → C0), idempotent & semantics-preserving
- `Brim.Emit.WitWasm` — WIT/Wasm GC emitter
- `Brim.Tool` — single-binary CLI (`brim`)

### Quick start

```bash
mise trust
mise install
