# Brim Diagnostics Design

This document specifies the design and usage conventions for lexical and syntactic diagnostics in the Brim toolchain. It complements the streaming parser architecture and aims to keep hot paths allocation‑light while enabling rich, consistent user‑facing messages (CLI, LSP, DAP, future IDE tooling).

---
## Goals

* Centralize construction of diagnostics (single source of truth for message semantics).
* Keep per‑diagnostic memory footprint small (value types; no per‑instance string building on hot path).
* Defer human‑readable message formatting until a presentation layer renders it.
* Support fast offset → line/column reporting without repeated mapping work.
* Allow compact encoding of small payloads (e.g., actual token kind plus up to 3 expected kinds) without heap allocations.
* Enable future extensibility (related spans, quick fixes, localization) without redesign.

## Non‑Goals (Current)

* Localization infrastructure (messages are rendered in English only for now).
* Rich multi‑span related information (can be added incrementally).
* Semantic / type diagnostics (syntax only at this stage).

---
## Data Model

````csharp
public enum DiagnosticId {
    UnexpectedToken,
    MissingToken,
    UnterminatedString,
    UnterminatedBlockComment,
    InvalidCharacter,
    // Extend with new ids here.
}

public enum DiagnosticSeverity { Error, Warning, Info }

// Value type to avoid per-instance heap overhead.
public readonly record struct Diagnostic(
    DiagnosticId Id,
    DiagnosticSeverity Severity,
    int Offset,
    int Length,
    ushort Line,
    ushort Column,
    uint Data0,
    uint Data1,
    uint Data2
);
````

### Packing Strategy

`Data0..Data2` are generic 32‑bit slots. Common encodings:
* UnexpectedToken:
  * `Data0` = (uint)actualTokenKind
  * `Data1` = packed expected token kinds (see below)
  * `Data2` = metadata (e.g. expected count, truncation flag)
* InvalidCharacter: `Data0` = UTF-16 code unit of offending char
* Unterminated constructs: `Data0` = (uint)openingTokenKind

Example expected set packing (≤3 kinds, consistent with LL(k ≤ 3)):
````csharp
static uint PackExpected(TokenKind a, TokenKind b, TokenKind c) {
    // 10 bits per kind (supports up to 1024 distinct TokenKind values)
    return ((uint)a & 0x3FF) | (((uint)b & 0x3FF) << 10) | (((uint)c & 0x3FF) << 20);
}

static (TokenKind a, TokenKind b, TokenKind c) UnpackExpected(uint packed)
    => ((TokenKind)(packed & 0x3FF),
        (TokenKind)((packed >> 10) & 0x3FF),
        (TokenKind)((packed >> 20) & 0x3FF));
````

If fewer than 3 expected kinds are used, trailing slots are set to a sentinel (often `TokenKind.None`). `Data2` lower 8 bits can store the actual expected count (0–255 covers future expansion).

---
## Factory API

````csharp
public static class DiagnosticFactory {
    public static Diagnostic UnexpectedToken(in Token actual, TokenKind e0, TokenKind e1, TokenKind e2, byte expectedCount, SourceText src) { /* ... */ }
    public static Diagnostic MissingToken(TokenKind expectedKind, int insertionOffset, SourceText src) { /* ... */ }
    public static Diagnostic UnterminatedString(in Token start, SourceText src) { /* ... */ }
    public static Diagnostic UnterminatedBlockComment(in Token start, SourceText src) { /* ... */ }
    public static Diagnostic InvalidCharacter(int offset, char ch, SourceText src) { /* ... */ }
}
````

Guidelines:
* Factories compute line/column once via `SourceText` line map and embed them in the `Diagnostic` (fast for downstream display & LSP ranges).
* Do not allocate arrays or lists within factory methods.
* Reuse helper packing utilities for expected token sets.

---
## Rendering Layer

A separate renderer converts a `Diagnostic` into a human-readable message (CLI, logs, LSP publishDiagnostics):

````csharp
public static class DiagnosticRenderer {
    public static string ToMessage(Diagnostic d) => d.Id switch {
        DiagnosticId.UnexpectedToken => RenderUnexpected(d),
        DiagnosticId.MissingToken => RenderMissing(d),
        DiagnosticId.UnterminatedString => "unterminated string literal",
        DiagnosticId.UnterminatedBlockComment => "unterminated block comment",
        DiagnosticId.InvalidCharacter => RenderInvalidChar(d),
        _ => d.Id.ToString()
    };
}
````

Message formatting rules:
* Only decode packed data inside renderer (not on hot path).
* Avoid slicing source text unless essential (e.g., for invalid character display); when required, limit to minimal spans.
* Future localization: switch renderer implementation; factories remain unchanged.

---
## Usage in Parser / Lexer

Lexer example (invalid character):
````csharp
_diags.Add(DiagnosticFactory.InvalidCharacter(_pos, ch, _source));
````

Parser example (unexpected token at start of statement):
````csharp
if (!IsStartOfStatement(Current.Kind)) {
    _diags.Add(DiagnosticFactory.UnexpectedToken(CurrentToken, TokenKind.Identifier, TokenKind.Let, TokenKind.Eof, 3, _source));
    Advance(); // ensure progress
}
````

Missing token insertion point (e.g., expecting closing paren):
````csharp
if (Current.Kind != TokenKind.RightParen) {
    _diags.Add(DiagnosticFactory.MissingToken(TokenKind.RightParen, Current.Offset, _source));
}
````

---
## Performance Considerations

* `Diagnostic` value size target: ≤ 32–40 bytes (current layout ~32 bytes depending on runtime padding).
* No per‑diagnostic heap allocations (arrays, strings) until rendering.
* Packing/unpacking uses simple bit ops—branchless and fast.

### Why Not Store Message Strings Directly?

* Avoids repeated string formatting during parsing of erroneous code.
* Enables localization / customization later without rewriting factories.
* Keeps diagnostics comparable structurally (helpful for testing and tooling).

---
## Testing Strategy

* Factory unit tests: verify line/column and packed data fields for representative tokens.
* Round‑trip tests: create diagnostic → render → ensure output includes expected token kind names.
* Edge cases: 0 expected kinds (e.g., stray closing token), 1 expected kind (missing specific punctuator), 3 expected kinds (typical LL(k) boundary), invalid character outside BMP (?) if encountered (ensure `char` to code unit works).

---
## Extensibility Guidelines

When adding a new diagnostic:
1. Add a new `DiagnosticId` enum member.
2. Implement a factory method (pack any payload into `Data0..2`).
3. Add renderer case to produce human-readable message.
4. Update docs (this file and a short note/usage example if needed).
5. Prefer reusing existing packing encodings before inventing a new scheme.

If a diagnostic requires more than 3 x 32-bit slots:
* First consider compact encoding (e.g., 10 bits per token kind, small integer ranges).
* If genuinely insufficient, consider introducing a secondary storage (e.g., pool-backed struct with pointer index) but only after profiling proves necessity.

---
## Future Directions

* Related information (secondary spans) for constructs like mismatched delimiter pairs.
* Quick fix hints (code + title) for IDE lightbulb integrations.
* Severity elevation (warnings vs info) for style / lint diagnostics once those layers exist.
* Localization by mapping `DiagnosticId` + payload to resource templates.

---
## Summary

The DiagnosticFactory pattern ensures consistent, low-overhead diagnostic creation in a streaming parser. By separating structural data capture (factory) from presentation (renderer), Brim preserves performance while remaining flexible for future tooling enhancements.
