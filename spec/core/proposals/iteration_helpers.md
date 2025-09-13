---
id: core.iteration_helpers
layer: core
status: draft
title: Iteration Helpers (Stdlib)
authors: ['assistant']
updated: 2025-09-13
---

# Iteration Helpers (Draft)

## Status & Scope
Draft proposal for a minimal set of standard library iteration / recursion helpers. Pure library layer: introduces **no new syntax** and **no parser changes**. All helpers are ordinary functions expressible in existing core forms.

## Motivation
Removal of the bespoke loop surface emphasizes recursion + pattern matching. While explicit recursion is sufficiently expressive, several patterns are verbose or risk accidental stack depth until tail recursion optimization or trampolining is standardized. A tiny, orthogonal set of helpers improves readability and guidance without enlarging the core grammar.

Target properties:
- Zero new syntax or tokens (library only)
- Composable, referentially transparent
- Encourage structural/unfold style over mutable indexing
- Amenable to future optimization (inliner / TCO / fusion) without semantic change

## Design Principles
- Prefer explicit accumulator passing (clarity over hidden mutation)
- Separate generation (producer) from consumption (fold / map)
- Expose early termination via sum types (`T?`, `T!`) rather than control statements
- Avoid baking in laziness: start with strict list-returning forms; allow streaming variants later

## Proposed Initial Set
Names are illustrative; final names to follow stdlib naming conventions.

### 1. tail_rec
Structured tail recursion via an explicit step function returning an Option for continuation.

Signature (illustrative):
```
tail_rec[T] : (state : T, step : (T) T?) T
```
Semantics:
- Invoke `step(state)`; if `?{}` (nil) => yield `state` as final; if `?{next}` => continue with `next`.
- Encourages explicit final state representation.

Example (integer increment until threshold):
```
res = tail_rec(0i32, (n) => n < 10i32 => ?{ n + 1i32 } | ?{})
```

### 2. iterate
Generate a list by repeated stepping with bounded count or predicate.
```
iterate[T] : (seed : T, step : (T) T, count : i32) list[T]
```
- Produces `count` states: `[seed, step(seed), step(step(seed)), ...]`.
- Variant with predicate could stop when predicate fails (not both forms initially to keep surface minimal).

### 3. unfold
Dual of fold: build a list from a seed until the function signals completion.
```
unfold[S, T] : (seed : S, next : (S) (S, T)?) list[T]
```
- `next(seed)` returns `?{ (s', value) }` to continue, or `?{}` to stop.

### 4. fold
Left fold over a list.
```
fold[T, U] : (xs : list[T], acc : U, f : (U, T) U) U
```
- Standard catamorphism; tail-recursive.

### 5. fold_while
Fold with early termination.
```
fold_while[T, U] : (xs : list[T], acc : U, f : (U, T) (U?)) U
```
- `f` returns `?{}` to stop (yield current acc), or `?{nextAcc}` to continue.

### 6. find
Return first element matching predicate.
```
find[T] : (xs : list[T], pred : (T) bool) T?
```

### 7. any / all
Boolean summary predicates (can short-circuit via fold_while).
```
any[T] : (xs : list[T], pred : (T) bool) bool
all[T] : (xs : list[T], pred : (T) bool) bool
```

### 8. map
Standard map (already showable via recursion; included for completeness & guidance).
```
map[T, U] : ((T) U, list[T]) list[U]
```
(Existing example in Fundamentals; ensure consistent signature.)

### 9. range
Finite integer range construction.
```
range : (start : i32, end_exclusive : i32, step : i32) list[i32]
```
- Preconditions: `step != 0`; sign of `step` must move toward end; violations produce `error` in a future Result-returning variant.

## Example Rewrites
Original (conceptual) loop counting to sum squares (now removed):
```
-- pseudo-loop style (removed)
@{ acc = 0i32; i = 0i32
  i >= n <@ acc
  acc = acc + i * i
  i = i + 1i32
@}
```
Rewrite using unfold + fold:
```
squares = unfold((0i32, 0i32), (state) => {
  (i, acc) = state
  i >= n => ?{}
  ?{ ((i + 1i32, acc + i * i), acc + i * i) }
})
result = fold(squares, 0i32, (a, v) => a + v)
```
Simpler with tail_rec:
```
result = tail_rec((0i32, 0i32), (state) => {
  (i, acc) = state
  i >= n => ?{}
  ?{ (i + 1i32, acc + i * i) }
})
-- final state is (n, sumSquares)
```

## Non-Goals (Initial)
- Laziness / streaming iterators (future service- or protocol-based design)
- Parallel iteration primitives
- Indexed zips (can be composed with `range` + `map` for now)
- Mutation-based builders (stay purely functional first)

## Open Questions
1. Should `tail_rec` return the last state or allow a projection `(T) U`? (Variant: `tail_rec_result`.)
2. Is `fold_while` necessary initially, or do we expect users to encode early stop via Option in a combined fold? (Tradeoff: discoverability.)
3. Provide infix / pipeline sugar later vs relying on prefix calls now.
4. Naming: prefer concise (`fold`, `unfold`) vs namespaced (`list::fold`). Decision couples to module organization of stdlib.
5. Should `iterate` and `unfold` both exist initially, or can one desugar to the other? (`iterate(seed, step, count)` can be an `unfold` special case.)
6. Error signaling: stick to Option now; introduce Result variants later when domain errors (not structural termination) needed.

## Alternatives Considered
- Reintroducing a loop syntax (rejected: increases grammar & token set; duplicates recursion power)
- CPS / trampoline baked into core (premature without measured stack issues)
- Lazy sequence protocol first (added complexity & potential performance overhead before need)

## Migration
No legacy surface: previous loop syntax already purged. Users write direct recursion today; these helpers add clarity but are optional.

## Next Steps
1. Gather feedback on helper set & naming.
2. Validate recursion depth characteristics on representative workloads.
3. Prototype implementations in stdlib module (`std::iter` placeholder).
4. Add tests for edge cases (empty lists, early termination, negative steps in `range`).
5. Consider adding benchmarking harness to evaluate tail recursion elimination once optimizer exists.
