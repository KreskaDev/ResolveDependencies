---
title: Architecture
description: Structure of the solution, dependency direction between projects, core components, validation algorithm and testing strategy.
related:
  - docs/tech-design.md
  - docs/test-design.md
  - docs/story/ResolveDependencies.md
---
# Architecture

This document covers only architecture.

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
- **Console** references Core. It is responsible for argument handling, enumerating input
  files in a folder, reading them, calling Core and writing results to standard output.
  It contains no domain logic.
- **Tests** reference Core only. The console project is a thin adapter and is covered by
  running it against `docs/story/goldenData/` rather than by unit tests.

## Console application

- Single responsibility: adapt the file system and the CLI to Core.
- Input: path to a folder. Processes every `*.txt` file whose name starts with `input`,
  sorted by name, and prints one `PASS` / `FAIL` line per file to standard output. It does
  **not** write `output*.txt` files, so running it against `docs/story/goldenData/` never modifies the
  golden files.
- Exit code `0` when all files were processed, non-zero on usage errors or when any file
  failed to parse.
- No dependency-injection container or logging framework; the app is small enough that a
  plain `Main` is clearer.

## Testing strategy

Tests live in `ResolveDependencies.Tests` and target Core only.

1. **Unit tests – parser.** Each row of the "input format decisions" table has a dedicated
   test, plus negative tests for malformed input.
2. **Unit tests – validator.** Hand-written `InstallationRequest` instances covering: no
   dependencies, direct conflict, transitive conflict, cycle, self-dependency, unreachable
   conflicting dependency.
3. **Golden tests.** A `[Theory]` driven by `MemberData` that enumerates `docs/story/goldenData/input*.txt`,
   runs `DependencyResolver.Resolve` and compares against the matching `output*.txt`
   (trimmed). The `docs/story/goldenData/` folder is linked into the test project and copied to the output
   directory, so the repository remains the single source of truth for the cases.

Adding a new case is therefore just adding an `inputNNN.txt` / `outputNNN.txt` pair.
