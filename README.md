# Resolve dependencies

## 1. Goal

The program receives an installation description (a list of packages to install plus
dependency rules) and answers whether the installation is valid. An installation is
invalid when, after taking the transitive closure of the dependencies, any package is
required in more than one version.
Checks whether a set of software packages can be installed without requiring two different versions of the same package.


Result: `PASS` or `FAIL`.


- The problem statement is in [`Abstract.md`](abstract.md)
- The design is described in [`Architecture.md`](docs/architecture.md).

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
dotnet run --project src/ResolveDependencies.Console -- goldenData
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
every `goldenData/inputNNN.txt` and compare the result with `goldenData/outputNNN.txt`. To add a
case, add a new `inputNNN.txt` / `outputNNN.txt` pair to `goldenData/`.
