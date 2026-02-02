# Tasks: Export Test Results

**Input**: Design documents from `/specs/022-export-results/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md

**Tests**: Not requested - manual testing per existing project pattern

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

**NOTE**: CSV and JSON exports are already fully implemented. This feature focuses on:
1. Adding PDF export capability (US1)
2. Adding export button discoverability (US4)
3. Adding diagnostic logging to all exporters (FR-020)

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

```text
src/
├── ReqChecker.Core/           # Interfaces (no changes needed)
├── ReqChecker.Infrastructure/ # Export services
└── ReqChecker.App/            # UI and ViewModels
```

---

## Phase 1: Setup

**Purpose**: Add QuestPDF dependency for PDF generation

- [X] T001 Add QuestPDF NuGet package to `src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj`
- [X] T002 [P] Create ReqChecker logo PNG asset at `src/ReqChecker.App/Resources/Images/reqchecker-logo.png`
- [X] T003 [P] Add logo as EmbeddedResource in `src/ReqChecker.App/ReqChecker.App.csproj`

---

## Phase 2: User Story 1 - Export to PDF for Stakeholder Reporting (Priority: P1) 🎯 MVP

**Goal**: Generate professionally formatted PDF reports with branding, summary, machine info, and test results table

**Independent Test**: Run tests, click PDF export button, verify PDF contains all test results with ReqChecker branding, summary stats, and error details for failed tests

### Implementation for User Story 1

- [X] T004 [US1] Create `PdfExporter.cs` implementing `IExporter` in `src/ReqChecker.Infrastructure/Export/PdfExporter.cs`
- [X] T005 [US1] Implement PDF header section with ReqChecker logo, profile name, run date in `PdfExporter.cs`
- [X] T006 [US1] Implement PDF summary section with total/pass/fail/skip counts and pass rate in `PdfExporter.cs`
- [X] T007 [US1] Implement PDF machine info section with hostname, OS, user in `PdfExporter.cs`
- [X] T008 [US1] Implement PDF test results table with status colors, name, duration, type in `PdfExporter.cs`
- [X] T009 [US1] Implement failed test error details inline display in `PdfExporter.cs`
- [X] T010 [US1] Implement PDF footer with page numbers and generation timestamp in `PdfExporter.cs`
- [X] T011 [US1] Register `PdfExporter` in DI container in `src/ReqChecker.App/App.xaml.cs`

**Checkpoint**: PdfExporter service complete and registered - ready for UI integration

---

## Phase 3: User Story 4 - Quick Export from Results Dashboard (Priority: P1)

**Goal**: Add PDF export button alongside existing CSV/JSON buttons for easy access

**Independent Test**: Navigate to Results Dashboard with test results, verify PDF button is visible next to JSON and CSV buttons, click PDF button and verify file save dialog appears with PDF filter

### Implementation for User Story 4

- [X] T012 [US4] Inject `PdfExporter` into `ResultsViewModel` constructor in `src/ReqChecker.App/ViewModels/ResultsViewModel.cs`
- [X] T013 [US4] Add `ExportToPdfCommand` RelayCommand method in `src/ReqChecker.App/ViewModels/ResultsViewModel.cs`
- [X] T014 [US4] Add PDF export button to header section in `src/ReqChecker.App/Views/ResultsView.xaml`
- [X] T015 [US4] Verify PDF button follows existing SecondaryButton style and uses Document24 icon

**Checkpoint**: PDF export fully functional from UI - MVP complete

---

## Phase 4: User Story 2 - CSV Export Logging (Priority: P2)

**Goal**: Add diagnostic logging to CSV export operations (already implemented - just add logging per FR-020)

**Independent Test**: Export to CSV, verify logs contain format, outcome, and file size

**Status**: CSV export already implemented - only logging enhancement needed

### Implementation for User Story 2

- [X] T016 [US2] No changes needed - CSV export already functional
- [X] T017 [US2] Logging will be added in shared ExportAsync method (Phase 6)

**Checkpoint**: CSV export continues to work, logging will be added cross-cutting

---

## Phase 5: User Story 3 - JSON Export Logging (Priority: P3)

**Goal**: Add diagnostic logging to JSON export operations (already implemented - just add logging per FR-020)

**Independent Test**: Export to JSON, verify logs contain format, outcome, and file size

**Status**: JSON export already implemented - only logging enhancement needed

### Implementation for User Story 3

- [X] T018 [US3] No changes needed - JSON export already functional
- [X] T019 [US3] Logging will be added in shared ExportAsync method (Phase 6)

**Checkpoint**: JSON export continues to work, logging will be added cross-cutting

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Add diagnostic logging that benefits all export operations (FR-020)

- [X] T020 Enhance `ExportAsync` method in `ResultsViewModel.cs` with structured logging (format, outcome, file size, duration)
- [X] T021 Add Stopwatch timing to measure export duration in `ResultsViewModel.cs`
- [X] T022 Add FileInfo.Length to log exported file size in `ResultsViewModel.cs`
- [X] T023 Verify all three export buttons (PDF, CSV, JSON) are properly aligned and styled
- [ ] T024 Manual testing: Verify PDF export with 0, 1, 10, and 50+ test results
- [ ] T025 Manual testing: Verify PDF opens correctly in Adobe Reader and browser
- [ ] T026 Manual testing: Verify export logging appears in Serilog output

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **User Story 1 (Phase 2)**: Depends on Setup (T001 package, T002-T003 logo)
- **User Story 4 (Phase 3)**: Depends on User Story 1 (needs PdfExporter)
- **User Story 2 (Phase 4)**: Already complete - no blocking work
- **User Story 3 (Phase 5)**: Already complete - no blocking work
- **Polish (Phase 6)**: Depends on User Story 4 completion

### User Story Dependencies

- **User Story 1 (PDF Export)**: Core new functionality - must complete first
- **User Story 4 (Dashboard UI)**: Depends on US1 (needs PdfExporter to bind to command)
- **User Story 2 (CSV)**: Independent - already implemented
- **User Story 3 (JSON)**: Independent - already implemented

### Parallel Opportunities

**Phase 1 (Setup):**
```bash
# Can run in parallel:
T002 "Create ReqChecker logo PNG asset"
T003 "Add logo as EmbeddedResource"
```

**Phase 2 (User Story 1) - Sequential:**
Tasks T004-T010 build on each other within PdfExporter.cs - execute sequentially

---

## Implementation Strategy

### MVP First (User Story 1 + User Story 4)

1. Complete Phase 1: Setup (add QuestPDF, add logo)
2. Complete Phase 2: User Story 1 (PdfExporter service)
3. Complete Phase 3: User Story 4 (PDF button in UI)
4. **STOP and VALIDATE**: Test PDF export end-to-end
5. Feature is usable! CSV/JSON already work.

### Incremental Delivery

Since CSV and JSON already work:
1. Setup + US1 + US4 → PDF export works → **MVP Complete**
2. Add Phase 6 → Logging for all exports → **Full Feature Complete**

### Task Count Summary

| Phase | User Story | Task Count | Status |
|-------|------------|------------|--------|
| Phase 1 | Setup | 3 | New |
| Phase 2 | US1 (PDF) | 8 | New |
| Phase 3 | US4 (UI) | 4 | New |
| Phase 4 | US2 (CSV) | 2 | Already done |
| Phase 5 | US3 (JSON) | 2 | Already done |
| Phase 6 | Polish | 7 | New |
| **Total** | | **26** | 22 actual work |

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- CSV and JSON exports are ALREADY IMPLEMENTED - only PDF is new
- PdfExporter follows exact same pattern as existing JsonExporter and CsvExporter
- Use QuestPDF Community license (free for <$1M revenue)
- Logo asset should be simple PNG, approximately 150x50px
- Avoid: modifying existing working export code except for logging enhancement
