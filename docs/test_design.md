---
title: Test Design
description: Naming conventions for test classes and test methods.
related:
  - docs/architecture.md
  - docs/tech_design.md
---
# Test Design

This document describes the conventions used in the test project. Which cases are tested
is described in [`architecture.md`](architecture.md) and [`tech_design.md`](tech_design.md).

## Naming

Test classes are named after the class under test with the `Tests` suffix:

```
<ClassUnderTest>Tests
```

Test methods follow the `UnitOfWork_StateUnderTest_ExpectedBehavior` pattern. The three
parts are separated by underscores and each part is written in PascalCase:

- **UnitOfWork** is the method or behaviour being exercised.
- **StateUnderTest** is the input or precondition that distinguishes this case.
- **ExpectedBehavior** is the outcome the test asserts.

Examples:

```
InputParserTests.Parse_MissingDependencySection_ReturnsNoDependencies
InputParserTests.Parse_OddNumberOfFields_ThrowsFormatException
DependencyValidatorTests.Validate_CyclicDependencies_ReturnsPass
DependencyValidatorTests.Validate_TransitiveConflict_ReturnsFail
```


## 7. Tests

### 7.1 Core unit tests

One case per finding S1-S11 plus the examples from `docs/story/ResolveDependencies.md`. They do not read
files; they build `InstallationRequest` in code.

### 7.2 Parser tests

One case per rule F2-F8, including negative cases (expected parse error with the
correct line number).

### 7.3 Acceptance tests on `docs/story/goldenData/`

The runner iterates over `inputNNN.txt` / `outputNNN.txt` pairs, runs the full pipeline
(parser + core), renders the result in the output file format (`PASS`, `FAIL`, or `ERROR`
with the reason on the second line) and compares it with the expected file after `trim()`
on both sides. Adding a new file to `docs/story/goldenData/` automatically adds a case.

Coverage of the current data:

| Test | Expected | What it verifies |
|------|----------|------------------|
| 000 | PASS | version as an arbitrary string, no trailing newline in input, junk blank lines in output |
| 001 | FAIL | S5, conflicting rules of one version |
| 002 | FAIL | F2, line with three pairs |
| 003 | PASS | S3 self-dependency, F5 trailing blank line |
| 004 | PASS | S4, S7, S9 |
| 005 | FAIL | S1 chain of depth 3 |
| 006 | PASS | S2 cycle |
| 007 | FAIL | S6 conflict between two dependencies |
| 008 | PASS | F3 missing dependency section |
| 009 | PASS | S1 long chain without conflict |

Missing cases worth adding as own test files: N = 0, duplicate on the install list, two
versions of the same package on the install list, and `ERROR` cases: blank line inside a
section, mismatched N/M, odd number of fields.

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
