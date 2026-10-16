# Resolve dependencies

Checks whether a set of software packages can be installed without requiring two different
versions of the same package. The problem statement is in [`abstract.md`](abstract.md);
the design is described in [`docs/architecture.md`](docs/architecture.md).

## Requirements

- .NET SDK 10.0 or newer

## Build

```
dotnet build
```

## Run

The console application takes one argument: the path to a folder containing input files.
Every file named `input*.txt` in that folder is processed in name order and one line per file
is printed to standard output.

```
dotnet run --project src/ResolveDependencies.Console -- <path-to-folder>
```

Example, using the test data shipped with the repository:

```
dotnet run --project src/ResolveDependencies.Console -- testdata
```

Output format:

```
input000.txt: PASS
input001.txt: FAIL
...
```

The application only prints results; it never writes or modifies files in the given folder.
A file that cannot be parsed is reported as an error on standard error, the remaining files
are still processed, and the exit code is non-zero.

## Test

```
dotnet test
```

The test suite contains unit tests for the parser and validator and golden tests that run
every `testdata/inputNNN.txt` and compare the result with `testdata/outputNNN.txt`. To add a
case, add a new `inputNNN.txt` / `outputNNN.txt` pair to `testdata/`.

## Input file format

```
N              number of packages to install
p,v            N lines: package p in version v
M              number of dependencies (section may be omitted)
p1,v1,p2,v2    M lines: p1,v1 depends on p2,v2 (a line may list several p,v requirements)
```

## Project layout

```
src/ResolveDependencies.Core      library with the parsing and validation logic
src/ResolveDependencies.Console   command-line entry point
src/ResolveDependencies.Tests     xUnit tests
testdata/                         golden input/output pairs
docs/                             architecture notes
```
