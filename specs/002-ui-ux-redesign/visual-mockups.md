# ReqChecker Visual Mockups

## Overview

This document describes the visual appearance of each view in the redesigned ReqChecker application. These serve as implementation reference for achieving the desired premium aesthetic.

---

## 1. Application Shell

### Description

The main window features a custom-styled title bar that blends seamlessly with the application design. The left sidebar contains navigation with elegant iconography. The main content area displays the currently selected view.

### Layout

```
+------------------------------------------------------------------+
|  [_] [O] [X]                ReqChecker                            | <- Custom title bar (dark, 32px height)
+------------------------------------------------------------------+
|        |                                                          |
|  [*]   |                                                          |
|  ===   |                                                          |
|        |                                                          |
|  [P]   |               MAIN CONTENT AREA                          |
|  Pro   |                                                          |
|        |             (View-specific content)                      |
|  [T]   |                                                          |
|  Test  |                                                          |
|        |                                                          |
|  [R]   |                                                          |
|  Res   |                                                          |
|        |                                                          |
|  [D]   |                                                          |
|  Diag  |                                                          |
|        |                                                          |
|        +----------------------------------------------------------+
|  ---   |  v1.0.0  |  Current Profile: Default Profile   |  [Sun]  | <- Status bar
+--------+----------------------------------------------------------+

Sidebar: 72px collapsed, 240px expanded
Title bar: #0f0f1a with subtle accent line at bottom
Active nav item: Left border accent gradient, elevated background
```

### Visual Details

- **Title bar**: Deep background with window controls (custom styled) and centered app name
- **Sidebar**: Slightly elevated from base background (#1a1a2e)
- **Navigation items**: Icon + label stacked vertically, icon only when collapsed
- **Active indicator**: 3px left border with cyan-to-violet gradient
- **Status bar**: Slim footer with version, current profile, theme toggle button

---

## 2. Profile Selector View

### Description

A welcoming view for selecting or importing configuration profiles. Profiles display as prominent cards with visual distinction between company-managed and user profiles.

### Layout

```
+------------------------------------------------------------------+
|                                                                   |
|    SELECT PROFILE                          [+ Import Profile]     |
|    Choose a configuration profile to load                         |
|                                                                   |
+------------------------------------------------------------------+
|                                                                   |
|  +---------------------------+  +---------------------------+    |
|  |  [Gradient Header Bar]    |  |  [Gradient Header Bar]    |    |
|  |                           |  |                           |    |
|  |  Company Default Profile  |  |  My Custom Profile        |    |
|  |                           |  |                           |    |
|  |  12 tests configured      |  |  5 tests configured       |    |
|  |  Schema v1.2              |  |  Schema v1.0              |    |
|  |                           |  |                           |    |
|  |  [Company-Managed]        |  |  [User Profile]           |    |
|  |                           |  |                           |    |
|  |           [Select ->]     |  |           [Select ->]     |    |
|  +---------------------------+  +---------------------------+    |
|                                                                   |
+------------------------------------------------------------------+

Cards: 300px min-width, responsive grid
Gradient header: Subtle cyan-to-violet on company profiles, muted on user
```

### Visual Details

- **Page header**: "SELECT PROFILE" in display typography, subtle description below
- **Import button**: Secondary style with upload icon
- **Profile cards**:
  - Gradient accent bar at top (4px)
  - Company profiles: Vibrant accent gradient
  - User profiles: Muted gray gradient
- **Card content**: Profile name (H3), test count, schema version
- **Badge**: Pill badge showing source type
- **Select button**: Primary gradient button with arrow icon
- **Empty state**: Illustrated graphic with "No profiles yet" message

---

## 3. Test List View

### Description

A clear overview of all tests in the selected profile. Tests display as interactive cards with type indicators and quick-run capability.

### Layout

```
+------------------------------------------------------------------+
|                                                                   |
|    TEST SUITE                                                     |
|    Default Profile - 12 tests                   [Run All Tests]  |
|                                                                   |
+------------------------------------------------------------------+
|                                                                   |
|  +--------------------------------------------------------------+|
|  |  [Ping Icon]  Ping Test - api.example.com            [Pending]||
|  |               Network connectivity check                      ||
|  +--------------------------------------------------------------+|
|                                                                   |
|  +--------------------------------------------------------------+|
|  |  [HTTP Icon]  HTTP GET - Health Endpoint             [Pending]||
|  |               GET https://api.example.com/health              ||
|  +--------------------------------------------------------------+|
|                                                                   |
|  +--------------------------------------------------------------+|
|  |  [File Icon]  File Exists - Config File              [Pending]||
|  |               Check C:\Config\settings.json exists            ||
|  +--------------------------------------------------------------+|
|                                                                   |
+------------------------------------------------------------------+

Cards: Full width with 12px vertical gap
Hover: Slight elevation + right arrow appears
Click: Opens Test Config view
```

### Visual Details

- **Page header**: "TEST SUITE" title, profile name and count as subtitle
- **Run button**: Large primary gradient button with play icon
- **Test cards**:
  - Left: Type icon in subtle colored circle (24px)
  - Center: Test name (H3) with description below (Caption)
  - Right: Status badge (Pending = gray pill)
- **Hover state**: Card elevates, subtle glow, pointer cursor
- **Click target**: Entire card is clickable

---

## 4. Test Configuration View

### Description

Detailed view of a single test's parameters. Locked fields are clearly marked. Editable fields use modern input styling.

### Layout

```
+------------------------------------------------------------------+
|                                                                   |
|    <- Back to Tests                                               |
|                                                                   |
|    HTTP GET TEST                                                  |
|    Health Endpoint Check                                          |
|                                                                   |
+------------------------------------------------------------------+
|                                                                   |
|    PARAMETERS                                                     |
|    +----------------------------------------------------------+  |
|    |  URL                                              [Lock]  |  |
|    |  +----------------------------------------------------+  |  |
|    |  | https://api.example.com/health                     |  |  |
|    |  +----------------------------------------------------+  |  |
|    |  Company-managed - cannot be modified                     |  |
|    +----------------------------------------------------------+  |
|                                                                   |
|    +----------------------------------------------------------+  |
|    |  Expected Status Codes                                    |  |
|    |  +----------------------------------------------------+  |  |
|    |  | 200, 201                                           |  |  |
|    |  +----------------------------------------------------+  |  |
|    +----------------------------------------------------------+  |
|                                                                   |
|    +----------------------------------------------------------+  |
|    |  Timeout (seconds)                                        |  |
|    |  +----------------------------------------------------+  |  |
|    |  | 30                                                 |  |  |
|    |  +----------------------------------------------------+  |  |
|    +----------------------------------------------------------+  |
|                                                                   |
|    +-------+  +-------+                                          |
|    | Save  |  | Reset |                                          |
|    +-------+  +-------+                                          |
|                                                                   |
+------------------------------------------------------------------+

Locked fields: Reduced opacity (60%), lock icon, helper text
Editable fields: Full opacity, normal input styling
```

### Visual Details

- **Back link**: Ghost button with left arrow
- **Header**: Test type as overline (Caption, uppercase), name as H1
- **Parameter sections**: Grouped in subtle bordered containers
- **Locked fields**:
  - Lock icon (16px) in the top-right of label area
  - Input field disabled with reduced opacity
  - Helper text: "Company-managed - cannot be modified"
- **Editable fields**: Standard input styling with focus states
- **Action buttons**: Primary (Save), Secondary (Reset)

---

## 5. Run Progress View

### Description

An engaging, visually dynamic view during test execution. The circular progress ring dominates the center, with current test status and completed tests below.

### Layout

```
+------------------------------------------------------------------+
|                                                                   |
|                                                                   |
|                        +----------------+                         |
|                        |                |                         |
|                        |      67%       |                         |
|                        |                |                         |
|                        +----------------+                         |
|                            Running...                             |
|                                                                   |
|                        +--------------------------+               |
|                        |  HTTP GET - Health       |  [Running]    |
|                        |  Checking endpoint...    |               |
|                        +--------------------------+               |
|                                                                   |
+------------------------------------------------------------------+
|                                                                   |
|    COMPLETED (6 of 12)                                           |
|    +----------------------------------------------------------+  |
|    |  [Check] Ping Test                               [Passed] |  |
|    |  [Check] File Exists                             [Passed] |  |
|    |  [X]     FTP Connection                          [Failed] |  |
|    |  ...                                                      |  |
|    +----------------------------------------------------------+  |
|                                                                   |
|                          [Cancel Run]                             |
|                                                                   |
+------------------------------------------------------------------+

Progress ring: 120px diameter, animated stroke
Current test: Subtle pulse animation
Completed list: Scrollable, compact cards
```

### Visual Details

- **Progress ring**:
  - Background stroke: Subtle gray
  - Progress stroke: Gradient accent (cyan to violet)
  - Center: Percentage (Display size), "Running..." label below
  - Smooth animation as percentage updates
- **Current test card**:
  - Elevated card with subtle pulse glow
  - Animated status indicator (spinning if in progress)
  - Test name and current action description
- **Completed tests**:
  - Compact list items (40px height)
  - Status icon (check/x) with color coding
  - Status badge on right
- **Cancel button**: Secondary style, positioned at bottom

---

## 6. Results View

### Description

A comprehensive results dashboard with summary statistics, filterable list of test results, and export capabilities.

### Layout

```
+------------------------------------------------------------------+
|                                                                   |
|    RESULTS                                                        |
|    Default Profile - Run completed at 10:45 AM                    |
|                                                                   |
|    [Export JSON]  [Export CSV]                                    |
|                                                                   |
+------------------------------------------------------------------+
|                                                                   |
|    +----------------+  +----------------+  +----------------+     |
|    |     8 / 12     |  |     3 / 12     |  |     1 / 12     |     |
|    |    PASSED      |  |    FAILED      |  |    SKIPPED     |     |
|    |  [Donut Chart] |  |  [Donut Chart] |  |  [Donut Chart] |     |
|    +----------------+  +----------------+  +----------------+     |
|                                                                   |
|    [All: 12]  [Passed: 8]  [Failed: 3]  [Skipped: 1]             |
|                                                                   |
|    +----------------------------------------------------------+  |
|    |  [Check]  HTTP GET - Health                     [Passed]  |  |
|    |           Response: 200 OK in 145ms                       |  |
|    |                                            [Show Details] |  |
|    +----------------------------------------------------------+  |
|                                                                   |
|    +----------------------------------------------------------+  |
|    |  [X]      FTP Connection                        [Failed]  |  |
|    |           Connection refused: ECONNREFUSED                |  |
|    |                                            [Show Details] |  |
|    |  +------------------------------------------------------+ |  |
|    |  | Error: Unable to establish connection to ftp.example | |  |
|    |  | Stack: FtpClient.Connect() at line 45...             | |  |
|    |  +------------------------------------------------------+ |  |
|    +----------------------------------------------------------+  |
|                                                                   |
+------------------------------------------------------------------+

Summary cards: Animated count-up, mini donut visualization
Result cards: Expandable with smooth accordion animation
Filter tabs: Pill-style segmented control
```

### Visual Details

- **Page header**: Title with timestamp, export buttons (icon + text)
- **Summary cards**:
  - Three cards in a row (Passed/Failed/Skipped)
  - Large number with label
  - Mini donut chart visualization (optional)
  - Color-coded backgrounds (subtle tint)
- **Filter tabs**: Segmented pill control showing counts per status
- **Result cards**:
  - Status icon on left (colored)
  - Test name with summary line
  - Status badge on right
  - Expandable details section with smooth animation
  - Technical details in monospace font with code styling
- **Error states**: Red-tinted background for failed test details

---

## 7. Diagnostics View

### Description

A clean, organized view for system information and troubleshooting. Provides quick access to logs and support information.

### Layout

```
+------------------------------------------------------------------+
|                                                                   |
|    DIAGNOSTICS                                                    |
|    System information and troubleshooting tools                   |
|                                                                   |
+------------------------------------------------------------------+
|                                                                   |
|    SYSTEM INFO                                                    |
|    +----------------------------------------------------------+  |
|    |  Machine:      WORKSTATION-01                             |  |
|    |  OS:           Windows 11 Pro (22H2)                      |  |
|    |  .NET:         8.0.4                                      |  |
|    |  App Version:  1.0.0                                      |  |
|    +----------------------------------------------------------+  |
|                                                                   |
|    LAST RUN                                                       |
|    +----------------------------------------------------------+  |
|    |  Run ID:       abc123-def456                              |  |
|    |  Profile:      Default Profile                            |  |
|    |  Time:         2026-01-30 10:45:23                        |  |
|    |  Duration:     2m 34s                                     |  |
|    |  Status:       8 passed, 3 failed, 1 skipped              |  |
|    +----------------------------------------------------------+  |
|                                                                   |
|    ACTIONS                                                        |
|    +----------------+  +----------------+                         |
|    | [Copy] Copy    |  | [Folder] Open  |                        |
|    |  Details       |  |  Logs Folder   |                        |
|    +----------------+  +----------------+                         |
|                                                                   |
+------------------------------------------------------------------+

Info sections: Clean key-value layout in cards
Actions: Large button cards for easy access
```

### Visual Details

- **Page header**: Title with subtitle description
- **Info cards**:
  - Clean two-column layout (label: value)
  - Subtle separator lines between rows
  - Monospace font for technical values
- **Last run section**:
  - Summary with status color indicators
  - Pass/fail/skip counts with colored numbers
- **Action buttons**:
  - Large card-style buttons
  - Icon on left, label on right
  - Hover elevation effect

---

## Animation Specifications

### Page Transitions

```
Type: Slide + Fade
Duration: 200ms
Easing: ease-out
Direction: Slide from right (forward), slide from left (back)
```

### Card Hover

```
Transform: translateY(-2px)
Shadow: Level 2
Duration: 150ms
Easing: ease-in-out
```

### Progress Ring

```
Type: Stroke-dashoffset animation
Duration: 400ms (per update)
Easing: ease-out
Stroke: Gradient applied via pattern/mask
```

### Accordion Expand

```
Type: Height + Opacity
Duration: 250ms
Easing: ease-out
```

### Status Updates

```
Type: Scale pulse + glow
Duration: 300ms
Easing: spring
```

### Staggered List Entrance

```
Delay per item: 50ms
Duration per item: 200ms
Transform: translateY(10px) -> translateY(0)
Opacity: 0 -> 1
```
