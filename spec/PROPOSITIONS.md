- [ ] Match guard prefix token, and supporting ternary or if/then like match shorthand centered on the guard prefix.
   Propose `??`.
- [ ] Unit value should only be `unit{}`. `unit` should be required in type position.
- [ ] Creating named types should be expression-based, yielding syntax like: `User = %{name :str, status :|{Active: Perms, Inactive}}` to cont bind the symbol User to the struct shape.
- [ ] Consider using different pair for construction aggregates to avoid expr block visual similarity.
- [ ] Flag pattern algebra should avoid bitwise ops common in other languages.
- [ ] A more compact form for list literals would be nice, but difficult to achieve consistency.
