# Feature Specification: Improve Test List UI/UX

**Feature Branch**: `014-improve-test-list`
**Created**: 2026-02-01
**Status**: Draft
**Input**: User description: "Improve test list UI with type-specific icons and short descriptions for better visual differentiation and information density"

## Problem Statement

The current test list view displays all tests with a generic beaker icon regardless of test type, and does not show the test description even though the data is available. This makes it difficult for users to:
- Quickly identify test types at a glance
- Understand what each test does without clicking into it
- Differentiate between network tests, file system tests, and other categories

**Current UI Issues (from screenshot analysis):**
1. All tests show identical beaker icon - no visual differentiation
2. Only shows DisplayName and Type text (e.g., "Ping", "HttpGet")
3. Description field exists in data but is not displayed
4. Users must click each test to understand what it does

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Visual Test Type Identification (Priority: P1)

As a user viewing the test list, I want to see distinct icons for each test type category, so that I can quickly identify the nature of each test at a glance.

**Why this priority**: Visual differentiation is the primary UX improvement - enables instant recognition without reading text.

**Independent Test**: Load test list with multiple test types, verify each type shows its distinctive icon.

**Acceptance Scenarios**:

1. **Given** I am viewing the Tests list, **When** I see a Ping test, **Then** I see a network/signal icon that visually represents connectivity testing.
2. **Given** I am viewing the Tests list, **When** I see an HttpGet test, **Then** I see a globe/web icon that visually represents HTTP/web testing.
3. **Given** I am viewing the Tests list, **When** I see a FileExists test, **Then** I see a document/file icon that visually represents file system testing.
4. **Given** I am viewing the Tests list, **When** I see a DirectoryExists test, **Then** I see a folder icon that visually represents directory/folder testing.
5. **Given** I am viewing the Tests list, **When** I see a ProcessList test, **Then** I see an app/process icon that visually represents process testing.
6. **Given** I am viewing the Tests list, **When** I see a RegistryRead test, **Then** I see a settings/registry icon that visually represents registry testing.
7. **Given** I am viewing the Tests list, **When** I see a DnsLookup test, **Then** I see a DNS/lookup icon that visually represents DNS testing.
8. **Given** I am viewing the Tests list, **When** I see an unknown/new test type, **Then** I see the default beaker icon as a fallback.

---

### User Story 2 - Display Test Descriptions (Priority: P1)

As a user viewing the test list, I want to see a brief description of each test, so that I can understand what the test does without clicking into it.

**Why this priority**: Information density improvement - reduces clicks needed to understand test purpose.

**Independent Test**: Load test list with tests that have descriptions, verify descriptions are displayed.

**Acceptance Scenarios**:

1. **Given** a test has a description defined, **When** I view the test in the list, **Then** I see the description text displayed below the test name.
2. **Given** a test has a long description, **When** I view the test in the list, **Then** the description is truncated with ellipsis to maintain layout consistency.
3. **Given** a test has no description (null or empty), **When** I view the test in the list, **Then** the description area is not shown (no empty space or placeholder).
4. **Given** multiple tests are in the list, **When** some have descriptions and some don't, **Then** the layout remains visually consistent and aligned.

---

### User Story 3 - Icon Color Coding by Category (Priority: P2)

As a user viewing the test list, I want icons to have subtle color coding by test category, so that I can group similar tests visually.

**Why this priority**: Enhancement to P1 - adds another layer of visual organization but requires icon system first.

**Independent Test**: Load test list, verify icons have category-appropriate colors.

**Acceptance Scenarios**:

1. **Given** I am viewing tests, **When** I see network-related tests (Ping, HttpGet, DnsLookup), **Then** their icons use a consistent network category color.
2. **Given** I am viewing tests, **When** I see file system tests (FileExists, DirectoryExists), **Then** their icons use a consistent file system category color.
3. **Given** I am viewing tests, **When** I see system tests (ProcessList, RegistryRead), **Then** their icons use a consistent system category color.
4. **Given** I am viewing an unknown test type, **When** it falls back to default icon, **Then** it uses the existing neutral tertiary color.

---

### Edge Cases

- What happens when a test type is not in the icon mapping? → Falls back to default beaker icon with neutral color
- What happens with extremely long descriptions? → Truncate to 2 lines maximum with ellipsis
- What happens with empty/null descriptions? → Hide description area entirely (no empty space)
- What happens with new test types added in future? → Default icon until mapping is added
- What happens with dark/light theme switching? → Icons and colors must work in both themes

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display type-specific icons for each known test type (Ping, HttpGet, FileExists, DirectoryExists, ProcessList, RegistryRead, DnsLookup)
- **FR-002**: System MUST display test description below the test name when description is available
- **FR-003**: System MUST truncate long descriptions to prevent layout overflow (maximum 2 lines)
- **FR-004**: System MUST hide description area when test has no description (null or empty)
- **FR-005**: System MUST display default beaker icon for unknown test types
- **FR-006**: System SHOULD apply category-based color coding to icons (network=blue, filesystem=orange, system=purple)
- **FR-007**: System MUST maintain existing functionality (click to open test config, hover effects, keyboard navigation)
- **FR-008**: Icons MUST be visually clear at the current size (44x44 icon container)

### Key Entities

- **TestDefinition**: Existing entity - contains Type (string) and Description (string?) fields used for icon mapping and display
- **TestTypeIconMapping**: New concept - maps test Type string to icon symbol and category color

### Assumptions

- WPF-UI Fluent icon library has suitable icons for all test type categories
- Existing premium design system color tokens can be used for category colors
- Description field is already populated in test profile JSON files

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can identify test type by icon alone in under 1 second (icon recognition)
- **SC-002**: 100% of known test types display their designated icon (no generic icons for mapped types)
- **SC-003**: Test descriptions are visible without requiring any clicks (0-click information access)
- **SC-004**: Layout remains consistent whether tests have descriptions or not (no visual jumping)
- **SC-005**: All icons and colors work correctly in both light and dark themes
- **SC-006**: Existing test list functionality (navigation, selection, keyboard support) remains fully functional
