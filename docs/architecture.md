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
  files, folders, the console or the text format. Its public API accepts a structurally
  valid `InstallationRequest` and returns a `ValidationResult`; whether the installation
  is valid is the only question Core answers.
- **Console** references Core. It adapts the file system and the command line to Core:
  enumerates the input files, sanitizes and parses their text into an
  `InstallationRequest`, hands it to Core, writes the output files and the console
  summary. It contains no domain logic.
- **Tests** reference Core and Console. Core is covered through its domain service,
  Console through its sanitizer and parser; file enumeration and output writing are
  covered by running the application against `docs/story/goldenData/`.

## Console application

- Responsibilities: arguments, reading and sanitizing the input files, parsing them into
  the Core contract, writing the output files and the console summary, exit codes.
- No dependency-injection container or logging framework; the app is small enough that a
  plain `Main` is clearer.
