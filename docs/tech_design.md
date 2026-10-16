---
title: Tech Design
description: Semantic findings derived from the golden data, input and output format rules, data model, algorithm and test coverage.
related:
  - docs/architecture.md
  - docs/story/ResolveDependencies.md
  - docs/story/goldenData/
---
# Tech Design

## 1. Semantic findings

| # | Finding | Source |
|---|---------|--------|
| S1 | Dependencies are **transitive**. If `A,2` is installed and `A,2 -> C,3 -> D,4 -> B,1`, then `B,1` is required. | 005, 009 |
| S2 | **Cycles** in the dependency graph are allowed and are not an error. The algorithm must keep a visited set. | 006 |
| S3 | A package depending on **itself** in the same version is valid. | 003 |
| S4 | Rules for **versions that are not installed** (neither directly nor transitively) are ignored. | 004, abstract example 1 |
| S5 | One package version may have **several dependency rules**. If they point to different versions of the same package, the installation is invalid. | 001 |
| S6 | A conflict may arise between two dependencies, not only between a dependency and the install list. | 007 |
| S7 | A package that appears only as a dependency target (not on the install list) is installed and does not break the result unless it introduces a version conflict. | 004 |
| S8 | Package and version are **arbitrary strings without a comma**, compared **exactly** (case-sensitive, no normalisation). Versions are not interpreted semantically; `1` and `1.0` are different versions. | 000 (`Beta-1`), assumption |
| S9 | No dependency rule for a given version means that version has no dependencies. | 004 (`C,3`) |
| S10 | Duplicates on the install list in the same version are allowed. The same package in two versions on the install list is `FAIL`. | assumption, no test |
| S11 | An empty install list (N = 0) yields `PASS`. | assumption, no test |
| S12 | One input file = one test case. | goldenData |

## 2. Input format

### 2.1 Canonical format (what the core sees)

The core does not read files. It receives an already parsed structure:

```
InstallRequest
  packages:     list of pairs (package, version)            -- to be installed
  dependencies: list of (package, version) -> list of (package, version)
```

All strings are non-empty and contain no comma. Several entries for the same
`(package, version)` pair in `dependencies` are allowed; the core merges them.

### 2.2 Text file format (what the console app reads)

The description from `docs/story/ResolveDependencies.md`, extended with the deviations found in `docs/story/goldenData/`.

```
N                        number of packages to install
p,v                      x N
M                        number of dependency lines
p1,v1,p2,v2[,p3,v3...]   x M
```

Detailed rules:

| # | Rule | Source |
|---|------|--------|
| F1 | Fields are separated by a comma. Package and version contain no comma. | abstract |
| F2 | A dependency line has an **even number of fields, at least 4**. The first pair is the dependent package, every following pair is one of its dependencies. `B,2,A,1,C,1` means `B,2 -> A,1` and `B,2 -> C,1`. | 002 (deviation from abstract) |
| F3 | The dependency section (the `M` line and what follows) **may be absent**. Treated as M = 0. | 008 |
| F4 | Line endings: `\n`, `\r\n` or no trailing newline. All equivalent. | goldenData |
| F5 | Empty lines (after trim) at the end of the file are ignored. Empty lines inside a section are a format error. | 003, 007 |
| F6 | Leading and trailing whitespace of a line is trimmed. Whitespace inside a field is **not** trimmed; it is part of the name. | assumption |
| F7 | N and M must be non-negative integers and must match the actual number of lines in the section. A mismatch is a format error. | goldenData (all consistent) |
| F8 | An empty field (e.g. `,1` or `A,`) is a format error. | assumption |

### 2.3 Format error handling

A format error is any input the parser rejects: an empty line inside a section (F5), a
count that is not a non-negative integer or does not match the section (F7), an empty
field (F8), an odd number of fields in a dependency line, or an install line with a field
count other than two. The parser stops at the first error and reports its line number and
reason.

A format error is a regular per-file result, not a program failure. The file gets an
output file with `ERROR` on the first line and the reason on the second line (section 3).
The remaining files are still processed and the exit code stays `0`. Only invalid
arguments and I/O errors make the program exit with a non-zero code.

## 3. Output format

For every `inputNNN.txt` the program writes `outputNNN.txt`: the input file name with the
`input` prefix replaced by `output`. The file is written next to the input file unless the
`--output <folder>` option names another folder. An existing output file is overwritten.

| Result | File content |
|--------|--------------|
| Installation valid | `PASS` |
| Installation invalid (a package is required in more than one version) | `FAIL` |
| Input file malformed | `ERROR`, followed by a second line `line <n>: <reason>` |

- Result keywords are upper case. Every line is terminated with `
`.
- stdout gets one summary line per file: `inputNNN.txt: PASS`, `FAIL` or `ERROR`.
  Diagnostics for I/O errors go to stderr.
- The `docs/story/goldenData/output*.txt` files have inconsistent endings (missing newline, extra
  blank lines). The test runner compares values **after trim()** on both sides.

Exit codes:

| Code | Meaning |
|------|---------|
| 0 | all input files processed; every file has an output file, including `ERROR` results |
| 1 | invalid arguments or I/O error (folder missing, file unreadable, output folder not writable) |

## 4. Core design

### 4.1 Domain model

| Type                  | Kind            | Purpose                                                                                  |
|-----------------------|-----------------|------------------------------------------------------------------------------------------|
| `PackageVersion`      | `record struct` | `(Name, Version)` pair. Value equality is what the whole algorithm relies on.            |
| `Dependency`          | `record`        | `Dependent` → `Requirement`, both `PackageVersion`.                                      |
| `InstallationRequest` | `record`        | Parsed input: list of packages to install + list of dependencies.                        |
| `ValidationResult`    | `enum`          | `Pass` / `Fail`. Mapped to the `PASS` / `FAIL` strings only at the output boundary.      |

### 4.2 Components

```
 text ──► InputParser ──► InstallationRequest ──► DependencyValidator ──► ValidationResult
```

- **`InputParser`** – turns the raw text of one input file into an `InstallationRequest`.
  Pure function, throws a `FormatException` with a line number on malformed input.
- **`DependencyValidator`** – takes an `InstallationRequest` and decides `Pass` / `Fail`.
  Pure function, no exceptions for valid-but-failing inputs.
- **`DependencyResolver`** (facade) – `Resolve(string input) => ValidationResult`. The one
  entry point the console app calls; composes parser and validator.

### 4.3 Validation algorithm

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

### 4.4 Input format decisions

The specification in `docs/story/ResolveDependencies.md` is minimal; the golden files in `docs/story/goldenData/` show the
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
| Line endings                                             | n/a                                              | `
` and `
` both accepted.                                                                                                  |

Anything else (non-numeric count, fewer lines than declared, odd number of fields) is a
`FormatException` from the parser, which the console app reports as an error for that file
and continues with the next one.

## 5. Components

### 5.1 Core

- Pure functions, no I/O, no knowledge of files or commas.
- Input: `InstallRequest` (section 2.1). Assumes the data is structurally valid. Does not
  validate the format. May check invariants (non-empty strings) and throw a programming
  error if they are broken.
- Output: `Result` = `Valid` or `Invalid(conflicts)`, where `conflicts` is a list of
  packages with the set of required versions. Conflicts are useful for diagnostics and
  tests; the console app prints only `PASS`/`FAIL`.
- Unit-tested in isolation.

### 5.2 Console app

- Responsible for: arguments, reading a file or stdin, sanitize + parse, calling the core,
  writing the result, exit codes.
- **Sanitizer/Parser** implements rules F1-F8. It is the only place that knows the text
  format. It receives raw text and returns an `InstallRequest` or a parse error with a
  line number.
- Invocation:
  - `app <input-file>` reads the file,
  - `app` without arguments reads stdin.
  - Both variants are supported because the runner's invocation method is unknown.

### 5.3 Data model

```
PackageRef   { Package: string, Version: string }   -- value type, exact comparison
InstallRequest {
  Packages:     PackageRef[]
  Dependencies: Map<PackageRef, PackageRef[]>
}
Result = Valid | Invalid(Conflict[])
Conflict { Package: string, Versions: Set<string> }
```

## 6. Algorithm (core)

Graph traversal of the dependency graph starting from the install list, with a
per-package version check.

```
required  : Map<package, version>   -- package -> the only allowed version
visited   : Set<PackageRef>
queue     : Queue<PackageRef>  <- all Packages from InstallRequest
conflicts : Map<package, Set<version>>

while queue not empty:
  ref = queue.pop()
  if ref in visited: continue
  visited.add(ref)

  if required[ref.Package] exists and != ref.Version:
      conflicts[ref.Package] += {required[ref.Package], ref.Version}
      continue                      -- do not descend, the result is FAIL anyway
  required[ref.Package] = ref.Version

  for dep in Dependencies[ref] (or empty):
      queue.push(dep)

return conflicts.empty ? Valid : Invalid(conflicts)
```

Properties:

- Complexity O(V + E), where V is the number of unique `(package, version)` pairs
  reachable from the install list and E the number of edges between them.
- `visited` guarantees termination on cycles (S2).
- Self-dependency (S3) is handled naturally by `visited`.
- Rules for unreachable versions (S4) are never visited.


