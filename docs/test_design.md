---
title: Test Design
description: Naming conventions for tests, the request builder, and the cases covered by sanitizer, parser, resolver and golden tests.
related:
  - docs/tech_design.md
  - docs/algorithm_spec.md
---
# Test Design

Tests live in `ResolveDependencies.Tests` and target Core and Console. Rule identifiers
`F1`-`F8` refer to the input format rules in `docs/tech_design.md`, `S1`-`S11` to the
semantic rules in `docs/algorithm_spec.md`.

Tests exercise the components through instances, never through static members.
`InstallationRequestBuilder` builds the Core contract fluently; one call mirrors one line
of an input file (`Install` for a package line, `Requires` for a dependency line with the
dependent first and its requirements after it).

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
InputSanitizerTests.Sanitize_SurroundingWhitespace_TrimsEachLine
InputParserTests.Parse_MissingDependencySection_ReturnsNoDependencies
InputParserTests.Parse_OddNumberOfFields_ThrowsFormatException
DependencyResolverTests.Resolve_CyclicDependencies_ReturnsValid
DependencyResolverTests.Resolve_TransitiveConflict_ReturnsConflict
```

## Sanitizer unit tests

One case per rule F4-F6, each feeding text to `InputSanitizer` in code and asserting the
returned lines: `\n` endings, a lone `\r` not ending a line, trimming of every line, and
an empty line inside the file being kept.

## Parser unit tests

One case per rule F2, F3, F7 and F8 plus the install line field count, each feeding lines
to `InputParser` in code. Negative cases assert that a `FormatException` is thrown and
that it carries the correct line number.

## Resolver unit tests

`DependencyResolverRuleTests` holds one case per rule S1-S11 plus the two examples from
`docs/story/ResolveDependencies.md`, built with `InstallationRequestBuilder` and run
through `DependencyResolver`. Each case asserts the verdict and, for an invalid
installation, the conflicting packages with their versions in the order the traversal
meets them. `DependencyGraphBuilderTests` covers the graph: a version without rules, rule
order for one dependent, and rules of different versions kept apart.

## Console unit tests

- `CommandLineParserTests` covers both argument shapes, options in any order, the two
  styles mixed, and every usage error: no input folder, a third positional argument, a
  folder given twice, an option without a value, an unknown option.
- `FileReportTests` asserts the output file content and the console lines for `PASS`,
  `FAIL` and `ERROR`.
- `InputFileProcessorTests` runs raw text through the whole chain and asserts the verdict
  and, for `ERROR`, the reason.

## Golden tests

One input file is one `[Fact]` in each of two classes. The test project does not read the
files; `docs/story/goldenData/` stays the reference the tests are transcribed from.

- `InputSanitizerTests` holds the exact text of `inputNNN.txt` as a string constant,
  including its line endings and trailing blank lines, and asserts the clean lines the
  parser receives.
- `DependencyResolverTests` builds the request of `inputNNN.txt` with
  `InstallationRequestBuilder`, exactly as the parser would read it, and asserts the
  verdict found in `outputNNN.txt`: `IsValid` with no conflicts for `PASS`, not `IsValid`
  for `FAIL`.

Adding a case is adding an `inputNNN.txt` / `outputNNN.txt` pair and a test method in
each class.

Coverage of the current data:

| Test | Expected | What it verifies |
|------|----------|------------------|
| 000 | PASS | version as an arbitrary string, F4 no trailing newline in input, junk blank lines in output |
| 001 | FAIL | S5, conflicting rules of one version |
| 002 | FAIL | F2, line with three pairs |
| 003 | PASS | S3 self-dependency, F5 trailing blank line |
| 004 | PASS | S4, S7, S9 |
| 005 | FAIL | S1 chain of depth 3 |
| 006 | PASS | S2 cycle |
| 007 | FAIL | S6 conflict between two requirements, F5 trailing blank line |
| 008 | PASS | F3 missing dependency section |
| 009 | PASS | S1 long chain without conflict |

Missing cases worth adding as own file pairs: N = 0, duplicate on the install list, two
versions of the same package on the install list, and `ERROR` cases: blank line inside a
section, mismatched N/M, odd number of fields.
