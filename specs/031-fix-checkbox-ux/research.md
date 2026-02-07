# Research: Fix Checkbox Visibility and Select All Placement

**Feature**: 031-fix-checkbox-ux
**Date**: 2026-02-07

## R1: WPF Path Geometry Rendering in Custom CheckBox Templates

**Decision**: Rescale the checkmark Path `Data` coordinates to fit within the 15x15 inner area (18x18 border minus 1.5px border thickness on each side).

**Rationale**: The current Path `Data="M3.5,7.5 L6.5,10.5 L14.5,2.5"` defines a checkmark with a bounding box of approximately 11x8 pixels. While these coordinates fit within a 15x15 area in theory, the Path element has no explicit `Width`, `Height`, or `Stretch` property set. In WPF, a Path with `Stretch="None"` (the default) renders at its natural geometry size relative to the top-left origin of its parent. The path's rightmost point (14.5) plus the 2px stroke extends to ~16.5px, which exceeds the inner width after border insets. Additionally, the Path uses `SnapsToDevicePixels="True"` which can cause sub-pixel clipping. Reducing the coordinates to fit comfortably within the container (e.g., `M4,9 L7,12 L13,4`) with explicit centering ensures visibility.

**Alternatives considered**:
- Setting `Stretch="Uniform"` with explicit Width/Height — adds complexity, could distort the glyph
- Using a TextBlock with Unicode checkmark character — less control over stroke styling
- Using WPF-UI's built-in SymbolIcon for checkmark — doesn't match the custom style design

## R2: Select All Toolbar Placement Pattern

**Decision**: Add a new Grid row (Auto height) between the header and test list, containing the Select All checkbox and label.

**Rationale**: Common patterns in test runner UIs (e.g., Visual Studio Test Explorer, JetBrains Rider) place bulk selection controls in a toolbar row directly above the list they control, not in the page title bar. This creates a clear spatial relationship between the control and the items it affects. The toolbar row should only be visible when a profile is loaded (matching the test list visibility).

**Alternatives considered**:
- Keeping Select All in the header but with better spacing — doesn't solve the spatial disconnection issue
- Adding Select All as the first item in the ListBox — mixes control with content, breaks virtualization
- Floating toolbar overlay — overengineered for a single checkbox
