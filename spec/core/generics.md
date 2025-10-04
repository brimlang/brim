---
id: core.generics
title: Generics
layer: core
authors: ['trippwill']
updated: 2025-09-14
status: accepted
version: 0.1.0
---

# Generics

Defines canonical syntax & minimal semantics for parametric polymorphism.

## Scope
Applies to: functions, struct types, union types, services, protocols, builtin parametric types (`T?`, `T!`, `seq[T]`, `buf[T; N]`).
Excludes: higher-kinded types, value generics, variadics, partial application.

## Parameter Lists

Form attached immediately after the declared symbol:
```
Name[T, U]
Name[T :Eq]
Map[K :Eq + Hash, V]
```
Grammar (informal):
```
GenericParams  ::= '[' (GenericParam (',' GenericParam)* (',')?)? ']'
GenericParam   ::= Ident (':' ConstraintList)?
ConstraintList ::= ProtocolRef ('+' ProtocolRef)*
```
Rules:
- Single optional trailing comma permitted.
- Empty list `[]` has no comma.
- Duplicate parameter name → EGEN001.
- Duplicate protocol in a list → EGEN002.
- Unknown protocol → EGEN003.

Colon spacing: space before colon, none after (matches Brim style): `T :Eq + Show`.

## Constraints
Any generic parameter may include zero or more protocols after a single colon.
All listed protocols must be satisfied (conjunction).

Examples:
```brim
Pair[T, U] := #{T, U}
Cache[K :Eq + Hash, V] := %{ entries :seq[Pair[K, V]] }
all_equal[T :Eq] :(xs :seq[T]) bool = { ... }
```

## Services & Protocols
Protocols:
```brim
Iterable[T] : .{ next : () T? }
Show[T]     : .{ show : (T) str }
```
Services with implements list (unified colon form):
```brim
Logger :^log{ target :str } :Fmt + Flush = {
  ^(to : str) = { log.target = to }
  to_string : () str = () => log.target
  ~() unit = { }
}
```
Generic + constraints on service parameters:
```brim
Store[K :Eq, V] :^s{ /* state */ } :Flush = { /* body */ }
```

## Builtin Parametric Types
Remain unchanged syntactically:
- `T?` (option type)
- `T!` (result type)
- `seq[T]` (growable sequence)
- `buf[T; N]` (fixed-length buffer)
Constraints do not attach inside these forms; they attach where parameters are introduced.

*Note: The previous `opt[T]` and `res[T]` types are replaced by `T?` and `T!` as per the Option/Result & Return Lifting spec.*

## Instantiation & Inference
Call sites and type uses rely on inference; no explicit type argument application syntax in core:
- Function generic parameters MUST be inferable from argument types and/or contextual expected return type.
- Missing inference → EGEN004.
- Unused generic parameter → EGEN005.
- Conflicting inferred types → EGEN006.

Example:
```brim
map[T, U] :((T) U, seq[T]) seq[U] = (f, xs) {
  xs =>
    (h, ..t) => seq.concat(seq{ f(h) }, map(f, t))
    ()       => seq{}
}
res = map((x) { x + 1 }, seq{1,2,3})
-- infers T=i32, U=i32
```

## Patterns
Patterns never restate generic arguments:
```brim
Reply[T] := |{ Good : T, Error : str }
val :Reply[i32] = Reply|{ Good = 42 }
val =>
  |(Good(v)) => v
```

## Diagnostics (Seed)
- EGEN001 duplicate generic parameter name
- EGEN002 duplicate protocol in constraint list
- EGEN003 unknown protocol in constraint list
- EGEN004 cannot infer generic parameter
- EGEN005 unused generic parameter
- EGEN006 conflicting inferred types for parameter
- EGEN007 protocol constraint not satisfied

## Open Deferrals
- Explicit type application syntax
- Variance annotations
- Higher-kinded parameters
- Partial specialization
