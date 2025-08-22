# brim
## Layout

- `Brim.Parse` — lex/parse & concrete surface concerns
- `Brim.C0` — canonical unsugared core (analysis/validation)
- `Brim.S0` — sugar expander (CST → C0), idempotent & semantics-preserving
- `Brim.Emit.WitWasm` — WIT/Wasm GC emitter
- `Brim.Tool` — single-binary CLI (`brim`)

### Quick start
```bash
mise install
mise run build
dotnet run --project src/Brim.Tool -- build --dump=wit
mise run aot-linux-x64
./artifacts/publish/Brim.Tool/release_linux-x64/Brim.Tool version

