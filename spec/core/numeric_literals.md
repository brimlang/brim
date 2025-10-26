---
id: core.numeric_literals
title: Numeric Literals
layer : core
authors: ['trippwill', 'assistant']
updated: 2025-01-28
status: accepted
version: 0.1.0
---

# Numeric Literals

## Overview

Brim integer literals support decimal, hexadecimal, and binary forms. Underscores may separate digits for readability but must
appear between digits. A type suffix may follow the digits; the suffix is required when no surrounding context provides a type
so that every identifier's type is known at declaration.

## Grammar

```ebnf
IntegerLiteral ::= (Decimal | Hex | Binary) TypeSuffix?
Decimal        ::= Digit ( "_"? Digit )*
Hex            ::= "0x" HexDigit ( "_"? HexDigit )*
Binary         ::= "0b" BinaryDigit ( "_"? BinaryDigit )*
TypeSuffix     ::= ("i" | "u") ("8" | "16" | "32" | "64")
Digit          ::= "0".."9"
HexDigit       ::= Digit | "a".."f" | "A".."F"
BinaryDigit    ::= "0" | "1"
```

When a literal appears without an explicit type annotation or other inference, a suffix such as `i32` or `u8` must be supplied.

## Examples

```brim
decimal : i32 = 12345
big     : i32 = 1_000_000
hex     : i32 = 0xFF_EC_DE_5E
binary  : i32 = 0b1010_0110
count   := 0i32
```

---

## Related Specs

- `spec/fundamentals.md` — Core types and primitive overview
- `spec/unicode.md` — Literal lexing and encoding rules
- `spec/grammar.md` — Numeric literal grammar productions
