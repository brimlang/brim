# Brim Diagnostics Design

This document describes the CURRENT implemented diagnostic model (explicit fields) and the DEFERRED packed model (original proposal). The implementation intentionally chose clarity and straightforward access over early bit‑packing; the packed design remains feasible if/when profiling indicates the need.

---
## 1. Goals (Active)

* Single shared list collected during lexing + parsing.
* Zero string allocation on hot path (messages rendered later).
* Predictable, trivially inspectable struct layout (no decoding logic required to write tests).
* Support LL(4) expected token reporting (up to 4 unique first tokens from prediction table).
* Fast line/column capture (taken directly from tokens as they are produced).

## 2. Non‑Goals (For Now)

* Localization / message templating.
* Multi‑span related info.
* Semantic / type layer diagnostics.
* Packed field encodings (postponed; see §6).

---
## 3. Current Data Model (Implemented)

Defined in `Diagnostics.cs`:

````csharp
public enum DiagCode {
    UnexpectedToken,
    MissingToken,
    UnterminatedString,
    InvalidCharacter,
}

public readonly struct Diag {
    public readonly DiagCode Code;
    public readonly int Offset;
    public readonly ushort Length;
    public readonly ushort Line;
    public readonly ushort Column;
    public readonly ushort ActualKind;      // RawTokenKind when relevant
    public readonly byte   ExpectedCount;   // 0..4
    public readonly ushort Expect0;
    public readonly ushort Expect1;
    public readonly ushort Expect2;
    public readonly ushort Expect3;
    public readonly uint   Extra;           // variant payload (char code, etc.)
}
````

Rationale:
* Direct fields outperform ad‑hoc packing until counts are large.
* Debuggability & test ergonomics outweigh the small (currently theoretical) memory gain of bit packing.
* Size remains modest (fits in < 32 bytes on typical runtimes after padding).

### 3.1 Factory Helpers

`DiagFactory` creates instances without allocations. Expected kinds are passed as a span and copied into up to 4 slots with deduplication performed upstream (parser collects unique K1 values).

### 3.2 Renderer

`DiagRenderer` converts a `Diag` to a human‑readable message on demand. No caching yet (cost is minimal relative to I/O at CLI boundaries).

---
## 4. Emission Points

| Layer | Diagnostic Codes | Notes |
|-------|------------------|-------|
| Lexer | InvalidCharacter, UnterminatedString | Added immediately when token recognized / fails to terminate. |
| Parser | UnexpectedToken, MissingToken | Parser aggregates expected K1 tokens per prediction table miss; MissingToken emitted when required syntax kind not present. |

The parser always guarantees progress (advances on unexpected) preventing infinite loops.

---
## 5. Testing (Implemented)

`DiagnosticsTests` cover:
* UnexpectedToken (number literal at start of module).
* MissingToken (incomplete struct declaration).
* InvalidCharacter (isolated `$`).
* UnterminatedString (`"hello`).

Future enhancements: snapshot message rendering tests, and invariants (e.g., `ExpectedCount` matches non‑zero `Expect*` fields, uniqueness of expected kinds).

---
## 6. Deferred Packed Representation (Original Proposal)

The prior design specified a generic 3x `uint` payload (`Data0..Data2`) plus enum id + severity. This remains viable; converting would:
* Shrink struct by ~4–8 bytes (depending on padding) only if expected fields could be tightly packed.
* Add indirection (packing/unpacking helpers) harming test readability.
* Provide easier future expansion beyond 4 expected kinds (currently not needed: LL(4) cap).

Migration trigger criteria:
1. Memory profiling shows diagnostic storage is a measurable portion of total allocations for large erroneous files.
2. We require more than 4 expected kinds or multiple additional payload fields.
3. Localization requires stable numeric template keys (packing could help separate language‑agnostic payload).

Until then, explicit fields are simpler.

---
## 7. Follow‑Ups / Backlog

Short‑term:
* Dedupe cascaded MissingToken bursts (e.g., consecutive required tokens) to reduce noise.
* Add severity concept (currently all implicit errors) if/when warnings appear.
* Add test asserting renderer output contains actual & expected kind names.
* Implement `UnterminatedBlockComment` (lexer does not yet produce multi‑line block comments).
* CLI: group diagnostics by line or sort by offset (currently insertion order).

Medium:
* Provide structured JSON emission for tooling (LSP bridge).
* Add quick fix hint scaffolding (e.g., insert missing terminator) via optional metadata struct.
* Introduce suppression / filtering (e.g., only first N errors) for very noisy inputs.

Longer‑term:
* Localization: map `DiagCode` + payload to resource templates.
* Related spans (e.g., open vs missing close brace pairing).
* Consider packed representation if trigger criteria met (see §6).
* Optional pooling of large diagnostic bursts for pathological inputs.

---
## 8. Guidelines for Adding a Diagnostic (Current Model)
1. Add new enum member to `DiagCode`.
2. Add factory method in `DiagFactory` (reuse existing field semantics; use `Extra` for small scalar payloads).
3. Extend `DiagRenderer` with message case.
4. Write unit test (struct field assertions + rendered message if applicable).
5. Update this document's Follow‑Ups list if new capabilities introduced.

---
## 9. Summary

Brim currently favors a transparent, explicit diagnostic struct that is easy to reason about, test, and render. The more compact packed design remains documented but postponed until real data justifies the complexity. This keeps iteration velocity high while preserving a clear upgrade path.
