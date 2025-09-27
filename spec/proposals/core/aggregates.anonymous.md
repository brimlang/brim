---
id: core.aggregates.anonymous
layer: core
title: Anonymous Aggregate Values
authors: ['trippwill', 'assistant']
updated: 2025-09-12
status: proposed
---

# Anonymous Aggregate Values

## Summary

This proposal introduces a canonical syntax for constructing anonymous aggregate values (structs, unions, flags, etc.) in Brim, using the `_` sigil to indicate an unnamed type. This enables concise, explicit, and consistent construction of aggregates without requiring a nominal type declaration.

## Motivation

- Enables one-off aggregate values without polluting the namespace.
- Maximizes consistency with function and aggregate binding syntax.
- Reduces token noise for simple, local, or throwaway aggregates.
- Useful for inline data, testing, or as arguments to functions.

## Specification

### Syntax

- An anonymous aggregate value is constructed using the `_` sigil followed by the aggregate shape and field/type bindings.
- The type and value are given together, with each field as `name : Type = value`.
- Only `=` (const) and `:=` (var) bindings are allowed at the top level.

#### Structs

```brim
foo := _%{id :i32 = 5, allowed :bool = true}
bar = _%{name :str = "hi", age :i32 = 42}
```

#### Unions

```brim
baz = _|{ Ok :i32 = 1, Err :str = "fail" }
```

#### Flags

```brim
mask = _&u8{ read = true, write = false }
```

### Rules

- The `_` sigil indicates the type is anonymous and not bound to a nominal symbol.
- All field/type/value pairs must be explicit.
- The anonymous value can be assigned to a variable or used inline as an expression.
- Only const (`=`) or var (`:=`) binding is allowed at the top level.
- The anonymous type is unique to the value and cannot be referenced elsewhere.

### Examples

```brim
# Anonymous struct value, var binding
foo := _%{id :i32 = 5, allowed :bool = true}

# Anonymous struct value, const binding
bar = _%{name :str = "hi", age :i32 = 42}

# Inline use as function argument
do_something(_%{id :i32 = 7, admin :bool = false})

# Anonymous union value
result = _|{ Ok :i32 = 1, Err :str = "fail" }

# Anonymous flags value
mask = _&u8{ read = true, write = false }
```

## Status

`proposed`. On acceptance, this syntax becomes canonical for anonymous aggregate values.
