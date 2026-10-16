---
title: Parser location
description: Whether the text parser lives in the Core library or in the console application.
status: Proposed
date: 2026-10-16
related:
  - docs/architecture.md
  - docs/tech-design.md
  - docs/ADR/0002-project-structure.md
---
# Parser location

## Context

Two documents disagree. `architecture.md` places `InputParser` in Core: Core accepts
text and returns a result, the console only handles the file system, and the test project
references Core alone. `tech-design.md` places the sanitizer/parser in the console
application so that Core receives only already valid structures.

## Options

1. **Parser in Core.** Core exposes both `InputParser` (text to `InstallationRequest`)
   and `DependencyValidator` (request to result), plus a facade composing them. Parsing is
   still a pure function with no I/O. The console stays a thin adapter with no logic to
   test. The test project references Core only.
2. **Parser in Console.** Core exposes only the validator and the domain model. The
   console owns the text format. Parser unit tests then need a reference to the
   executable project or a fourth project for the parser.
3. **Parser in its own library.** Clean boundary, but one more project for a few files.

## Decision

Option 1. The parser is part of Core, in its own namespace, and Core also exposes the
validator directly for callers that already hold an `InstallationRequest`.

## Consequences

- `tech-design.md` sections 3.1 and 5.2 must be reworded once this record is accepted:
  "what the core sees" becomes the validator's input, not Core's only input.
- The console project contains no parsing code and needs no unit tests.
- Golden tests can call the Core facade with the file text and nothing else.
