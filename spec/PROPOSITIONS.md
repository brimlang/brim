AGENTS: Discuss, iterate, refine. Agree before updating core specs.

- [x] Match guard prefix token.
   Adopt `??` in core match arms. Ternary form `cond ?? then :: else` is moved to S0 sugar (separate proposal doc).
- [x] Unit value should only be `unit{}`. `unit` is required in type position.
   Clarified in Fundamentals and Token Reference; patterns use `()` and `!()`.
- [x] Creating named types should be expression-based.
   Adopt `TypeExpr` → TypeValue with type binding `Name[T?] := TypeExpr` (nominal if RHS is a shape literal; alias otherwise). Binding model finalized: const `=`, var `.=` (declaration and reassignment), lifecycle `~=`. Member access uses `:`; paths use `::`. Type ascription limited to binding headers (`Ident :Type`).
- [x] Consider using different pair for construction aggregates to avoid expr block visual similarity.
   Decision: keep existing `sigil+{` constructors unchanged. Rely on formatting to distinguish from blocks (no space before `{` in constructors; space before `{` for blocks).
- [x] Flag pattern algebra should avoid bitwise ops common in other languages.
   Adopt two explicit modes without bitwise operators: exact set `(a, b, ...)` and constraints `(+a, -b, ...)`. No mixing of bare and signed names; duplicates and contradictions are rejected.
- [x] A more compact form for list literals would be nice, but difficult to achieve consistency.
   Adopt S0 sugar `[e1, e2, ...]` desugaring to `list{ ... }`. Empty `[]` requires expected type or binding ascription. Core remains `list{ ... }`.
- [x] Reconsider module import syntax in light other recent binding changes.
   Adopt dedicated module bind `::=`: `alias ::= pkg::ns`. Imports are required for term-space access (no direct `pkg::ns:member`); per-item aliasing uses const or type bind based on export kind.
- [x] Supporting type casts and type assertions will be important in the core, but should be considered compiler domain.
   Adopt explicit expression operator `:>` for compile‑time checked conversions: `expr :> Type`. No runtime casts; if a conversion cannot be proven viable, emit a diagnostic. Exact coercion/narrowing/widening lattice is deferred.
