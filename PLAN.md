# Brim Language Implementation Plan

**Status Legend**: [X] Complete | [P] Partial | [ ] Not Started

## Current Implementation State (2025-10-24)

**Completed Major Features:**
- Lexer with trivia handling and significant token production
- Module structure parsing (headers, imports, exports, declarations)
- Type system core: struct, union, tuple, flags, protocol, service shapes
- Type postfix operators (`?`, `!`) for optional/fallible types
- Expression parsing with operators, precedence climbing, literals, aggregates
- Function literals (lambdas) with structured parameter lists
- Match expressions with guards
- Value declarations (mutable and immutable)
- Generic parameters and constraints

**In Progress:**
- Pattern parsing (currently using expression nodes as temporary bridge)
- Service implementation surface (type and state shapes done; member surface incomplete)

**Next Priorities:**
- Dedicated pattern AST nodes
- Service constructors, methods, destructors
- Aggregate construction expressions (field/variant initialization)
- Loop constructs (`@{ ... @}` with control flow)

---

## Detailed Phase Tracking

- [X] **PRIORITY PHASE - PRESERVE ALL SIGNIFICANT TOKENS IN ORDER IN GREEN NODES**
    - [x] Current state and gaps
        - [x] Already preserved as trailing commas on list elements:
            - [x] Generic args/params: `GenericArgument`, `GenericParameter` include `TrailingComma?`.
            - [x] Union/struct/named tuple lists: `UnionVariantDeclaration`, `FieldDeclaration`, `NamedTupleElement` include `TrailingComma?`.
            - [x] Protocol methods: `MethodSignature` includes `TrailingComma?`.
            - [x] `ModulePath` preserves separators as explicit tokens in `Parts`.
            - [x] Terminators are explicit `GreenToken` members in the module body.
        - [X] Gaps to fix to meet the rule uniformly:
            - [X] FunctionType parameter list: currently stores `StructuralArray<GreenNode>` (commas dropped). Needs element nodes with optional `TrailingComma`.
            - [X] ServiceShape protocol refs list: currently drops commas. Needs element nodes with optional `TrailingComma`.
    - [X] Service impl:
        - [X] StateBlock field list: currently drops commas. `ServiceStateField` should carry optional `TrailingComma`.

- [X] **PRIORITY PHASE – Module-level typed value bindings**
    - [x] Surface: support `Ident ':' Type '=' Initializer Terminator` and `'^' Ident ':' Type '=' Initializer Terminator` at module scope (init required).
    - [x] Parser: add a Module member prediction for `Identifier ':'` (const) and `'^' Identifier ':'` (mutable); parse header and capture initializer through the next Terminator (structure-only for now).
    - [x] AST: add `ValueDeclaration` node with optional `^` mutability, header tokens, `=` and `Terminator`.
    - [X] Tests:
        - [X] Parse: Covered in ExpressionParsingTests (lambda literals, match expressions with value bindings).
        - [X] Error: No-initializer case handled with diagnostic in ValueDeclaration.ParseAfterName.
    - [X] Docs: spec/sample.brim contains canonical examples (lines 22-24, 28-29); spec/grammar.md documents surface (lines 88-89).

- [X] **Phase 4 (Option/Result Type Postfix)**
    - [X] Add type parser support for postfix `?` / `!` creating optional/fallible type suffixes.
    - [X] TypeExpr.Parse handles `?` and `!` suffixes as GreenToken stored in TypeExpr.Suffix.
    - [X] No chaining: single suffix only (enforced by parser structure).
    - [ ] Add diagnostics for misuse if needed (currently parsing captures suffix cleanly).

- [P] **Phase 5 (Services & Protocols)**
    - [X] Parse protocol shapes: `.{ methodSig (, methodSig)* }` via ProtocolShape node.
    - [X] Parse service state shapes: `@{ field, ... }` via ServiceShape node.
    - [P] Parse service type declarations with protocol bounds and implementations.
    - [ ] Members: constructors `^(forms)`, methods, destructor `~()`.
    - Note: Service declarations exist in SyntaxKind but full implementation surface incomplete.

- [X] **Phase 6 – Expression Layer & Operators**
    - [X] Operator catalogue, precedence tiers documented in spec/grammar.md lines 121-175.
    - [X] Expression parser implemented: identifiers, literals, parenthesized, aggregates (struct, union, tuple, flags, service constructions).
    - [X] Postfix propagation operators (`?`, `!`, `!!`) implemented in PropagationExpr; bind tighter than application.
    - [X] Expression grammar in Parser.Expressions.cs with LL(k≤4) maintained.
    - [X] Expression green node hierarchy in src/Brim.Parse/Green/Expressions/.
    - [X] SyntaxKind extended for expression nodes.
    - [X] Parser.Expressions.cs partial contains primary/call/propagation/binary/unary parsing (356 lines).
    - [X] Precedence climbing for binary operators implemented.
    - [X] Function literals parse with structured LambdaParams nodes.
    - [X] Match expressions implemented with guards and arms (MatchExpr, MatchArm, MatchGuard).
    - [X] Block expressions implemented with statements (AssignmentStatement, ExpressionStatement).
    - [X] Diagnostics and recovery paths for expression parsing failures present.
    - [X] Parser tests in ExpressionParsingTests.cs; spec/sample.brim and spec/grammar.md updated.
    - [ ] Function short-hand combined decl and definition.

- [X] **Phase 7 (Construct Expressions & Aggregates)**
    - [X] Basic aggregate construction syntax parsed (struct `%{}`, union `|{}`, tuple `#{}`, flags `&{}`).
    - [X] Field initialization nodes for struct/service constructions.
    - [X] Variant initialization for union constructions.
    - [X] Linear collection constructions: `seq[T]{ ... }`, `buf[T]{ ... }`.
    - [X] Option/Result wrapper constructions: `?{ expr }`, `!{ expr }`, `!!{ expr }`.

- [P] **Phase 8 (Match, Patterns, Loops)**
    - [X] Match expressions with guards implemented (MatchExpr, MatchArm, MatchGuard).
    - [P] Pattern AST: Currently patterns parsed as expressions (ExprNode); need dedicated pattern nodes.
    - [ ] Pattern nodes: Wildcard, Identifier, TuplePattern, VariantPattern, Option/Result Patterns, FlagPattern.
    - [ ] Parse match arms: pattern (guard?) => expr with proper pattern parsing (not expression parsing).
    - [ ] Loop parsing: `@{ ... @}` with `@>` (continue) and `<@ expr` (break) — currently deferred.
    - [ ] List rest patterns: Nail down `..rest` rules and exhaustiveness interactions; add 2–3 canonical examples.

- [ ] **Phase 9 (Lifecycle & Control Flow)**
    - [X] Service lifecycle declarations (constructors with `^`, destructors with `~`).
    - [X] Lifecycle binding syntax: `name :Type ~= expr` parsed.
    - [ ] Loop parsing: `@{ ... @}` with `@>` (continue) and `<@ expr` (break).
    - [ ] Defer statements for cleanup.

- [ ] **Semantic Backlog (post-parse)**
    - [ ] Module bindings: reject exports of mutable symbols (or document semantics).
    - [ ] Module bindings: consistent error for stray `:` without a following `=` at module scope.
    - [ ] Services & protocols: diagnostics for missing receiver, duplicate protocols, invalid implements lists.
    - [ ] Expressions: diagnostics for common operator misuse (e.g., mixing incompatible types).
    - [ ] Pattern exhaustiveness checking for match expressions.
    - [ ] Type checking and inference.

- [ ] **Future Work**
    - [ ] Semantic layer: bindings, scope resolution, type inference.
    - [ ] Warning pass: unused imports, unreachable code, unused bindings.
    - [ ] Formatter: canonical code style enforcement.
    - [ ] LSP server: completions, go-to-definition, diagnostics.
    - [ ] Code generation: WASM via WIT/Component Model.

- [ ] **At any time**
    - [ ] Improve comment attachment in `SignificantProducer`.
    - [ ] Performance profiling and optimization of hot paths.
