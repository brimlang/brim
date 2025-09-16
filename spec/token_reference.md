---
id: reference.tokens
status: draft
title: Surface & Token Reference (Index)
layer: core
updated: 2025-09-14
authors: ['trippwill','assistant']
---

# Surface & Token Reference (Index)

Status: draft (informative). Canonical meaning is defined in accepted core specs and the Grammar. This file is a quick lookup index.

## Lexical & Separators
| Surface | Category | Notes |
|---|---|---|
| Ident | lexical | Unicode; normalization TBD; lexed as single token |
| Integer, Decimal, String, Rune | literal | |
| `-- comment` | comment | To end of line; single token |
| Terminator (newline, `;`) | separator | Collapsed runs; single token |

Greedy compound glyphs: operators and delimiters are lexed with longest-match semantics using an ASCII table. Sequences up to 3 characters (e.g., `::=`, `<<`, `|{`, `*{`, `!!{`, `[[`, `]]`, `.{`, `??`) are matched as single tokens (a 4th char may be added in future). Example: `[[` is a single token and never two `[` tokens.

## Bindings & Modules
| Surface | Category | Notes |
|---|---|---|
| `Name[T?] := TypeExpr` | type-binding | Nominal if shape literal; else alias |
| `name :Type = expr` | const | Binding header in any scope; init required |
| `@name :Type = expr` | mutable | Mutable binding; init required; reassign with `=` |
| `name ~= expr` | lifecycle | Services only; destructor at scope exit |
| `alias ::= pkg::ns` | module | Import alias (top-level only) |
| `<< Name` | module | Export const-bound symbol |

## Function Types & Values
| Surface | Category | Notes |
|---|---|---|
| `(Type, ...) Ret` | type | Function type |
| `(params) { ... }` | value | Function literal; names-only params |
| `f :(Type, ...) Ret = (params) { ... }` | binding | Named const function |
| `@f :(Type, ...) Ret = (params) { ... }` | binding | Mutable function value; rebind with `f = (..){..}` |
| `f :(x :T, ...) Ret { ... }` | binding | Combined header (const-only) |

## Member & Paths
| Surface | Category | Notes |
|---|---|---|
| `expr.member(args?)` | expression | Field/method/module member; not allowed on literals |
| `[[pkg::ns]]` | path-literal | Module header only |
| `pkg::ns::Name` | path | Used in headers and types |

## Control & Conversions
| Surface | Category | Notes |
|---|---|---|
| `expr =>` | control | Match introducer |
| `?? expr` | control | Guard after pattern |
| `expr :> Type` | conversion | Compile-time cast |

## Aggregates (Types & Constructors)
| Surface | Category | Notes |
|---|---|---|
| `Type := #{T, ...}` | type | Named tuple |
| `Type#{e, ...}` | ctor | Named tuple ctor |
| `Type := %{ f :Type, ... }` | type | Struct |
| `Type%{ f = e, ... }` | ctor | Struct ctor |
| `Type := |{ V :Type?, ... }` | type | Union |
| `Type|{ V }` / `Type|{ V = e }` | ctor | Union ctors |
| `Type := &uN{ a, ... }` | type | Flags |
| `Type&{ a, ... }` | ctor | Flags ctor |
| `list[T]` | type | List type |
| `list{e, ...}` | ctor | List ctor |

## Patterns (Type-Directed)
| Surface | Category | Notes |
|---|---|---|
| `_` | pattern | Wildcard |
| `(h, ..t)` / `()` | pattern | List |
| `(f = p, ...)` | pattern | Struct (order-insensitive) |
| `Type#(p, ...)` | pattern | Named tuple |
| `Variant(p?)` | pattern | Union |
| `(a, b, ...)` | pattern | Flags exact set |
| `(+a, -b, ...)` | pattern | Flags require/forbid |
| `?()` / `?(v)` | pattern | Option |
| `!!(e)` / `!(v)` | pattern | Result |
| `..` / `..name` | pattern | List rest |

## Builtins & Core Literals
| Surface | Category | Notes |
|---|---|---|
| `void`, `unit`, `bool`, `error` | builtin | |
| `T?`, `T!` | builtin | Option / Result |
| `list[T]` | builtin | |
| `unit{}` | literal | Unit value |

## Module System
| Surface | Category | Notes |
|---|---|---|
| `[[pkg::ns::leaf]]` | module | Header (first line) |
| `<< Name` | module | Export const-bound symbol |
| `alias ::= pkg::ns::path` | module | Import alias |

## Reserved / Future
| Surface | Category | Notes |
|---|---|---|
| `` ` `` | reserved | Macro/quoting future |
| Protocol shape `.{...}` | aggregate | Protocol type declaration via `:= .{...}` |
| `:::` | reserved | |
| `/>`, `</` | sugar | Pipes (S0 proposal) |

## Removed / Historical
| Surface | Category | Notes |
|---|---|---|
| `#(T, ...)`, `#(p, ...)` | aggregate | Anonymous tuple types/patterns |
| `#{...}` | aggregate | Anonymous tuple terms |
| `*[T]`, `*{...}`, `*(...)` | aggregate | Star list syntax |
| `?(cond)` | control | Guard sigil wrapper |
| `|Variant(p)` | pattern | Union pattern sigil |
| `f = (x :T) U { ... }` | function | Old inline form |
