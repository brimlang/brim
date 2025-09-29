# Spec Overview

Brim language specs are organized by layer:

- **Core**
  - `spec/grammar.md` — canonical grammar.
  - `spec/unicode.md` — Unicode & encoding rules (identifiers, literals, source input).
  - `spec/core/fundamentals.md` — core laws, binding rules, primitive types.
  - `spec/core/aggregates.md` — aggregate types (`seq`, `buf`, structs, unions, flags).
  - `spec/core/expressions.md`, `spec/core/functions.md`, `spec/core/services.md`, etc.
- **Proposals**
  - `spec/proposals/` — sugar, core extensions, toolchain ideas.
- **Runtime / Tooling**
  - `spec/runtime/`, `spec/tools/` — implementations and CLI guidance.

Each spec file includes front matter describing status and version; accepted specs represent the canonical behaviour.

## Canonical Sync Set

`spec/sample.brim`, `spec/grammar.md`, `spec/fundamentals.md`, and `spec/unicode.md` must describe the same language at all times. When you update one, audit the others in the same change set and refresh examples so they stay aligned. If you discover a mismatch, treat it as a bug report and reconcile the four files before landing any other spec work.
