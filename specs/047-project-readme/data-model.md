# Data Model: Project README Documentation

**Feature**: 047-project-readme
**Date**: 2026-02-21

## Overview

This feature produces a single documentation file (README.md) with no data persistence or runtime entities. The "data model" here describes the structure of the README content itself.

## README Section Entity Model

### Section: Project Overview
- **Content**: Application name, tagline, purpose, target audience
- **Source**: Derived from codebase analysis (no single source file)

### Section: Test Type Summary Table
- **Fields per row**: Type Name, Category, One-line Description
- **Count**: 23 rows
- **Source**: TestManifest.props (names), *Test.cs files (descriptions)

### Section: Test Type Reference (per test type)
- **Fields**: Heading (type name), Description (1-3 sentences), Parameter Table, JSON Example
- **Parameter Table columns**: Name, Type, Required, Default, Description
- **Count**: 23 sections
- **Source**: Individual *Test.cs files (parameter extraction from ExecuteAsync method)

### Section: Profile Configuration
- **Entities documented**:
  - Profile root object (id, name, schemaVersion, source, runSettings, tests, signature)
  - RunSettings (defaultTimeout, defaultRetryCount, retryBackoff, adminBehavior, retryDelayMs, interTestDelayMs)
  - TestDefinition (id, type, displayName, description, parameters, fieldPolicy, timeout, retryCount, requiresAdmin, dependsOn)
  - FieldPolicyType enum (Locked, Editable, Hidden, PromptAtRun)
- **Source**: Core/Models/Profile.cs, Core/Models/RunSettings.cs, Core/Models/TestDefinition.cs, Core/Enums/FieldPolicyType.cs

### Section: Build Instructions
- **Content**: Prerequisites, build commands, conditional compilation (IncludeTests parameter)
- **Source**: .csproj files, TestManifest.props

## Relationships

```
README.md
├── references → TestManifest.props (test type names)
├── references → *Test.cs (parameters, behavior)
├── references → default-profile.json (example configurations)
├── references → Core/Models/* (schema documentation)
└── references → *.csproj (build instructions)
```

No runtime data model changes. No database or storage impact.
