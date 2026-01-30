# ReqChecker Design System

## Color Palette

### Dark Theme (Primary)

```
Background Layers:
- Base:         #0f0f1a (deepest background)
- Surface:      #1a1a2e (cards, panels)
- Elevated:     #252542 (raised elements)
- Overlay:      #2f2f52 (modals, dropdowns)

Text:
- Primary:      #ffffff (main text)
- Secondary:    #a0a0b8 (supporting text)
- Tertiary:     #6b6b80 (disabled, hints)

Borders:
- Subtle:       #2a2a45 (dividers)
- Default:      #3a3a5c (cards)
- Strong:       #4a4a6e (focus)
```

### Light Theme

```
Background Layers:
- Base:         #f8f9fa (page background)
- Surface:      #ffffff (cards, panels)
- Elevated:     #ffffff (raised elements with shadow)
- Overlay:      #ffffff (modals with shadow)

Text:
- Primary:      #1a1a2e (main text)
- Secondary:    #6b7280 (supporting text)
- Tertiary:     #9ca3af (disabled, hints)

Borders:
- Subtle:       #e5e7eb (dividers)
- Default:      #d1d5db (cards)
- Strong:       #9ca3af (focus)
```

### Accent Colors

```
Primary Accent Gradient:
- Start:        #00d9ff (cyan)
- End:          #6366f1 (violet)

Semantic Colors:
- Success:      #10b981 (emerald)
- Warning:      #f59e0b (amber)
- Error:        #ef4444 (red)
- Info:         #3b82f6 (blue)

Status Colors:
- Pass:         #10b981 with glow rgba(16, 185, 129, 0.3)
- Fail:         #ef4444 with glow rgba(239, 68, 68, 0.3)
- Skip:         #f59e0b with glow rgba(245, 158, 11, 0.3)
```

## Typography

### Font Family

```
Primary:    Segoe UI Variable, Segoe UI, -apple-system, BlinkMacSystemFont, sans-serif
Monospace:  Cascadia Code, Consolas, Monaco, monospace
```

### Type Scale

| Name      | Size  | Weight    | Line Height | Usage                      |
|-----------|-------|-----------|-------------|----------------------------|
| Display   | 32px  | SemiBold  | 1.2         | Hero sections, main titles |
| H1        | 24px  | SemiBold  | 1.25        | Page titles                |
| H2        | 20px  | SemiBold  | 1.3         | Section headers            |
| H3        | 16px  | SemiBold  | 1.4         | Card titles                |
| Body      | 14px  | Regular   | 1.5         | Main content               |
| Body Bold | 14px  | Medium    | 1.5         | Emphasis                   |
| Caption   | 12px  | Regular   | 1.4         | Secondary info, labels     |
| Tiny      | 11px  | Regular   | 1.3         | Badges, timestamps         |

## Spacing System

### Base Unit: 8px

| Token  | Value | Usage                              |
|--------|-------|------------------------------------|
| xs     | 4px   | Tight gaps, icon padding           |
| sm     | 8px   | Small gaps, compact layouts        |
| md     | 12px  | Default element spacing            |
| base   | 16px  | Standard gaps, card padding        |
| lg     | 24px  | Section spacing, large padding     |
| xl     | 32px  | Major section breaks               |
| 2xl    | 48px  | Page sections, hero areas          |
| 3xl    | 64px  | Maximum spacing                    |

## Border Radius

| Token  | Value | Usage                              |
|--------|-------|------------------------------------|
| sm     | 4px   | Input fields, small buttons        |
| md     | 8px   | Cards, containers, buttons         |
| lg     | 12px  | Modals, larger containers          |
| xl     | 16px  | Special emphasis cards             |
| full   | 9999px| Pills, circular avatars            |

## Shadows & Elevation

### Dark Theme (Glows)

```
Level 1 (Cards):
  box-shadow: 0 0 20px rgba(0, 217, 255, 0.05)

Level 2 (Hover):
  box-shadow: 0 0 30px rgba(0, 217, 255, 0.1)

Level 3 (Modals):
  box-shadow: 0 0 60px rgba(0, 217, 255, 0.15)

Focus Ring:
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.5)
```

### Light Theme (Shadows)

```
Level 1 (Cards):
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06)

Level 2 (Hover):
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1), 0 2px 4px rgba(0, 0, 0, 0.06)

Level 3 (Modals):
  box-shadow: 0 20px 25px rgba(0, 0, 0, 0.15), 0 10px 10px rgba(0, 0, 0, 0.04)

Focus Ring:
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.3)
```

## Animation Timings

### Duration

| Token    | Value  | Usage                              |
|----------|--------|------------------------------------|
| instant  | 0ms    | Immediate feedback                 |
| fast     | 100ms  | Button press, micro-interactions   |
| normal   | 200ms  | Hover states, navigation           |
| slow     | 300ms  | Theme transitions, modals          |
| slower   | 400ms  | Page transitions                   |

### Easing Functions

```
ease-out:     cubic-bezier(0, 0, 0.2, 1)     - Entering elements
ease-in:      cubic-bezier(0.4, 0, 1, 1)     - Exiting elements
ease-in-out:  cubic-bezier(0.4, 0, 0.2, 1)   - Moving elements
spring:       cubic-bezier(0.34, 1.56, 0.64, 1) - Bouncy interactions
```

## Component Specifications

### Navigation Sidebar

```
Width:
- Collapsed: 72px
- Expanded:  240px

Item Height: 48px
Icon Size:   24px
Active Indicator: 3px accent left border

Transitions:
- Width: 200ms ease-out
- Active indicator: 150ms ease-in-out
```

### Cards

```
Padding: 16px
Border Radius: 8px
Border: 1px solid (theme border color)

States:
- Default: Surface background
- Hover: Elevated background + Level 2 shadow
- Selected: Accent border (2px)
```

### Buttons

```
Height:
- Small:   32px
- Medium:  40px  (default)
- Large:   48px

Padding:
- Horizontal: 16px
- Icon-only:  12px

Border Radius: 8px

Variants:
- Primary:   Gradient background, white text
- Secondary: Transparent background, accent border, accent text
- Ghost:     Transparent, text color, hover background
```

### Input Fields

```
Height: 40px
Padding: 12px horizontal
Border Radius: 6px
Border: 1px solid (theme border color)

States:
- Default: Border subtle
- Hover:   Border default
- Focus:   Border accent + focus ring
- Error:   Border error + error ring
```

### Status Badges

```
Height: 24px
Padding: 6px 12px
Border Radius: 9999px (pill)
Font Size: 12px
Font Weight: Medium

Glow Effect:
- box-shadow: 0 0 8px (status color at 30% opacity)
```

### Progress Ring

```
Size: 120px diameter
Stroke Width: 8px
Background Stroke: Theme border subtle
Progress Stroke: Gradient accent

Center Text:
- Percentage: 32px SemiBold
- Label: 12px Regular Secondary

Animation: Smooth stroke-dashoffset transition
```

## Iconography

### Icon Set: Fluent System Icons (WPF-UI)

### Common Icons

| Context          | Icon Name           |
|------------------|---------------------|
| Profiles         | Person20            |
| Tests            | BeakerSettings20    |
| Results          | CheckmarkCircle20   |
| Diagnostics      | Bug20               |
| Settings         | Settings20          |
| Theme            | WeatherMoon20 / Sun20 |
| Run              | Play20              |
| Stop             | Stop20              |
| Export           | ArrowDownload20     |
| Import           | ArrowUpload20       |
| Pass             | CheckmarkCircle20   |
| Fail             | DismissCircle20     |
| Skip             | ArrowForward20      |
| Lock             | LockClosed20        |
| Expand           | ChevronDown20       |
| Collapse         | ChevronUp20         |
| Copy             | Copy20              |
| Open Folder      | FolderOpen20        |
| Refresh          | ArrowSync20         |

### Icon Sizing

| Context          | Size    |
|------------------|---------|
| Navigation       | 24px    |
| Buttons          | 20px    |
| Inline           | 16px    |
| Badge            | 14px    |

## Layout Grid

### Main Content Area

```
Max Width: 1400px
Margins: 32px horizontal (24px on smaller screens)
Column Gap: 24px
```

### Sidebar Layout

```
Sidebar: 72px / 240px
Content: Remaining width with max-width
```

### Card Grid

```
Columns: 1-3 depending on container width
Gap: 16px
Breakpoints:
- < 800px:  1 column
- 800-1200px: 2 columns
- > 1200px: 3 columns (where applicable)
```

## Responsive Behavior

### Breakpoints

| Name   | Min Width | Behavior                           |
|--------|-----------|-----------------------------------|
| sm     | 640px     | Single column, collapsed sidebar  |
| md     | 768px     | Two column where applicable       |
| lg     | 1024px    | Full sidebar expanded option      |
| xl     | 1280px    | Maximum comfortable viewing       |

### Sidebar Behavior

- Below 768px: Collapsed by default, overlay on expand
- 768px and up: Collapsed or expanded based on preference
- User preference persisted in local storage
