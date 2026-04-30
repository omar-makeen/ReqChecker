# View-Model Binding Contract — Profile Manager List Redesign

**Branch**: `068-profile-list-redesign`
**Date**: 2026-04-30
**Purpose**: Define the exact surface that `ProfileSelectorView.xaml` binds to. This is the "API" of the redesign and the contract any test or alternate consumer can rely on.

---

## Page-level (DataContext = `ProfileSelectorViewModel`)

### Properties

| Binding path | Type | Direction | Triggers in view |
|---|---|---|---|
| `Items` | `IEnumerable<ProfileListItemViewModel>` | One-way (collection) | `ListBox.ItemsSource` |
| `SelectedItem` | `ProfileListItemViewModel?` | Two-way | `ListBox.SelectedItem` |
| `IsLoading` | `bool` | One-way | Visibility of the centered progress ring overlay |
| `HasError` | `bool` | One-way | Visibility of the inline error banner |
| `ErrorMessage` | `string?` | One-way | Text inside the inline error banner |
| `ShowWelcomeBanner` | `bool` | One-way | Visibility of the welcome banner |

### Commands

| Binding path | Parameter | Behavior contract |
|---|---|---|
| `LoadProfilesCommand` | none | Re-runs profile loading; flips `IsLoading` true → false. Surfaces errors via `HasError` + `ErrorMessage`. |
| `ImportProfileCommand` | none | Opens the file dialog; on success appends to `Items` and `Profiles`; on failure surfaces error. |
| `SelectProfileCommand` | `Profile` | Sets `IAppState.CurrentProfile` and navigates to Test List. **Idempotent**: invoking with the already-active profile MUST NOT re-navigate. |
| `DismissWelcomeBannerCommand` | none | Sets `IPreferencesService.HasSeenOnboarding = true`; `ShowWelcomeBanner` flips to `false`. |
| `ClearErrorCommand` | none | Clears `HasError` and `ErrorMessage`. |

### Behavior contract: `SelectedItem`

1. When the view sets `SelectedItem` (mouse click or arrow-key navigation), the VM MUST NOT navigate. Selection is purely visual until the user activates the row.
2. When the view sets `SelectedItem` AND the user pressed Enter / Space / double-click, the activation handler in the View invokes `SelectProfileCommand(SelectedItem.Profile)`. The VM is responsible for the navigate-and-set-active behavior; the View is responsible only for translating Enter/Space into the command.
3. When the page VM receives `IAppState.CurrentProfileChanged`, it sets `SelectedItem` to the matching item (or `null` if no match). The corresponding `IsActive` flag is updated on every row.
4. Single click on a row MUST select-and-activate (FR-005). The View invokes `SelectProfileCommand` directly on `MouseLeftButtonUp` of the row Border, bypassing the "selection ≠ activation" rule above. Keyboard requires Enter/Space because moving focus with arrows changes selection but should not load the profile silently.

> **Why split keyboard vs. mouse**: Mouse users expect "click = pick". Keyboard users expect "arrows browse, Enter commits" — moving focus through 50 rows would otherwise navigate to Test List 50 times.

---

## Per-row (DataContext = `ProfileListItemViewModel`)

### Read-only properties bound by the row template

| Binding path | Type | Bound to |
|---|---|---|
| `Name` | `string` | Dominant `TextBlock` (FontSize ≥ 15, FontWeight = SemiBold) |
| `SourceLabel` | `string` | Inner text of the outlined source chip |
| `TestCountLabel` | `string` | Secondary `TextBlock` (font 13, color `TextSecondary`) |
| `SchemaVersionLabel` | `string?` | Secondary `TextBlock`; row hides this segment when `null` |
| `ModifiedLabel` | `string?` | Secondary `TextBlock`; row hides this segment when `null` |
| `IsRecommended` | `bool` | Drives `RecommendedBadge` visibility (`BoolToVisibilityConverter`) |
| `IsActive` | `bool` (notifies) | Drives a row-level data trigger that swaps `Card` ↔ `CardSelected`; also drives `AutomationProperties.PositionInSet` annotation as needed |
| `AccessibleName` | `string` | `AutomationProperties.Name` on the row Border |

### Optional bindings (used for tooltip and a11y)

| Binding path | Type | Bound to |
|---|---|---|
| `Name` | `string` | `ToolTipService.ToolTip` content of the row (FR-010 — full name on hover when truncated) |

### Mutability rules

- All formatted-string properties are computed once at construction. `Profile` does not change identity once the row VM is created — if profiles reload, the page VM rebuilds `Items` rather than mutating individual rows.
- `IsActive` is the **only** mutable property. It is owned by `ProfileSelectorViewModel` (internal setter) and changes only in response to `IAppState.CurrentProfileChanged`.

---

## Wiring contract: `ListBox` configuration

The view's `ListBox` MUST be configured exactly as follows (mirrors `HistoryView`):

```xml
<ListBox ItemsSource="{Binding Items}"
         SelectedItem="{Binding SelectedItem, Mode=TwoWay}"
         SelectionMode="Single"
         Background="Transparent"
         BorderThickness="0"
         ScrollViewer.HorizontalScrollBarVisibility="Disabled"
         ScrollViewer.VerticalScrollBarVisibility="Auto"
         VirtualizingPanel.IsVirtualizing="True"
         VirtualizingPanel.VirtualizationMode="Recycling"
         VirtualizingPanel.ScrollUnit="Pixel"
         AutomationProperties.Name="Profiles list"/>
```

The `ItemContainerStyle` MUST set:
- `Padding=0`, `Margin=0,0,0,12`,
- `HorizontalContentAlignment=Stretch`,
- `Background=Transparent`, `BorderThickness=0`,
- `FocusVisualStyle={StaticResource FocusVisualStyle}`,
- a stripped `Template` whose body is a single `ContentPresenter`.

These values are not negotiable — they are what makes the list visually consistent with `HistoryView` (and therefore with FR-002, SC-001).

---

## Tests required against this contract

Mandatory unit tests in `tests/ReqChecker.App.Tests/ViewModels/`:

1. `ProfileListItemViewModelTests`
   - `Constructor_ComputesIsRecommended_WhenIdMatchesDefault`
   - `Constructor_DefaultsIsActive_ToFalse`
   - `ModifiedLabel_IsNull_WhenSourcePathIsNull`           *(bundled-profile case)*
   - `ModifiedLabel_IsPopulated_FromFileLastWriteTime`     *(user-profile case, uses temp file)*
   - `SchemaVersionLabel_IsNull_WhenSchemaVersionIsZero`
   - `TestCountLabel_PluralizesCorrectly`                  *(0, 1, 2)*
   - `AccessibleName_IncludesRecommendedSuffix_WhenIsRecommended`
   - `IsActive_RaisesPropertyChanged_WhenSet`

2. `ProfileSelectorViewModelTests` (extensions)
   - `LoadProfiles_PopulatesItemsCollection`
   - `LoadProfiles_SetsSelectedItem_WhenAppStateHasCurrentProfile`
   - `OnCurrentProfileChanged_UpdatesIsActiveForMatchingItem`
   - `OnCurrentProfileChanged_ClearsIsActiveForOtherItems`
   - `SelectingItemThatEqualsCurrentProfile_DoesNotRefireNavigation`  *(idempotence)*
   - `Dispose_UnsubscribesFromCurrentProfileChanged`

These tests are sufficient to validate the binding contract end-to-end without a UI test harness.
