# Research: Conditional Test Builds

## Decision 1: MSBuild Conditional Compilation Approach

**Decision**: Use an MSBuild `.props` file as the test manifest, with `<Compile Remove>` / `<Compile Include>` controlled by an `IncludeTests` property.

**Rationale**: MSBuild's item group conditions are the standard .NET mechanism for conditional compilation. A `.props` file imported by the `.csproj` keeps the logic modular and avoids polluting the main project file. The existing reflection-based DI registration (`Assembly.GetTypes().Where(t => typeof(ITest).IsAssignableFrom(t))`) already works as a runtime discovery mechanism — if a test class isn't compiled in, reflection won't find it.

**Alternatives considered**:
- `#if` preprocessor directives per file — rejected: requires modifying every test file, messy
- Separate NuGet packages per test — rejected: overkill for 22-150 test files, massive maintenance burden
- Category-based module assemblies — rejected: less granular than needed, grey areas in categorization

## Decision 2: Test Manifest Format

**Decision**: Use an MSBuild `.props` file (`TestManifest.props`) in the Infrastructure project root. Each test type is an `<ItemGroup>` with a condition on `$(IncludeTests)`.

**Rationale**: Keeps the manifest in the same language as the build system (MSBuild XML). No custom tooling needed. The `.props` file is imported by the `.csproj` and participates in standard build evaluation. Build validation (FR-004, FR-014) can be implemented as MSBuild targets within the same file.

**Alternatives considered**:
- JSON manifest + custom MSBuild task — rejected: adds complexity for no benefit
- YAML manifest — rejected: needs parsing tooling, not native to MSBuild
- Convention-based (folder per test type) — rejected: disruptive restructuring of existing code

## Decision 3: Default Profile Filtering

**Decision**: Use an MSBuild target that runs after build to filter `default-profile.json` based on `IncludeTests`. The target invokes a small inline C# task or PowerShell script to parse the JSON, remove entries whose `type` doesn't match any included test, and write the filtered version.

**Rationale**: The profile is an embedded resource compiled into the App assembly. It must be filtered before the resource embedding step. An MSBuild target with `BeforeTargets="EmbeddedResource"` can modify the file in a temp location before it's embedded.

**Alternatives considered**:
- Runtime filtering (app ignores unknown types) — already required by FR-006, but doesn't solve the UX issue of SC-006 (no orphaned entries)
- Manual profile per customer — rejected: doesn't scale with 150+ test types

## Decision 4: Build Validation Strategy

**Decision**: Two MSBuild targets:
1. **ValidateIncludeTests** — checks each name in `IncludeTests` against known identifiers in the manifest. Fails with `<Error>` if unknown.
2. **ValidateManifestSync** — globs `Tests/*.cs` (excluding shared files like `TestTypeAttribute.cs`), checks each file has a manifest entry. Fails with `<Error>` if unregistered.

**Rationale**: Both validations run early in the build pipeline (before compilation), providing fast feedback. MSBuild `<Error>` tasks produce clear, actionable messages.

## Decision 5: GitHub Actions Workflow

**Decision**: Create a `workflow_dispatch` workflow with inputs for `tests` (semicolon-separated list or "all"), `customer-name` (optional string), and `version` (semver string). The workflow runs `dotnet publish` with the appropriate `/p:IncludeTests` parameter and uploads the named artifact.

**Rationale**: `workflow_dispatch` allows manual triggering from the GitHub UI with input forms — ideal for on-demand customer builds. No existing workflows exist, so there's no conflict.

## Decision 6: Shared Infrastructure Code Handling

**Decision**: The manifest's `<Compile Remove>` pattern only targets files matching `Tests/*Test.cs`. All other files in the Infrastructure project (base classes, utilities, interfaces, attributes) are always included.

**Rationale**: The naming convention `*Test.cs` cleanly separates test implementations from shared code. `TestTypeAttribute.cs` and any future base classes don't match the pattern and are always compiled.
