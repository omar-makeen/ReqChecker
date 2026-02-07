# Feature Specification: Search & Filter in Test List

**Feature Branch**: `035-test-list-search`
**Created**: 2026-02-07
**Status**: Draft
**Input**: User description: "Feature: Search & Filter in Test List. Why It Matters: Profiles with 30+ tests become hard to navigate. A search box filtering by test name/type is a quick win."

## Clarifications

### Session 2026-02-07

- Q: What visual style should the search box use? → A: Use existing `InputField` style (40px height, 6px radius, accent focus border) with Search16 icon as placeholder decoration.
- Q: What placeholder text should the search box display? → A: "Search by name, type, or description..." to communicate full search scope.
- Q: How should the clear button (X) be implemented? → A: Inline X button inside the TextBox, right-aligned, appears only when text is present, ghost/transparent style.
- Q: Should the search box have an entrance animation? → A: No entrance animation — toolbar row (search + Select All) is static, consistent with current toolbar behavior.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Search Tests by Name (Priority: P1)

A user has loaded a profile with many tests (30+). They want to quickly find a specific test without scrolling through the entire list. They type a few characters into a search box and the test list instantly filters to show only tests whose display name contains the search text.

**Why this priority**: This is the most common search need — users know the name (or part of it) of the test they're looking for. It provides immediate value for large profiles.

**Independent Test**: Can be fully tested by loading a profile with 8+ tests, typing a partial test name in the search box, and verifying only matching tests are shown. Delivers immediate value for navigating large profiles.

**Acceptance Scenarios**:

1. **Given** a loaded profile with tests displayed, **When** the user types "DNS" into the search box, **Then** only tests whose display name contains "DNS" (case-insensitive) are shown.
2. **Given** a filtered test list, **When** the user clears the search box, **Then** all tests are shown again.
3. **Given** a loaded profile, **When** the user types a search term that matches no tests, **Then** an empty state message is shown (e.g., "No tests match your search").
4. **Given** a filtered test list showing 2 of 8 tests, **When** the user looks at the test count indicator, **Then** it reflects the visible count (e.g., "2 of 8 tests").

---

### User Story 2 - Search Tests by Type (Priority: P2)

In addition to searching by name, the user wants to find all tests of a specific type (e.g., all "Ping" tests, all "HttpGet" tests). The search box should also match against the test type label.

**Why this priority**: Complements name search by allowing users to quickly locate all tests of a specific category. Useful when debugging a class of failures (e.g., "all network tests").

**Independent Test**: Can be tested by typing a test type (e.g., "Ping") into the search box and verifying all tests of that type appear, regardless of their display name.

**Acceptance Scenarios**:

1. **Given** a loaded profile with mixed test types, **When** the user types "Ping" into the search box, **Then** all tests with type "Ping" are shown, even if "Ping" is not in their display name.
2. **Given** a loaded profile, **When** the user types "Http", **Then** tests with type "HttpGet" or "HttpPost" are shown (partial match on type).

---

### User Story 3 - Search Tests by Description (Priority: P3)

The user wants to search by keywords in the test description, not just the name or type. This is useful when the display name is generic but the description contains specific details (e.g., searching "google" to find a DNS test that resolves google.com).

**Why this priority**: Lower priority because descriptions are optional and less commonly used for searching. Still valuable for power users with detailed test descriptions.

**Independent Test**: Can be tested by typing a keyword that appears only in a test's description (not its name or type) and verifying the test appears in the filtered results.

**Acceptance Scenarios**:

1. **Given** a test with display name "DNS Resolution Check" and description "Verifies DNS can resolve google.com", **When** the user types "google", **Then** that test is shown in the filtered results.

---

### Edge Cases

- What happens when the search box is empty or contains only whitespace?
  - All tests are shown (no filtering applied). Leading/trailing whitespace is trimmed.
- What happens to the "Select All" checkbox and selection state when a filter is active?
  - The "Select All" checkbox operates only on visible (filtered) tests. Previously selected tests that are hidden by the filter retain their selection state.
- What happens when the user runs tests while a search filter is active?
  - The run command uses the selection state, not the filter. Tests that are selected but currently hidden by the filter are still included in the run.
- What happens when a new profile is loaded?
  - The search box is cleared and all tests from the new profile are displayed.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The Test List page MUST display a search input field in the toolbar area, between the header and the test list. The search box MUST use the existing `InputField` style (40px height, 6px corner radius, accent focus border) with a Search16 icon as leading decoration.
- **FR-002**: The search field MUST filter the test list in real-time as the user types (no submit button required).
- **FR-003**: The search MUST match against test display name (case-insensitive, partial match).
- **FR-004**: The search MUST match against test type (case-insensitive, partial match).
- **FR-005**: The search MUST match against test description (case-insensitive, partial match).
- **FR-006**: When the search term matches no tests, the list MUST display an empty state message indicating no results were found.
- **FR-007**: The test count indicator MUST update to reflect the number of visible (filtered) tests versus the total count.
- **FR-008**: Clearing the search box MUST restore the full unfiltered test list.
- **FR-009**: The search box MUST include an inline clear button (X), right-aligned inside the TextBox, that appears only when text is present. The clear button uses a ghost/transparent style.
- **FR-010**: The "Select All" checkbox MUST operate on the visible (filtered) tests only.
- **FR-011**: Test selection state MUST be preserved when filtering — selecting/deselecting a test while filtered does not affect hidden tests.
- **FR-012**: The search box MUST be cleared when a new profile is loaded.
- **FR-013**: The search box MUST be visually consistent with the existing premium design system — using the `InputField` style, no entrance animation (static toolbar row), and placeholder text "Search by name, type, or description..." to communicate search scope.

## Assumptions

- Filtering is performed entirely in-memory on the already-loaded test collection. No external search service or indexing is needed.
- The search matches if the search term is found anywhere in the display name, type, or description (substring match, not word-boundary or fuzzy match).
- The search box uses the `InputField` style with a Search16 icon, placeholder text "Search by name, type, or description...", and an inline clear (X) button that appears only when text is present.
- Performance is not a concern — profiles are expected to have at most a few hundred tests, so in-memory filtering is instantaneous.
- The search does not affect the run behavior — the "Run" action uses the selection state regardless of the current filter.
- The toolbar row (search box + Select All checkbox) has no entrance animation — it is static, consistent with current toolbar behavior.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can locate a specific test in a 30+ test profile within 3 seconds using the search box.
- **SC-002**: Filtering results appear instantly as the user types (no perceptible delay).
- **SC-003**: 100% of tests matching the search term by name, type, or description are shown in results — no false negatives.
- **SC-004**: The test count indicator accurately reflects the filtered count at all times.
