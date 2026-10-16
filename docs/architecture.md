---
title: Architecture
description: Structure of the solution, dependency direction between projects and the responsibility of each project.
related:
  - docs/tech_design.md
---
# Architecture

## Goals and constraints

- Keep the core logic independent of any I/O so it can be unit-tested in isolation.
- Keep the solution small.

## Technology

| Concern           | Choice                                                              |
|-------------------|---------------------------------------------------------------------|
| Platform          | .NET 10 (`net10.0`), C#                                             |
| Test framework    | xUnit                                                               |
| Language features | `Nullable` and `ImplicitUsings` enabled; warnings treated as errors |

## Projects and dependency direction

```
ResolveDependencies.Console ──► ResolveDependencies.Core ◄── ResolveDependencies.Tests
```

- **Core** has no project references and no package references. It knows nothing about
  files, folders or the console. Its public API accepts text (`string` / `TextReader`) and
  returns plain result types.
- **Console** references Core. It adapts the file system and the command line to Core:
  enumerates the input files, feeds their text to Core, writes the output files and the
  console summary. It contains no domain logic and no parsing code.
- **Tests** reference Core only. The console project is a thin adapter and is covered by
  running it against `docs/story/goldenData/` rather than by unit tests.

## Console application

- Single responsibility: adapt the file system and the command line to Core.
- No dependency-injection container or logging framework; the app is small enough that a
  plain `Main` is clearer.
