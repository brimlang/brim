# AI Agent Guidance for Brim Specs

- Treat `spec/grammar.md` as the single source of truth for syntax and token definitions. Do not duplicate grammar in other files.
- Update semantics in the corresponding file under `spec/core/`. Each topic has one file (e.g., `services.md`, `expressions.md`). Keep behavior, typing rules, and invariants there.
- Place runtime/ABI details in `spec/runtime/` and tooling documentation in `spec/tools/`; syntactic sugar belongs in `spec/sugar/`.
- When editing both syntax and semantics, touch the relevant documents in the same change set and keep examples in sync.
- Draft proposals live under `spec/proposals/` in a directory structure that mirrors `spec/`. Keep them there until fully accepted; once accepted, merge the content into `grammar.md` and the relevant semantic file, then delete the proposal copy.
- Avoid cross-file duplication. Link by path reference when necessary, but rely on the directory structure to communicate scope.
- Before writing new spec text, skim `spec/README.md` and `spec/template.md` to align with existing conventions (headings, front matter, tone).
- Do not run code or tests expecting the specs to be executable—they are documentation only.
