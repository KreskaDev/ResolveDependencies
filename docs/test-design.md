---
title: Test Design
description: Naming conventions for test classes and test methods.
related:
  - docs/architecture.md
  - docs/tech-design.md
---
# Test Design

This document describes the conventions used in the test project. Which cases are tested
is described in [`architecture.md`](architecture.md) and [`tech-design.md`](tech-design.md).

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
