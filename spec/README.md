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
