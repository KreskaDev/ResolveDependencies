# Architecture

This document describes the structure of the solution and the design decisions behind it.
It intentionally covers only architecture. Build, run and usage instructions live in
[`README.md`](../README.md); the problem statement lives in [`abstract.md`](../abstract.md).

## Goals and constraints

- Solve the "resolve dependencies" task: given packages to install and a dependency graph,
  decide whether the installation is valid (`PASS`) or requires two versions of the same
  package (`FAIL`).
- Keep the core logic independent of any I/O so it can be unit-tested in isolation.
- Keep the solution small. No external runtime dependencies; only the test framework is pulled
  in as a package.

## Technology

| Concern           | Choice                                                              |
|-------------------|---------------------------------------------------------------------|
| Platform          | .NET 10 (`net10.0`), C#                                             |
| Test framework    | xUnit                                                               |
| Project layout    | Single solution (`.slnx`), three projects in `src/`; shared build settings in `Directory.Build.props` |
| Language features | `Nullable` and `ImplicitUsings` enabled; warnings treated as errors |

## Repository layout

```
ResolveDependencies.slnx
Directory.Build.props                shared build settings (TFM, nullable, warnings as errors)
abstract.md                          problem statement
README.md                            build / run / test instructions
docs/
  architecture.md                    this document
testdata/
  inputNNN.txt / outputNNN.txt       golden test cases (input + expected result)
src/
  ResolveDependencies.Core/          class library: parsing + validation logic
  ResolveDependencies.Console/       console app: CLI entry point, file system I/O
  ResolveDependencies.Tests/         xUnit tests for Core (unit + golden tests)
```

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
  running it against `testdata/` rather than by unit tests.

## Core design

### Domain model

| Type                  | Kind            | Purpose                                                                                  |
|-----------------------|-----------------|------------------------------------------------------------------------------------------|
| `PackageVersion`      | `record struct` | `(Name, Version)` pair. Value equality is what the whole algorithm relies on.            |
| `Dependency`          | `record`        | `Dependent` → `Requirement`, both `PackageVersion`.                                      |
| `InstallationRequest` | `record`        | Parsed input: list of packages to install + list of dependencies.                        |
| `ValidationResult`    | `enum`          | `Pass` / `Fail`. Mapped to the `PASS` / `FAIL` strings only at the output boundary.      |

### Components

```
 text ──► InputParser ──► InstallationRequest ──► DependencyValidator ──► ValidationResult
```

- **`InputParser`** – turns the raw text of one input file into an `InstallationRequest`.
  Pure function, throws a `FormatException` with a line number on malformed input.
- **`DependencyValidator`** – takes an `InstallationRequest` and decides `Pass` / `Fail`.
  Pure function, no exceptions for valid-but-failing inputs.
- **`DependencyResolver`** (facade) – `Resolve(string input) => ValidationResult`. The one
  entry point the console app calls; composes parser and validator.

### Validation algorithm

1. Build a lookup `PackageVersion → list of required PackageVersion` from the dependency
   list (adjacency list of a directed graph).
2. Start a traversal (BFS/DFS) from every package in the install list.
3. Maintain `Dictionary<string packageName, string version>` of everything that will end up
   installed (requested packages plus everything transitively required).
4. When visiting a `PackageVersion`:
   - if the name is already present with a **different** version → `Fail`, stop;
   - if it is already present with the **same** version → already visited, skip
     (this is what makes cycles and self-dependencies terminate);
   - otherwise record it and enqueue its requirements.
5. If the traversal finishes without a conflict → `Pass`.

Complexity is linear in the number of packages and dependencies. Only versions that are
actually reachable from the install list matter; dependencies of versions nobody installs are
ignored (see `input004`: `A,1 → B,1` is irrelevant because `A,2` is installed, not `A,1`).

### Input format decisions

The specification in `abstract.md` is minimal; the golden files in `testdata/` show the
real edge cases. The parser is defined to handle all of them:

| Case                                                     | Example                                          | Decision                                                                                                                        |
|----------------------------------------------------------|--------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------|
| Trailing blank lines                                     | most files                                       | Ignored. Lines are trimmed; empty lines at the end of the file are skipped.                                                     |
| Missing dependency section                               | `input008`                                       | Treated as zero dependencies. Only the install section is required.                                                             |
| One dependency line with several requirements            | `input002`: `B,2,A,1,C,1`                        | Parsed as `B,2` depending on **each** following `p,v` pair (`A,1` and `C,1`). A line must have an even number of fields, at least four. |
| Self-dependency                                          | `input003`: `B,1,B,1`                            | Allowed; handled naturally by the visited check.                                                                                |
| Cycle                                                    | `input006`: `A,1 → B,2 → A,1`                    | Allowed; handled by the visited check.                                                                                          |
| Dependency of a version that is never installed          | `input009`: `A,1,B,2` while `A,2` is installed   | Ignored; only reachable versions are followed.                                                                                  |
| Package or version containing `,`                        | n/a                                              | Not supported, guaranteed by the task statement.                                                                                |
| Line endings                                             | n/a                                              | `\n` and `\r\n` both accepted.                                                                                                  |

Anything else (non-numeric count, fewer lines than declared, odd number of fields) is a
`FormatException` from the parser, which the console app reports as an error for that file
and continues with the next one.

## Console application

- Single responsibility: adapt the file system and the CLI to Core.
- Input: path to a folder. Processes every `*.txt` file whose name starts with `input`,
  sorted by name, and prints one `PASS` / `FAIL` line per file to standard output. It does
  **not** write `output*.txt` files, so running it against `testdata/` never modifies the
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
3. **Golden tests.** A `[Theory]` driven by `MemberData` that enumerates `testdata/input*.txt`,
   runs `DependencyResolver.Resolve` and compares against the matching `output*.txt`
   (trimmed). The `testdata/` folder is linked into the test project and copied to the output
   directory, so the repository remains the single source of truth for the cases.

Adding a new case is therefore just adding an `inputNNN.txt` / `outputNNN.txt` pair.

## Out of scope

- Version range / semantic-version comparison. Versions are opaque strings compared for
  exact equality, as the task defines them.
- Reporting *which* conflict caused a `FAIL`. The required output is only `PASS` / `FAIL`;
  the validator could be extended to return the conflicting pair without changing the
  architecture.
- Parallel processing of input files. Inputs are tiny.
