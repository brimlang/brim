# Spec Overview

Brim language specs are organized by layer with clear separation between syntax (grammar), canonical reference (fundamentals, functions), and semantic elaboration (core/).

Each spec file includes front matter describing status and version; accepted specs represent the canonical behavior.

## Canonical Sync Set

The following five files must always describe the same language. When you update one, audit all five in the same change set and refresh examples so they stay aligned. If you discover a mismatch, treat it as a bug report and reconcile before landing any other spec work.

1. **`spec/grammar.md`** — Canonical grammar (syntax only)
2. **`spec/fundamentals.md`** — Core laws, binding rules, basics primer
3. **`spec/unicode.md`** — Unicode & encoding rules
4. **`spec/functions.md`** — Function declaration forms
5. **`spec/sample.brim`** — Canonical sample code

---

## Spec Map

### Canonical Layer (Root)

These files define the authoritative language specification:

| File | Status | Description |
|------|--------|-------------|
| `grammar.md` | accepted | Canonical grammar: syntax productions, tokens, structural templates |
| `fundamentals.md` | accepted | Core laws, binding operators, statement separators, basics primer |
| `unicode.md` | accepted | Unicode & encoding rules for identifiers, literals, source input |
| `functions.md` | accepted | Function types, value declarations, combined declaration form |
| `sample.brim` | canonical | Comprehensive sample demonstrating all core language features |
| `AGENTS.md` | guidance | Maintenance guidelines and contributor reference |
| `template.md` | template | Spec file template with front matter requirements |

### Core Semantics (`core/`)

Semantic specifications for core language features. Each file covers one major topic.

| File | Status | Description |
|------|--------|-------------|
| `core/aggregates.md` | accepted | Structs, unions, named tuples, flags, sequences |
| `core/expressions.md` | draft | Expression forms and evaluation rules |
| `core/generics.md` | accepted | Type parameters, constraints, inference |
| `core/match.md` | accepted | Match expressions, arms, guards, exhaustiveness |
| `core/numeric_literals.md` | accepted | Integer/decimal literal syntax and type suffixes |
| `core/option_result.md` | accepted | `T?`/`T!` types, constructors, propagation operators |
| `core/patterns.md` | accepted | Pattern matching semantics and type-directed patterns |
| `core/reserved_extra.md` | draft | Reserved tokens for future use |
| `core/services.md` | accepted | Services, protocols, constraints, lifecycle |

### Examples (`examples/`)

Illustrative code samples (non-canonical):

| File | Description |
|------|-------------|
| `examples/inventory.brim` | Inventory management example |

### Proposals (`proposals/`)

Draft proposals for language extensions, organized by layer:

- `proposals/core/` — Core language extensions
- `proposals/sugar/` — Syntactic sugar proposals
- `proposals/emit/` — Emission and compilation targets
- `proposals/tools/` — Toolchain and CLI improvements

See `proposals/README.md` for proposal workflow and status.

### Runtime (`runtime/`)

Runtime behavior, memory model, ABI specifications (future):

- Placeholder for runtime semantics documentation

### Sugar (`sugar/`)

Syntactic sugar and convenience features (future):

- Placeholder for sugar layer documentation

### Tools (`tools/`)

Toolchain, formatter, linter, CLI guidance (future):

- Placeholder for tooling documentation

---

## Navigation Guide

- **Learning Brim:** Start with `fundamentals.md`, then `grammar.md` and `sample.brim`
- **Language reference:** Use `grammar.md` for syntax, `core/` files for semantics
- **Function syntax:** See `functions.md` (authoritative)
- **Pattern matching:** See `core/patterns.md` (overview) and `core/match.md` (match expressions)
- **Type system:** See `core/aggregates.md`, `core/option_result.md`, `core/services.md`, `core/generics.md`
- **Contributing:** Read `AGENTS.md` for maintenance guidelines

---

## Cross-Reference Conventions

Specs should reference related files using consistent format:

```markdown
For complete documentation, see `spec/core/[topic].md`.
```

Or with relative links:
```markdown
See [Topic](core/topic.md) for details.
```

Each spec should include a "Related Specs" section listing relevant files.
