• Phase 3 (Generic Constraints) — PARSING COMPLETE / DIAGNOSTICS PARTIAL
  • GenericParameter parsed as Name (':' Constraint ('+' Constraint)*)?.
  • Parameter list now stores (Name, Constraints[]).
  • Core constraint parsing and single basic diagnostic implemented; full EGEN001–EGEN007 set remains TODO.

• Phase 4 (Option/Result Type Postfix) — TODO
  • Add type parser support for postfix ? / ! creating OptionTypeNode / ResultTypeNode.
  • Decide and document chaining precedence (likely restrict ambiguous multi-postfix chains).
  • Add diagnostics for misuse (postfix applied outside type contexts).

• Phase 5 (Services & Protocols) — TODO
  • Parse protocols: Proto[...]? : .{ methodSig (, methodSig)* }.
  • Parse services: Service[...]? : ^recv{ state... } (: Proto ('+' Proto)*)? = { members }.
  • Members: constructors ^(forms), methods (reuse FunctionDeclaration after expression layer), destructor ~().
  • Diagnostics: missing receiver, duplicate protocols, invalid implements lists.

- Phase 6 (Define basic operators and precedence) — TODO
  • Define a minimal set of operators (e.g., arithmetic, logical, comparison) with clear precedence and associativity rules.
  • Implement parsing rules to respect operator precedence and associativity.
  • Add diagnostics for common operator misuse (e.g., mixing incompatible types).
  - Precedence table: As operators evolve, add a single authoritative precedence/associativity table (even if sparse now) to avoid drift between files. -- TODO

• Phase 7 (Expression Layer Foundations) — TODO
  • Minimal expression parser: identifiers, literals, aggregate constructions Type%{}, Type|{ Variant }, flags Type&{}, tuple Type#{}, option/result constructors.
  • Postfix propagation operators bind tighter than application.

• Phase 8 (Match, Patterns, Loops) — TODO
  • Pattern AST: Wildcard, Identifier, TuplePattern, VariantPattern, Option/Result Patterns, FlagPattern.
  • Parse match arms: pattern (guard?) => expr.
  • Loop parsing: @{ ... @} with @> (continue) and <@ expr (break).
  - List rest patterns: Nail down `..rest` rules and exhaustiveness interactions; add 2–3 canonical examples. -- TODO


- At any time:
  Improve comment attachment in SignficantProducer.
