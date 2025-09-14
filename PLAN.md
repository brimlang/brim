• Phase 1 (Lexical & Minimal Parsing Skeleton) — COMPLETE
  • Colon-based aggregate declaration predictions in place (≤4 lookahead).
  • Required tokens (#{ ?{ !{ etc.) validated/present.
  • Legacy '=' aggregate heads retained temporarily (deprecation diagnostic pending removal in Phase 8).
  • Unified TypeDeclaration refactor deferred (see Phase 8 / AST Refactor Outline).
• Phase 2 (Named Tuples & Flags Modernization) — COMPLETE
  • NamedTupleDeclaration parsed (#{ T1, T2 } form).
  • Modern flags syntax : &u8{ a, b } supported; legacy forms still accepted (pending Phase 8 removal).
  • Underlying integral type capture and member parsing stable.
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
• Phase 6 (Expression Layer Foundations) — TODO
  • Minimal expression parser: identifiers, literals, aggregate constructions Type%{}, Type|{ Variant }, flags Type&{}, tuple Type#{}, option/result constructors.
  • Postfix propagation operators bind tighter than application.
• Phase 7 (Match, Patterns, Loops) — TODO
  • Pattern AST: Wildcard, Identifier, TuplePattern, VariantPattern, Option/Result Patterns, FlagPattern.
  • Parse match arms: pattern (guard?) => expr.
  • Loop parsing: @{ ... @} with @> (continue) and <@ expr (break).
• Phase 8 (Cleanup & Removal) — TODO
  • Remove legacy '=' aggregate declarations and obsolete tests after grace period.
  • Retire GenericDeclaration.Parse intermediate in favor of unified TypeDeclaration pipeline.
• Phase 9 (Documentation & Tests) — IN PROGRESS (partial)
  • Existing tests mostly migrated to colon syntax; NamedTupleParseTests present; GenericConstraintTests added.
  • Pending: OptionResultTypeTests, ServiceProtocolParseTests, FlagsModernSyntaxTests, deprecation regression suites.
  • Keep spec cross-references minimal; avoid duplicating spec text.
AST Refactor Outline


• Introduce AggregateShapeKind enum: Struct, Union, Flags, Tuple, Service, Protocol.
• TypeDeclaration: holds Name, GenericParams?, ShapeKind, ShapeNode (fields / variants / tupleElements / flagsMembers / serviceState / protocolMethods), Metadata (implements list, underlying flag type).
• Replace separate top-level declarations in prediction table with single factory selecting shape after colon.
