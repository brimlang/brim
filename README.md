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
mise run build        # or: dotnet build
mise run test         # run all tests
mise run lint         # verify formatting and analyzers
```

### CLI usage

Use mise targets to run the CLI:

- Lex a file (raw tokens):
  - `mise rx lex ./demo.brim`
- Parse and show the tree:
  - `mise rx parse ./demo.brim`
- Parse with diagnostics:
  - `mise rx parse --diagnostics ./demo.brim`

### Specs

- Core grammar (draft): `spec/core/grammar.md`
- Token/surface index: `spec/token_reference.md`
- Core topical specs live under `spec/core/syntax/` and `spec/core/`.

Pre‑release policy: specs describe only the current accepted state; compatibility is not guaranteed until stabilization.
