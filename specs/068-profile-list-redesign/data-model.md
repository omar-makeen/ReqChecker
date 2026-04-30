# Phase 1 Data Model — Profile Manager List Redesign

**Branch**: `068-profile-list-redesign`
**Date**: 2026-04-30
**Scope**: Presentation only. The persisted `Profile` entity and all infrastructure types remain unchanged. This document describes the in-memory shapes the view binds to.

---

## Existing entity (unchanged)

### `Profile` (`ReqChecker.Core.Models.Profile`)

| Field | Type | Notes |
|---|---|---|
| `Id` | `string` | Unique identifier. The well-known constant `ProfileSelectorViewModel.DefaultProfileId` identifies the recommended profile. |
| `Name` | `string` | Human-readable display name. |
| `SchemaVersion` | `int` | Currently 1–3 in the wild; only displayed when ≥ 1. |
| `Source` | `ProfileSource` enum (`Bundled` / `UserProvided`) | Drives the source chip label. |
| `RunSettings` | `RunSettings` | Not displayed in the list. |
| `Tests` | `List<TestDefinition>` | Count is displayed (`Tests.Count`). |
| `Signature` | `string?` | Not displayed in the list. |

No fields are added. No fields are removed.

---

## New presentation entity

### `ProfileListItemViewModel` (new — `ReqChecker.App.ViewModels`)

A thin per-row VM that wraps a `Profile` plus its row-runtime state. Implements `INotifyPropertyChanged` (via `ObservableObject`) for the two reactive flags (`IsActive`, `IsSelectedInList` — see below).

#### Constructor

```csharp
public ProfileListItemViewModel(Profile profile, string? sourceFilePath, bool isRecommended);
```

- `profile`: the wrapped domain object (kept addressable as `Profile` for command handlers).
- `sourceFilePath`: full path to the underlying JSON file for user profiles; `null` for bundled profiles. Captured at load time by `ProfileSelectorViewModel`.
- `isRecommended`: cached at construction (`profile.Id == ProfileSelectorViewModel.DefaultProfileId`) — see research R8.

#### Public surface (binding targets)

| Member | Type | Semantics |
|---|---|---|
| `Profile` | `Profile` | The wrapped domain object. The page VM uses this when invoking `SelectProfileCommand`. |
| `Name` | `string` | `Profile.Name`. Bound to the dominant text element (FR-010). |
| `SourceLabel` | `string` | `"Bundled"` or `"User"` — derived from `Profile.Source`. Renders inside the quiet outlined chip (FR-014). |
| `TestCountLabel` | `string` | `$"{count} tests"` (or `"1 test"` when count is 1). Bound as secondary metadata (FR-011). |
| `SchemaVersionLabel` | `string?` | `$"v{Profile.SchemaVersion}"` when `SchemaVersion >= 1`; `null` otherwise. View hides the field when `null` (FR-012, edge case "missing optional metadata"). |
| `ModifiedLabel` | `string?` | `null` for bundled profiles. For user profiles, `$"modified {FriendlyDateConverter.Format(LastModifiedUtc)}"`. View hides the field when `null` (FR-013). |
| `LastModifiedUtc` | `DateTime?` | Computed at construction from `File.GetLastWriteTimeUtc(sourceFilePath)`; `null` if path is null or file is missing. |
| `IsRecommended` | `bool` | Set at construction; immutable. Drives the `RecommendedBadge` visibility (FR-015) and the accessible-name suffix (FR-019b). |
| `IsActive` | `bool` (observable) | True when this row's profile is the current `IAppState.CurrentProfile`. Recomputed by the page VM on `CurrentProfileChanged` (FR-009a). |
| `AccessibleName` | `string` | Computed: `Name` + (`" (recommended)"` if `IsRecommended`). Bound to `AutomationProperties.Name` on the row Border (FR-019b). |

#### Computed-property rules

- `SourceLabel`: `Bundled → "Bundled"`, `UserProvided → "User"`. Localization is out of scope.
- `TestCountLabel`: pluralization handled in-VM; the view does not branch.
- `ModifiedLabel`: format with `FriendlyDateConverter` for parity with `HistoryView`. The "modified " prefix is fixed text (per FR-013 wording).
- `IsActive` setter is `internal` — only `ProfileSelectorViewModel` may write it.

#### Validation rules

None (the wrapped `Profile` is already validated upstream by `IProfileValidator`). The row VM is purely projection.

---

## Page-level VM additions

### `ProfileSelectorViewModel` (extended)

Existing fields and commands are preserved. The redesign adds:

| Member | Type | Semantics |
|---|---|---|
| `Items` | `ObservableCollection<ProfileListItemViewModel>` | The list-bound collection. Populated alongside the existing `Profiles` collection during load; one item per loaded profile. |
| `SelectedItem` | `ProfileListItemViewModel?` (observable, two-way) | Bound to `ListBox.SelectedItem`. Setter invokes `SelectProfileCommand(value.Profile)` only when `value` is non-null AND its profile differs from the current `_appState.CurrentProfile` (avoids double-fire when initial selection is set on entry). |
| (private) | event handler on `_appState.CurrentProfileChanged` | Iterates `Items` and toggles each one's `IsActive`. Sets `SelectedItem` to the matching item so the `ListBox` highlights it. |

### Lifecycle

- **Load**: `LoadProfilesAsync` runs as today. After populating `_profiles`, the VM rebuilds `Items` (one `ProfileListItemViewModel` per profile, with the captured file path for user profiles), then sets `SelectedItem` to the item whose profile equals `_appState.CurrentProfile` (if any).
- **Active change**: when `CurrentProfileChanged` fires (e.g., from another screen, or from this very screen's selection), the handler updates `IsActive` on every item and refreshes `SelectedItem`. This satisfies FR-009a.
- **Dispose**: the existing `Dispose()` is extended to unsubscribe from `CurrentProfileChanged`.

---

## Relationships

```text
ProfileSelectorViewModel
  ├── Profiles : ObservableCollection<Profile>          (kept, unchanged for tests/back-compat)
  ├── Items    : ObservableCollection<ProfileListItemViewModel>   (new — bound by the View)
  └── SelectedItem : ProfileListItemViewModel?           (new — two-way to ListBox)

ProfileListItemViewModel
  ├── Profile  : Profile                                (the wrapped entity — read-only)
  ├── (presentation strings, computed)
  ├── IsRecommended : bool                              (immutable)
  └── IsActive      : bool (observable)                 (set by the page VM in response to IAppState events)
```

No new relationships to persisted data. No new persisted fields. No file format changes.

---

## State transitions

The only stateful flag introduced is `IsActive`, which transitions:

```text
IsActive = false ─────────────► IsActive = true
              ▲                      │
              │                      │
              └──────────────────────┘
            (CurrentProfileChanged)
```

Triggered exclusively by `IAppState.CurrentProfileChanged`. Initial value at load time = `(profile.Id == _appState.CurrentProfile?.Id)`.
