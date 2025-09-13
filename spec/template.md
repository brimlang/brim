---
id: layer.topic # required
layer: core|sugar|emit|std|toolchain # required
title: Your Title # required
authors: ['your-handle'] # required
updated: YYYY-MM-DD # required
status: draft|proposed|accepted|deprecated # required
version: 0.0.0 # optional; include when status is accepted
---

# Spec Template

## Front Matter

- id: Unique identifier for the spec - layer.topic
- layer: The layer this spec belongs to - core, sugar, emit, std, toolchain
- title: Title of the spec
- authors: List of authors
- updated: Date of last update
- status: Status of the spec - draft, proposed, accepted, deprecated
- version: release version when this spec was accepted - only valid for `accepted` specs

## Specification Guidelines

> Pre-release Note: During the pre-release phase, specs record only the current canonical state. Do not add migration notes, deprecation guidance, or legacy syntax. When behavior changes, replace the content outright. Historical context may be kept separately outside the spec tree if needed.


- Only documents with status `accepted` are considered authoritative with regard to syntax. For other statuses, the
document is informative only, and will be aligned with the canonical spec upon acceptance.
- Specs should be as concise as possible while still being clear.
- Specs should include examples.
- Specs should avoid redundancy with other specs; if a concept is defined elsewhere, link to it.
- Prefer specification of syntax by example rather than formal grammar where possible.
- New specs begin as status `draft`, then move to `proposed` when ready for review, and finally to `accepted` when
approved.
- `Proposed` specs should use canonical syntax, but may include non-normative discussion of alternatives. If the
proposal is a syntax change, the spec should include before-and-after examples.
- `Accepted` specs should avoid discussion of alternatives, and negative examples (what is not allowed).
- `Accepted` specs must include a version number indicating the release in which the spec was accepted.
- Specs with status `deprecated` are no longer authoritative, but remain for historical reference.
- Non-canonical references must always remain status `draft`.
- See `spec/template.md` for a spec template.
