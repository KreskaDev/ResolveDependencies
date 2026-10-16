---
title: Platform and language
description: Which runtime, language and test framework the solution is built with.
---
# Platform and language

## Context

The task statement does not prescribe a platform. The solution is a small console
program with a pure validation core and a set of golden input/output files. The choice
mainly affects how the solution is built, tested and read by a reviewer.

## Options

1. **.NET 10, C#, xUnit.** Strong typing, `record` types for the domain model, first-class
   test tooling, single `dotnet test` entry point. Requires the .NET SDK on the reviewer's
   machine.
2. **Python 3.** No build step, very short code. Weaker typing; test and packaging
   conventions vary more between projects.
3. **Node.js / TypeScript.** Comparable to option 1 in tooling, but adds a transpile step
   and a `node_modules` folder for a program of this size.

## Decision

Option 1: .NET 10 (`net10.0`), C#, xUnit. Shared build settings (`Nullable`,
`ImplicitUsings`, warnings treated as errors) live in `src/Directory.Build.props`.

## Consequences

- The reviewer needs .NET SDK 10.0 or newer; this is stated in `README.md`.
- The domain model can rely on value equality of `record struct` types.
- Only the test framework is pulled in as a package; the runtime has no dependencies.
