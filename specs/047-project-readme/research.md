# Research: Project README Documentation

**Feature**: 047-project-readme
**Date**: 2026-02-21

## Decision: Test Type Count

- **Decision**: 23 test types (not 24)
- **Rationale**: TestManifest.props lists exactly 23 KnownTestType entries. Initial exploration double-counted CertificateExpiry.
- **Alternatives considered**: None — this is a factual count from the source of truth.

## Decision: Test Type Categories

- **Decision**: Organize into 6 categories: Network (6), File System (5), System (6), Security & Certificates (2), FTP (2), Hardware (2)
- **Rationale**: Groups tests by functional domain for logical navigation. FTP is separated from Network because it uses a distinct protocol library (FluentFTP) and has different parameter patterns (credentials, remote paths).
- **Alternatives considered**: Merging FTP into Network (rejected — would create an oversized category and mix authentication patterns).

## Decision: README Scope

- **Decision**: Single README.md file, no external documentation pages
- **Rationale**: The user requested "a decent readme." Keeping everything in one file maximizes discoverability on GitHub where README.md is auto-rendered. The 23 test types with examples will be ~1200 lines, which is manageable with a table of contents.
- **Alternatives considered**: Wiki pages or /docs folder (rejected — adds navigation friction for a project of this size).

## Decision: Parameter Documentation Format

- **Decision**: Each test type gets a markdown table of parameters (Name | Type | Required | Default | Description) plus one JSON example
- **Rationale**: Tables are scannable and GitHub renders them well. The JSON example shows realistic usage. This matches common patterns in popular open-source projects.
- **Alternatives considered**: Inline lists (less scannable), full JSON schema (too verbose for README).

## Decision: Profile Schema Documentation

- **Decision**: Document the complete profile JSON schema with annotated examples for root object, runSettings, test definition, and fieldPolicy
- **Rationale**: Users need to create custom profiles. The schema is the contract between users and the application.
- **Alternatives considered**: Referencing the source code models (rejected — users shouldn't need to read C# to configure the app).

## Source Data Inventory

All parameter data will be extracted from:
- Test implementation files: `src/ReqChecker.Infrastructure/Tests/*Test.cs`
- Default profile: `src/ReqChecker.App/Profiles/default-profile.json`
- Core models: `src/ReqChecker.Core/Models/`
- Build manifest: `src/ReqChecker.Infrastructure/TestManifest.props`

No external research needed — all information exists in the codebase.
