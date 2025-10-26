# AI Agent Guidance for Brim Specs

## Quick Reference

- Treat `spec/grammar.md` as the single source of truth for syntax and token definitions. Do not duplicate grammar in other files.
- Function syntax and declaration forms are documented in `spec/functions.md` (canon layer). Do not duplicate in `spec/core/`.
- Update semantics in the corresponding file under `spec/core/`. Each topic has one file (e.g., `services.md`, `expressions.md`). Keep behavior, typing rules, and invariants there.
- Place runtime/ABI details in `spec/runtime/` and tooling documentation in `spec/tools/`; syntactic sugar belongs in `spec/sugar/`.
- When editing both syntax and semantics, touch the relevant documents in the same change set and keep examples in sync.
- Draft proposals live under `spec/proposals/` in a directory structure that mirrors `spec/`. Keep them there until fully accepted; once accepted, merge the content into `grammar.md` and the relevant semantic file, then delete the proposal copy.
- When doing broad language design work, you may ignore `spec/proposals/` unless specifically asked to evaluate a proposal; focus on the accepted core specs.
- Avoid cross-file duplication. Link by path reference when necessary, but rely on the directory structure to communicate scope.
- Before writing new spec text, skim `spec/README.md` and `spec/template.md` to align with existing conventions (headings, front matter, tone).
- Do not run code or tests expecting the specs to be executable—they are documentation only.
- Combined function declaration shorthand (`name :(param :Type, ...) Ret { body }`) is implemented; keep `spec/functions.md`, `spec/grammar.md`, and examples synchronized when modifying it.

---

## Maintenance Guidelines

### Canonical Sync Set

The following five files must always describe the same language. When you update one, audit all five in the same change set:

1. `spec/grammar.md` — Canonical grammar (syntax only)
2. `spec/fundamentals.md` — Core laws, binding rules, basics primer
3. `spec/unicode.md` — Unicode & encoding rules
4. `spec/functions.md` — Function declaration forms
5. `spec/sample.brim` — Canonical sample code

**Rule:** Before committing any change to the canonical sync set, verify all five files remain consistent. Update examples in `sample.brim` to reflect syntax changes.

### Before Adding New Spec Files

1. **Check if content belongs in an existing file**
   - Don't create new files for small additions
   - Extend existing topic files when possible

2. **Verify layer placement**
   - `spec/` root → Canonical sync set only (grammar, fundamentals, unicode, functions, samples)
   - `spec/core/` → Semantic specifications for core language features
   - `spec/proposals/` → Draft proposals (mirror structure of accepted specs)
   - `spec/runtime/` → Runtime behavior, ABI, memory model
   - `spec/sugar/` → Syntactic sugar and convenience features
   - `spec/tools/` → Toolchain, CLI, formatter, linter guidance

3. **Add entry to `spec/README.md` spec map**
   - Include file path, brief description, status
   - Organize by layer

4. **Use the template**
   - Copy `spec/template.md` for front matter
   - Include required fields: id, layer, title, authors, updated, status
   - Add version field only for `accepted` specs

### Avoiding Duplication

Each concept should have **one authoritative source**. Other files provide overview + cross-reference.

**Pattern:**
- Primary file: Full documentation with examples and rules
- Secondary files: Brief mention + "see [primary file]" link

**Examples:**
- **Functions:** `spec/functions.md` is authoritative. `spec/core/expressions.md` defers to it.
- **Patterns:** `spec/core/patterns.md` is authoritative. Type files reference it for pattern details.
- **Match expressions:** `spec/core/match.md` is authoritative. `spec/core/expressions.md` provides overview only.
- **Aggregates:** `spec/core/aggregates.md` is authoritative. Other files reference it for construction/pattern syntax.

**Cross-reference format:**
```markdown
For complete documentation, see `spec/core/[topic].md`.
```

Or with link:
```markdown
For complete documentation, see [Topic](core/topic.md).
```

### Pattern for New Topics

When documenting a new language feature:

1. **Syntax** → Add productions to `spec/grammar.md`
2. **Semantics** → Create or extend `spec/core/[topic].md`
3. **Examples** → Add to `spec/sample.brim` (canonical) or `spec/examples/` (illustrative)
4. **Proposals** → Draft in `spec/proposals/[layer]/[topic].md` until accepted

**Do not:**
- Duplicate grammar productions in semantic files
- Repeat semantic rules in multiple files
- Create topic files for syntax-only features (keep in grammar.md)

### Syntax vs. Semantics Separation

Maintain clear boundaries:

- **`spec/grammar.md`** — Syntax only (productions, tokens, structural templates)
- **`spec/fundamentals.md`** — Core laws, binding operators, statement separators
- **`spec/core/*.md`** — Type system semantics, evaluation rules, typing judgments

**Example: Terminators**
- Grammar defines `TERM` token: `TERM : {NEWLINE | SEMICOLON}+`
- Fundamentals defines statement separator policy: "Semicolons and newlines are interchangeable terminators"

### When Updating Canonical Sync Set

**Checklist:**
1. Update all affected grammar productions in `grammar.md`
2. Update core laws or binding rules in `fundamentals.md` if applicable
3. Update encoding rules in `unicode.md` if applicable
4. Update function forms in `functions.md` if applicable
5. Add or update examples in `sample.brim` to demonstrate changes
6. Update any `spec/core/` files that reference the changed feature
7. Document changes in commit message using Conventional Commits format
8. Update front matter `updated` field in all modified files

### Related Specs Sections

Each spec file should include a "Related Specs" or "See Also" section listing files that cover related topics.

**Format:**
```markdown
## Related Specs

- `spec/core/expressions.md` — Expression forms and evaluation
- `spec/core/aggregates.md` — Aggregate construction and patterns
- `spec/grammar.md` — Grammar productions for match expressions
```

### Sample Files

- **`spec/sample.brim`** — Canonical sample, part of sync set, must demonstrate all core features
- **`spec/examples/*.brim`** — Illustrative examples, non-canonical, can demonstrate proposals or advanced patterns

Add header comments to example files explaining their purpose:
```brim
-- This example demonstrates inventory management patterns
-- Status: illustrative (not canonical)
```

### Proposals Workflow

1. **Draft:** Create in `spec/proposals/[layer]/[topic].md` with status `draft`
2. **Review:** Change status to `proposed` when ready for feedback
3. **Acceptance:** 
   - Change status to `accepted`
   - Add version number
   - Merge content into canonical/core specs
   - Delete proposal file (or mark deprecated)
4. **Rejection:**
   - Change status to `deprecated` with rationale
   - Keep for historical reference

### Pattern Documentation

Patterns are type-directed and should be documented with the types they match:

- **Overview:** `spec/core/patterns.md` — Pattern matching semantics, exhaustiveness, guards
- **Type-specific patterns:** Document in the type's file (aggregates.md, option_result.md, services.md)
- **Match expressions:** `spec/core/match.md` — Match syntax and arm evaluation

### Quality Checklist

Before finalizing spec changes:

- [ ] No duplication of content from other specs
- [ ] Cross-references use consistent format
- [ ] Examples compile with current grammar
- [ ] Front matter is complete and accurate
- [ ] Canonical sync set is consistent (if applicable)
- [ ] README.md spec map is updated (for new files)
- [ ] Related specs section is present
- [ ] Commit message follows Conventional Commits format

---

## Common Tasks

### Adding a New Language Feature

1. Draft grammar productions in `spec/grammar.md`
2. Add semantic rules to new or existing `spec/core/[topic].md`
3. Add examples to `spec/sample.brim`
4. Update fundamentals.md if core laws are affected
5. Test examples parse correctly
6. Update README.md spec map if new file created

### Refactoring Existing Feature

1. Update grammar.md if syntax changes
2. Update semantic file in spec/core/
3. Update all examples in sample.brim and spec/examples/
4. Search for cross-references and update them
5. Verify canonical sync set consistency
6. Update related specs sections

### Documenting a Pattern

1. Add overview to `spec/core/patterns.md` if new pattern form
2. Document type-specific details in the type's file (aggregates.md, etc.)
3. Add match examples to `spec/core/match.md`
4. Update `spec/sample.brim` with canonical examples
5. Cross-reference from related files

### Resolving Duplication

1. Identify the authoritative source (usually most detailed file)
2. Reduce secondary files to brief overview + link
3. Add "See [authoritative file]" references
4. Move examples to authoritative source
5. Update cross-references throughout specs

---

## File Quick Reference

| File | Purpose | Contains |
|------|---------|----------|
| `grammar.md` | Canonical grammar | Syntax productions, tokens, templates |
| `fundamentals.md` | Core primer | Laws, binding rules, basics |
| `unicode.md` | Encoding | Unicode rules, identifiers, literals |
| `functions.md` | Functions | Function types, values, declarations |
| `sample.brim` | Canonical code | Examples demonstrating core features |
| `core/expressions.md` | Expressions | Expression forms and evaluation |
| `core/aggregates.md` | Aggregates | Struct, union, tuple, flags, seq |
| `core/patterns.md` | Patterns | Pattern matching semantics |
| `core/match.md` | Match | Match expressions and arms |
| `core/option_result.md` | Option/Result | `T?`/`T!` types and propagation |
| `core/services.md` | Services | Services, protocols, constraints |
| `core/generics.md` | Generics | Type parameters and constraints |
| `examples/*.brim` | Examples | Illustrative code (non-canonical) |
