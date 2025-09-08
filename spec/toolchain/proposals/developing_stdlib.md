---
title: Developing stdlib
authors: ['trippwill']
date: 2025-09-08
status: draft
---
# Stdlib in Monorepo — Resolver & CI Plan
*Date: 2025-08-22*

This document specifies how the Brim toolchain behaves when the **standard library (std)** lives inside the **Brim monorepo**, and how we publish BLAs for external users.

---

## 1) Goals
- Keep **C0 source** unchanged (`text = std::text`, `stdio = [wasi:io/stdio]`).
- Fast inner-loop for contributors; no unnecessary rebuilds.
- Reproducible CI artifacts; simple consumption by early adopters.

---

## 2) Resolver Behavior (Monorepo-Aware)

### Import categories
- **Internal Brim modules** (e.g., `std::text`, `std::task`)
- **Vendored BLAs** (checked into `vendor/`)
- **External host/WASI** (e.g., `[wasi:io/stdio]` → world import)
- **Registry BLAs** (GitHub Releases / OCI)

### Default resolver order (monorepo)
1. **Lockfile** (exact fingerprint/digest)
2. **Vendor/** (if present)
3. **Local cache** (`~/.brim/cache/bla/...`)
4. **Workspace std/** (when import path starts with `std::`)
5. **Registries** (GitHub Releases and/or OCI)

### Workspace selection rules
- If dependency path matches `^std::`, try **workspace** *unless*:
  - `source = "vendor"` is set for that dep, or
  - `--frozen` build is active (lockfile + cache only).

### Overrides (per project)
```toml
# brim.toolchain.toml
[dependencies]
"brim::std::text" = { source = "workspace" }  # prefer workspace build
"wasi:io/stdio"   = { host = true }           # world import

# Freeze std to CI-produced BLA:
"brim::std::text" = { source = "vendor", path = "vendor/brim.std.text" }

# Quick swap to a dev path:
"brim::std::text" = { source = "path", path = "../std.text/.out/std.text-0.3.1.bla.tgz" }
```

---

## 3) Developer Modes

- `--workspace`: prefer rebuilding std from workspace (hack on stdlib).
- `--frozen`: use **only** lockfile + cache (fast, reproducible).
- `--vendor-std`: copy CI BLAs into `vendor/` and resolve from there.

**Ergonomics:**
```bash
# edit std/text/*
brim build std --workspace   # rebuilds only changed std packages → BLAs
brim build myapp             # composes fresh std BLAs (no full std rebuild)
```

---

## 4) CI Plan (Monorepo)

### Pipelines
1. **Detect changes** in `std/*` packages.
2. **Build once per changed package**:
   - C0 → core WasmGC → Binaryen (opt) → component wrap.
   - Emit **BLA**: `component.wasm`, `package.wit`, `abi.json`, `fingerprint`, etc.
3. **Publish to CI cache** keyed by: `(package path, toolchain ver, options, content hash)`.
4. **Downstream stages** compose apps using cached std BLAs.
5. **On tag** (`v0.3.1`): publish std BLAs to **GitHub Releases** and/or **OCI**.

### Lockfile in repo
- `brim.lock` pins each std package to CI’s **fingerprint**.
- Contributors can opt-in to workspace rebuilds with a `[patch]` entry or `--workspace`.

---

## 5) External Distribution (Early Adopters)

### GitHub Releases (zero infra)
- `brim publish` uploads:
  - `std.text-0.3.1.bla.tgz`
  - `package.wit`
  - `fingerprint` and optional `.sig`
- `brim add brim::std::text --version ^0.3` resolves via release assets.

### OCI Artifacts (GHCR/ACR)
- Tag: `ghcr.io/brimlang/std.text:0.3.1`
- `brim add brim::std::text --oci ghcr.io/brimlang/std.text --version ^0.3`

---

## 6) Security & Provenance

- **Checksum** verification using `fingerprint` (sha256) for all BLAs.
- Optional **signatures**:
  - OCI: verify with `cosign` (keyless or pinned keys).
  - Releases: `.sig` + configured publisher keys.
- **Policy hooks**: allow/deny lists for publishers at org/workspace level.

---

## 7) Failure Modes

- **Missing host import** (`[wasi:…]`): instantiation fails at runtime (expected).
- **Workspace drift**: CI rejects if std workspace build changes public WIT but lockfile expects previous fingerprint (requires `brim lock --update`).
- **Over-aggressive DCE**: ensure required core exports are marked “preserve” before Binaryen passes.

---

## 8) Examples

### In-repo contributor (workspace-first)
```bash
brim build std --workspace
brim build apps/hello
```

### Hermetic build (frozen + vendored)
```bash
brim vendor std
brim build --frozen
```

### External user
```bash
brim add brim::std::text --version ^0.3
brim build
```

---

## 9) Decisions (2025-08-22)
- Resolver is **workspace-aware** for `std::` imports.
- CI produces std **BLAs** and caches by content hash; apps compose them, don’t rebuild.
- Tagged releases publish std BLAs to GitHub Releases and/or OCI.
- Lockfile pins fingerprints; contributors can override with `--workspace` or `[patch]`.

## 10) Deferrals
- Exact CLI flag names (`--vendor-std`, etc.) and TOML schema key names.
- Registry search/ discovery (`brim search`), to be added after initial dogfooding.
