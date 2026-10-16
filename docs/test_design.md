---
title: Test Design
description: Naming conventions for tests and the cases covered by parser unit tests, validator unit tests and golden tests.
related:
  - docs/tech_design.md
  - docs/algorithm_spec.md
---
# Test Design

Tests live in `ResolveDependencies.Tests` and target Core only. Rule identifiers `F1`-`F8`
refer to the input format rules in `docs/tech_design.md`, `S1`-`S11` to the semantic rules in
`docs/algorithm_spec.md`.

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
DependencyValidatorTests.Validate_CyclicDependencies_ReturnsValid
DependencyValidatorTests.Validate_TransitiveConflict_ReturnsConflict
```

## Parser unit tests

One case per rule F2-F8, each feeding text to `InputParser` in code. Negative cases assert
that a `FormatException` is thrown and that it carries the correct line number.

## Validator unit tests

One case per rule S1-S11 plus the two examples from `docs/story/ResolveDependencies.md`.
They do not read files; they build `InstallationRequest` in code. Each case asserts the
verdict and, for an invalid installation, the conflicting packages with their versions.

## Golden tests

One input file is one test case. A `[Theory]` driven by `MemberData` enumerates
`docs/story/goldenData/input*.txt`, runs `DependencyResolver.Resolve` on the file text,
renders the result in the output file format (`PASS`, `FAIL`, or `ERROR` with the reason
on the second line) and compares it with the matching `output*.txt`. The golden output
files have inconsistent endings (missing newline, extra blank lines), so both sides are
compared after `trim()`.

The `docs/story/goldenData/` folder is linked into the test project and copied to the output
directory, so the repository remains the single source of truth. Adding a case is adding an
`inputNNN.txt` / `outputNNN.txt` pair.

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
| 007 | FAIL | S6 conflict between two requirements |
| 008 | PASS | F3 missing dependency section |
| 009 | PASS | S1 long chain without conflict |

Missing cases worth adding as own file pairs: N = 0, duplicate on the install list, two
versions of the same package on the install list, and `ERROR` cases: blank line inside a
section, mismatched N/M, odd number of fields.
