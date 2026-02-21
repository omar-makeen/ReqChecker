# Quickstart: Project README Documentation

**Feature**: 047-project-readme
**Date**: 2026-02-21

## Implementation Steps

### Step 1: Read All Test Implementations
Read each of the 23 `*Test.cs` files to extract:
- Test type name (from `[TestType("...")]` attribute)
- Parameters (from `GetRequiredParameter` / `GetOptionalParameter` calls)
- Behavior description (from class summary and execute logic)
- Pass/fail criteria

### Step 2: Read Default Profile
Read `src/ReqChecker.App/Profiles/default-profile.json` to extract realistic JSON examples for each test type.

### Step 3: Read Core Models
Read profile/test definition models to document the schema accurately:
- `src/ReqChecker.Core/Models/Profile.cs`
- `src/ReqChecker.Core/Models/TestDefinition.cs`
- `src/ReqChecker.Core/Models/RunSettings.cs`
- `src/ReqChecker.Core/Enums/FieldPolicyType.cs`

### Step 4: Write README.md
Create `README.md` at the repository root with the full structure from plan.md.

### Step 5: Verify
- Count test type sections (must be 23)
- Verify JSON examples parse correctly
- Verify table of contents links work
- Verify build commands are accurate

## Verification Checklist

- [ ] README.md exists at repository root
- [ ] Table of contents with working anchor links
- [ ] Project overview section present
- [ ] All 23 test types documented with description, parameters, and JSON example
- [ ] Test types organized into 6 categories
- [ ] Profile JSON schema documented (root, runSettings, testDefinition, fieldPolicy)
- [ ] Build instructions with prerequisites
- [ ] Conditional build (IncludeTests) documented
- [ ] Application pages documented
- [ ] Test dependencies (dependsOn) documented with example
