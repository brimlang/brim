---
id: core.reserved_extra
layer : core
title: Additional Reserved Tokens
authors: ['trippwill']
updated: 2025-09-08
status: draft
---
# Additional Reserved Tokens

Token reserved for possible future use or to avoid potential parsing ambiguities:

- Backtick (for potential macro/quoted forms)
- Leading `.` followed by ident (kept for possible meta operators)—current list literal form uses `.{`, so no collision.
- `:::` triple colon (avoid accidental namespace inventing)
- T? (optional type shorthand)
- T! (result type shorthand)
- /> </ (pipe and reverse pipe operators)
- @id (decorator/attribute syntax)
