---
title: Algorithm Spec
description: What makes an installation valid, the semantic rules derived from the task and the golden data, and the validation algorithm with its properties.
related:
  - docs/story/ResolveDependencies.md
  - docs/story/goldenData/
---
# Algorithm Spec

## Problem

An installation request names the packages to install, each in one version, and a set of
dependency rules. A rule states that one package version requires another package version.
The installation is valid when, after following the rules transitively from the install
list, no package is required in more than one version.

## Input model

The algorithm works on an already parsed structure. How that structure is read from text
is outside this document.

```
InstallationRequest
  packages:     list of (package, version)                      -- to be installed
  dependencies: list of (package, version) -> (package, version) -- dependent -> requirement
```

Package and version are non-empty strings. Several rules for the same dependent
`(package, version)` are allowed and are read together as that version's requirement list.

## Semantic rules

The task statement is minimal; the golden data in `docs/story/goldenData/` fixes the edge
cases. The source column names the golden file number or marks an assumption.

| #   | Rule | Source |
|-----|------|--------|
| S1  | Dependencies are **transitive**. If `A,2` is installed and `A,2 -> C,3 -> D,4 -> B,1`, then `B,1` is required. | 005, 009 |
| S2  | **Cycles** in the dependency graph are allowed and are not an error. | 006 |
| S3  | A package depending on **itself** in the same version is valid. | 003 |
| S4  | Rules for **versions that are not installed** (neither directly nor transitively) are ignored. | 004, task statement example 1 |
| S5  | One package version may have **several rules**. If they point to different versions of the same package, the installation is invalid. | 001 |
| S6  | A conflict may arise between two requirements, not only between a requirement and the install list. | 007 |
| S7  | A package that appears only as a requirement (not on the install list) is installed and does not break the result unless it introduces a version conflict. | 004 |
| S8  | Package and version are **arbitrary strings**, compared **exactly** (case-sensitive, no normalisation). Versions are not interpreted; `1` and `1.0` are different versions. | 000 (`Beta-1`), assumption |
| S9  | No rule for a given version means that version has no requirements. | 004 (`C,3`) |
| S10 | Duplicates on the install list in the same version are allowed. The same package in two versions on the install list is invalid. | assumption |
| S11 | An empty install list is valid. | assumption |

## Algorithm

Graph traversal of the dependency graph starting from the install list, with a
per-package version check.

1. Build a lookup `(package, version) -> list of required (package, version)` from the
   rules (adjacency list of a directed graph).
2. Start a traversal (BFS or DFS) from every entry of the install list.
3. Maintain `required: package -> version`, the only version allowed for each package that
   will end up installed, and `visited`, the set of `(package, version)` pairs already
   processed.
4. When visiting a `(package, version)`:
   - if it is in `visited`, skip it (this is what makes cycles and self-dependencies
     terminate);
   - if `required` holds a **different** version for that package, record a conflict for
     the package with both versions and do not descend into its requirements;
   - otherwise record the version in `required` and enqueue its requirements.
5. The traversal always runs to completion so that every conflicting package is reported.
   The result is valid when no conflict was recorded.

```
required  : Map<package, version>
visited   : Set<(package, version)>
queue     : Queue<(package, version)>  <- all entries of the install list
conflicts : Map<package, Set<version>>

while queue not empty:
  current = queue.pop()
  if current in visited: continue
  visited.add(current)

  if required[current.package] exists and != current.version:
      conflicts[current.package] += {required[current.package], current.version}
      continue                      -- do not descend, the result is invalid anyway
  required[current.package] = current.version

  for requirement in lookup[current] (or empty):
      queue.push(requirement)

return conflicts                    -- valid when empty
```

## Properties

- Complexity O(V + E), where V is the number of distinct `(package, version)` pairs
  reachable from the install list and E the number of rules between them.
- `visited` guarantees termination on cycles (S2) and self-dependencies (S3).
- Rules for unreachable versions (S4) are never visited; `input004` installs `A,2`, so the
  rules of `A,1` play no part.
- The result lists every conflicting package with the set of versions required of it,
  in the order the versions were met.
