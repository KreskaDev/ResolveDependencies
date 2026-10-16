---
title: Conflict reporting
description: Whether the validator stops at the first version conflict or collects all of them, and what it returns.
---
# Conflict reporting

## Context

The required program output is only `PASS` or `FAIL`. The traversal could stop at the
first conflicting package or continue and gather every package that is required in more
than one version. The richer result helps diagnostics and makes unit tests more precise,
at the cost of a larger result type.

## Options

1. **Stop at the first conflict, return `Pass` / `Fail`.** Smallest API, the validator
   is a predicate. Tests assert only the verdict.
2. **Collect all conflicts, return `Valid` or `Invalid(conflicts)`** where each conflict
   names the package and the set of versions. Enables precise assertions and a verbose
   diagnostic mode later. Slightly more code and a result type to design.
3. **Stop at the first conflict but return it** together with the verdict. Middle ground;
   still needs a result type, but reports only one of possibly several problems.

## Decision

Option 1. `DependencyValidator` returns a `ValidationResult` enum with `Pass` and
`Fail`. Returning the conflicting pair can be added later without changing the
architecture.

## Consequences

- The console output maps the enum to `PASS` / `FAIL` at the boundary.
- Unit tests check the verdict only; which conflict triggered it is covered by the
  shape of the test input.
- `tech_design.md` sections 5.1, 5.3 and 6 describe option 2 and must be aligned once
  this record is accepted.
