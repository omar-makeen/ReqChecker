# Tasks: Project README Documentation

**Input**: Design documents from `/specs/047-project-readme/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: No tests required — this is a documentation-only feature.

**Organization**: Tasks are grouped by user story. Since all content targets a single file (README.md), research tasks are parallelizable but write tasks are sequential.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Research (Read Source Files)

**Purpose**: Extract all parameter data, examples, and schema information from source code before writing documentation.

- [x] T001 [P] Read all 6 Network test implementations to extract parameters and behavior: src/ReqChecker.Infrastructure/Tests/PingTest.cs, HttpGetTest.cs, HttpPostTest.cs, DnsResolveTest.cs, TcpPortOpenTest.cs, UdpPortOpenTest.cs
- [x] T002 [P] Read all 5 File System test implementations to extract parameters and behavior: src/ReqChecker.Infrastructure/Tests/FileExistsTest.cs, DirectoryExistsTest.cs, FileReadTest.cs, FileWriteTest.cs, DiskSpaceTest.cs
- [x] T003 [P] Read all 6 System test implementations to extract parameters and behavior: src/ReqChecker.Infrastructure/Tests/ProcessListTest.cs, RegistryReadTest.cs, WindowsServiceTest.cs, OsVersionTest.cs, InstalledSoftwareTest.cs, EnvironmentVariableTest.cs
- [x] T004 [P] Read Security, FTP, and Hardware test implementations to extract parameters and behavior: src/ReqChecker.Infrastructure/Tests/MtlsConnectTest.cs, CertificateExpiryTest.cs, FtpReadTest.cs, FtpWriteTest.cs, SystemRamTest.cs, CpuCoresTest.cs
- [x] T005 [P] Read default profile JSON for realistic test configuration examples: src/ReqChecker.App/Profiles/default-profile.json
- [x] T006 [P] Read core models for profile schema documentation: src/ReqChecker.Core/Models/Profile.cs, TestDefinition.cs, RunSettings.cs, RunReport.cs and src/ReqChecker.Core/Enums/FieldPolicyType.cs, AdminBehavior.cs, BackoffStrategy.cs
- [x] T007 [P] Read application views and main window to document navigation pages: src/ReqChecker.App/Views/ directory listing and MainWindow.xaml

**Checkpoint**: All source data extracted — writing can begin.

---

## Phase 2: User Story 1 - Project Overview (Priority: P1) MVP

**Goal**: A new user understands what ReqChecker is, its purpose, technology stack, and key features within 60 seconds of reading the introduction.

**Independent Test**: Give the README to someone unfamiliar with the project — they should be able to describe what ReqChecker does after reading the first two sections.

### Implementation for User Story 1

- [x] T008 [US1] Write README.md with project title, tagline, overview paragraph, key features bullet list, and technology stack section in README.md
- [x] T009 [US1] Write Application Pages section documenting Profiles, Tests, Results, History, Diagnostics, and Settings navigation in README.md

**Checkpoint**: README.md exists with project overview — independently valuable as a basic project description.

---

## Phase 3: User Story 2 - Test Type Reference (Priority: P1)

**Goal**: Every test type is documented with description, parameter table, and JSON example. Users can look up any test and understand how to configure it.

**Independent Test**: Verify every test type in TestManifest.props has a corresponding section in the README with description, parameters, and JSON example.

### Implementation for User Story 2

- [x] T010 [US2] Write Test Types summary table listing all 23 types with category and one-line description in README.md
- [x] T011 [US2] Write Network Tests reference section (Ping, HttpGet, HttpPost, DnsResolve, TcpPortOpen, UdpPortOpen) — each with description, parameter table, and JSON example in README.md
- [x] T012 [US2] Write File System Tests reference section (FileExists, DirectoryExists, FileRead, FileWrite, DiskSpace) — each with description, parameter table, and JSON example in README.md
- [x] T013 [US2] Write System Tests reference section (ProcessList, RegistryRead, WindowsService, OsVersion, InstalledSoftware, EnvironmentVariable) — each with description, parameter table, and JSON example in README.md
- [x] T014 [US2] Write Security & Certificate Tests reference section (MtlsConnect, CertificateExpiry) — each with description, parameter table, and JSON example in README.md
- [x] T015 [US2] Write FTP Tests reference section (FtpRead, FtpWrite) and Hardware Tests reference section (SystemRam, CpuCores) — each with description, parameter table, and JSON example in README.md

**Checkpoint**: All 23 test types documented — users can look up any test and configure it.

---

## Phase 4: User Story 3 - Profile Configuration (Priority: P2)

**Goal**: A user can create a custom test profile JSON from scratch by following the documentation.

**Independent Test**: Follow the profile documentation to create a minimal profile JSON and load it in the application.

### Implementation for User Story 3

- [x] T016 [US3] Write Profile Configuration section documenting JSON schema (root object, runSettings, test definition structure, fieldPolicy enum, and dependsOn with example) in README.md
- [x] T017 [US3] Write a minimal working profile JSON example showing a complete profile with 2-3 tests in README.md

**Checkpoint**: Profile schema fully documented — users can create custom profiles.

---

## Phase 5: User Story 4 - Build & Run Instructions (Priority: P2)

**Goal**: A developer can clone, build, and run the application by following the README instructions.

**Independent Test**: Follow the build instructions on a clean Windows machine with .NET 8 SDK.

### Implementation for User Story 4

- [x] T018 [US4] Write Getting Started section with prerequisites (.NET 8 SDK, Windows), build commands (dotnet build/run), and conditional build system (IncludeTests parameter) in README.md
- [x] T019 [US4] Write Project Structure section showing src/ directory layout (ReqChecker.App, ReqChecker.Infrastructure, ReqChecker.Core) in README.md

**Checkpoint**: Build instructions complete — developers can build from source.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final formatting, table of contents, and validation.

- [x] T020 Add table of contents with anchor links to all major sections at the top of README.md
- [x] T021 Add License section at the bottom of README.md
- [x] T022 Final review: verify all 23 test types documented, JSON examples valid, no broken anchor links in README.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Research (Phase 1)**: No dependencies — all tasks can run in parallel
- **US1 (Phase 2)**: Depends on T007 (app views) — creates the initial README.md file
- **US2 (Phase 3)**: Depends on T001-T005 (test source data) and T008 (README.md exists) — appends test reference sections
- **US3 (Phase 4)**: Depends on T006 (core models) and T008 (README.md exists) — appends profile docs
- **US4 (Phase 5)**: Depends on T008 (README.md exists) — appends build instructions
- **Polish (Phase 6)**: Depends on all user stories complete — adds TOC and final review

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Research — creates README.md (MVP)
- **User Story 2 (P1)**: Can start after US1 (needs the file to exist) — independent content
- **User Story 3 (P2)**: Can start after US1 — independent content
- **User Story 4 (P2)**: Can start after US1 — independent content

### Parallel Opportunities

- All 7 research tasks (T001-T007) can run in parallel
- US3 and US4 (T016-T019) target different sections and could be written in parallel after US1
- Within US2, all test category tasks (T011-T015) could be drafted in parallel then assembled

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Research (read all source files)
2. Complete Phase 2: US1 (write project overview)
3. **STOP and VALIDATE**: README.md exists with clear project description

### Incremental Delivery

1. Research → Read all source files
2. US1 → Project overview, features, tech stack (MVP!)
3. US2 → All 23 test types with full reference docs
4. US3 → Profile JSON schema documentation
5. US4 → Build & run instructions
6. Polish → TOC, license, final validation

---

## Notes

- All content goes into a single file: README.md at the repository root
- Research tasks extract data from source code; write tasks transform it into documentation
- JSON examples should come from default-profile.json where possible for consistency
- Parameter tables use format: Name | Type | Required | Default | Description
- Total estimated README size: ~1200-1500 lines of Markdown
