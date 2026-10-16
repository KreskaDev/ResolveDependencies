---
title: Project structure
description: How the solution is split into projects and what each project may reference.
status: Proposed
date: 2026-10-16
related:
  - docs/architecture.md
  - docs/ADR/0001-platform-and-language.md
---
# Project structure

## Context

The validation logic must be unit-testable without touching the file system, and the
console entry point should stay a thin adapter. The solution is small, so the structure
should not add ceremony beyond what gives this separation.

## Options

1. **Single console project** containing logic and tests side by side. Fewest files,
   but tests would reference an executable and I/O code would mix with the algorithm.
2. **Three projects in `src/`**: `Core` (class library), `Console` (executable referencing
   Core) and `Tests` (xUnit, referencing Core only). Dependency direction is
   `Console -> Core <- Tests`.
3. **Four projects**, splitting parsing and validation into separate libraries. Cleaner
   boundaries on paper, but two libraries of a few files each.

## Decision

Option 2. One solution file (`ResolveDependencies.slnx`), three projects under `src/`,
golden data linked into the test project from the repository root.

## Consequences

- Core has no project or package references and no I/O.
- The console project is not unit-tested; it is exercised by running it against
  `goldenData/`.
- Adding a golden case means adding a file pair in `goldenData/`, nothing else.
