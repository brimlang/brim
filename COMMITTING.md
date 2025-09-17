# Commit Message Guidelines (Conventional Commits)

Follow Conventional Commits 1.0.0 to keep history readable and tooling-friendly.

Format:

```
<type>(<scope>)!: <subject>

<body>

<footer(s)>
```

- type: one of `feat`, `fix`, `refactor`, `perf`, `docs`, `test`, `build`, `ci`, `chore`, `style`, `revert`.
- scope (optional): concise area like `lexer`, `parser`, `cli`, `diagnostics`, `format`, `build`, `tests`, `repo`.
- subject: imperative, present tense; no trailing period; keep concise (~50 chars; wrap details in body).
- breaking change: either `type(scope)!: ...` or footer `BREAKING CHANGE: ...`.
- footers: use for issue refs (`Fixes #123`, `Refs #456`) and co-authors.
- commits should be focused: one logical change per commit (mechanical format-only changes may batch under `chore(format): ...`).

Examples:

- `feat(parser): add predictive table for choice arms`
- `fix(lexer): treat '[[', ']]' as single tokens`
- `refactor(cli): extract run subcommands`
- `perf(parser)!: cap diagnostics at 512 and short-circuit`

With body and footer:

```
feat(aot): add linux-x64 publish task

Adds `mise run aot:linux-x64:publish` wiring and updates docs.

Fixes #123
```

References:
- Spec: https://www.conventionalcommits.org/en/v1.0.0/
