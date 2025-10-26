---
id: canon.unicode
layer: canon
title: Unicode & Encoding Rules
authors: ['trippwill', 'assistant']
updated: 2025-09-27
status: accepted
version: 0.1.0
canon:
  - spec/grammar.md
  - spec/unicode.md
  - spec/fundamentals.md
  - spec/sample.brim
---

# Unicode & Encoding Rules

Defines the canonical treatment of Unicode code points across tokens, literals, identifiers, and source input. Serves as a companion to `spec/grammar.md`.

## 1. Source Encoding

- Brim tooling accepts source files encoded as UTF-8.
- If the file begins with a UTF-8 byte order mark (`0xEF 0xBB 0xBF`), the reader strips the BOM silently before lexing.
- Any other encoding or ill-formed UTF-8 byte sequence triggers a hard diagnostic (`UENC001`) with the byte offset of the failure.

## 2. String Literals (`str`)

- Literal contents must decode to well-formed UTF-8; malformed sequences are rejected at lexing (`USTR001`).
- No normalization is applied. The scalar sequence the author writes (including combining marks) is preserved exactly.
- Escape sequences:
  - Simple escapes: `\n`, `\t`, `\r`, `\"`, `\\`.
  - Unicode escapes: `\u{HEX}` with 1–6 hexadecimal digits. Value must satisfy `0x0 <= value <= 0x10FFFF` and may not be in the surrogate range `0xD800–0xDFFF`; otherwise `USTR002` is emitted.
- Unrecognized escape sequences (`\q`, etc.) are rejected (`USTR003`).

## 3. Rune Literals (`rune`)

- Represent exactly one Unicode scalar value.
- After escape processing, the literal must reduce to a single scalar in range `0x0–0x10FFFF` excluding surrogates; multiple scalars or invalid values trigger `URUN001`.
- Escape table mirrors strings.
- No normalization is applied; developers may construct a `seq[rune]` or `str` for grapheme clusters.

## 4. Identifiers

- Lexical form:
  - First code point: `XID_Start` **or** `_`.
  - Subsequent code points: `XID_Continue` or `_`.
  - Characters outside these sets produce `UIDENT001`.
- Normalization:
  - Identifiers are normalized to NFC during lexing. The compiler stores the NFC form; no diagnostic is emitted for reordering differences.
  - Identifiers remain case-sensitive. `Foo`, `foo`, and `FOO` are distinct symbols.
- Disallowed characters (controls, whitespace, punctuation, emoji outside XID sets) cause `UIDENT002` with the offending code point (`U+XXXX`).

## 5. Whitespace & Terminators

- Recognized whitespace outside literals/comments:
  - ASCII space (`U+0020`), horizontal tab (`U+0009`), carriage return (`U+000D`), line feed (`U+000A`).
  - `\r\n` is collapsed to a single newline terminator.
- Statement terminator: newline only. Source code that relies on semicolons must emit an explicit newline instead.
- Any other Unicode whitespace character (NBSP `U+00A0`, zero-width spaces, `U+2028`, etc.) is rejected with diagnostic `ULEX001` unless it appears inside a literal or comment.

## 6. Comments

- Line comments start with `--` and run to the next newline terminator.
- Comment bodies may contain arbitrary Unicode scalars; malformed UTF-8 remains an error earlier in the pipeline.

## 7. Diagnostics & Rendering

- Diagnostics referring to specific characters include the code point in `U+XXXX` notation.
- Escapes in diagnostics follow the same syntax as source literals to avoid ambiguity (`"`, `\u{...}`).
- Suggested diagnostic codes:
  - `UENC001`: source is not valid UTF-8.
  - `USTR001`: malformed UTF-8 in string literal.
  - `USTR002`: Unicode escape out of range or surrogate.
  - `USTR003`: unknown escape sequence in string literal.
  - `URUN001`: rune literal must encode exactly one Unicode scalar.
  - `UIDENT001`: identifier start/continue rule violation.
  - `UIDENT002`: disallowed character in identifier (report code point).
  - `ULEX001`: unsupported whitespace character outside literals/comments.

## 8. Interop & ABI Notes

- Brim `str` values handed to WIT bindings are guaranteed to be well-formed UTF-8; bindings must validate inbound host strings before constructing Brim `str` values.
- Ill-formed host inputs should surface as `str!` (or equivalent) rather than propagating malformed data into user code.

## 9. Deferrals

- Confusable detection / security profiles (e.g., restricting identifiers to mixed scripts) are deferred.
- Additional comment forms (block comments) remain out of scope for now.
- Byte-level primitives (raw string literals, explicit encoding literals) will be addressed separately if needed.

---

## Related Specs

- `spec/grammar.md` — Lexical grammar and token definitions (authoritative for syntax)
- `spec/fundamentals.md` — Core types including `str` and `rune`
- `spec/core/numeric_literals.md` — Numeric literal encoding
- `spec/sample.brim` — Canonical examples demonstrating Unicode usage
