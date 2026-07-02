# HTML Wireframes To Unity UI Spec

This document converts the Chai Empire HTML wireframes into a Unity UI implementation contract. It is documentation only. It does not describe code that already exists unless marked as current.

Wireframe source:

```text
wireframes/chai-empire-brief-assets/ux-frames/chai-empire-wireframes.html
```

## Purpose

The wireframes define:

- Screen hierarchy.
- Player-facing state changes.
- Popup inventory.
- Tap targets.
- Information priority.
- Unity component boundaries.

They do not define:

- Final art direction.
- Final color palette.
- Animation timing.
- Sound design.
- Monetization surfaces.
- Figma components.

## Current Baseline

Current implemented UI:

| Area | Current implementation |
| --- | --- |
| UI creation | Runtime-created by `ChaiGamePresenter`. |
| Canvas mode | Screen Space Overlay. |
| Scaling | `CanvasScaler` with reference resolution `1080 x 1920`. |
| Layout | One scrollable portrait screen. |
| Section order | Header, stats, tutorial guide when needed, actions, upgrades, locations, prestige preview. |
| Refresh | Dynamic UI refresh every `0.2` seconds. |
| Save | Save every `10` seconds and on pause/quit. |

The HTML wireframes preserve this baseline so the first production UI can replace the runtime-created prototype without changing the economy model.

## Frame Inventory

| Frame ID | Product state | Unity purpose |
| --- | --- | --- |
| `main-stall-fresh` | New player at Gali Tapri with no upgrades. | Default first launch state. |
| `first-upgrade-ready` | Player has enough rupees for Strong Tea Leaves. | First affordance highlight and purchase feedback. |
| `automation-running` | Passive production is active. | Shows transition from tapping to management. |
| `location-unlock-ready` | Bus Stand/Railway Platform can be unlocked. | Teaches location demand multipliers. |
| `rush-hour-active` | Rush Hour is active with timer. | Active burst state and temporary multiplier feedback. |
| `offline-reward-popup` | Player returns after meaningful absence. | Offline reward modal contract. |
| `upgrade-detail-popup` | Upgrade secondary detail is open. | Planned detail modal contract. |
| `prestige-preview-locked` | Late-game player nearing prestige conditions. | Secret Masala locked preview and aspirational target. |

## Unity Scene Hierarchy

Recommended production hierarchy:

```text
ChaiEmpireScene
  Main Camera
  Chai Empire App
    Canvas
      SafeAreaRoot
        MainScrollView
          Viewport
            ContentColumn
              HeaderPanel
              StatsGrid
              TutorialPanel
              ActionPanel
              UpgradeSection
              LocationSection
              PrestigePreviewPanel
        ToastStatus
        ModalLayer
          OfflineRewardModal
          UpgradeDetailModal
```

Current scene can keep the same `Chai Empire App` root and `ChaiGamePresenter` during prototype work. Production work should split UI responsibilities into smaller view classes.

## Recommended View Classes

| Class | Responsibility | Depends on |
| --- | --- | --- |
| `ChaiHudView` | Coordinates header, stats, actions, sections, toast, and modal stack. | `ChaiGame`, content definitions. |
| `ChaiHeaderView` | Shows title, current location, and demand multiplier. | `ChaiGame.GetDemandMultiplier()`, unlocked locations. |
| `ChaiStatsView` | Shows rupees, passive/sec, tap value, chai served, legacy/lifetime as needed. | `ChaiGameState`, `ChaiGame` calculations. |
| `ChaiTutorialView` | Shows first-tap and first-upgrade guide until Strong Tea Leaves is bought. | `ChaiTutorial.GetPrompt()`. |
| `ChaiActionView` | Owns Tap Kettle, Serve Queue, Rush Hour buttons and timer label. | `TapKettle`, `TapCustomerQueue`, `TryTriggerRushHour`. |
| `UpgradeListView` | Renders upgrade cards and purchase states. | `ChaiContent.Upgrades`, `TryBuyUpgrade`. |
| `UpgradeCardView` | One upgrade row/card. | `UpgradeDefinition`, current level, current cost. |
| `LocationListView` | Renders location cards and unlock states. | `ChaiContent.Locations`, `TryUnlockLocation`. |
| `LocationCardView` | One location row/card. | `LocationDefinition`, unlocked/affordable state. |
| `PrestigePreviewView` | Shows current Secret Masala preview only. | `ChaiGame.GetPrestigePreview()`. |
| `SettingsView` | Shows local save controls. | `ChaiSaveRepository.DeleteSave()`. |
| `OfflineRewardModalView` | Shows return reward and claim button. | `LoadResult.OfflineReward`. |
| `UpgradeDetailModalView` | Planned detail view for secondary upgrade info. | `UpgradeDefinition`, computed cost. |
| `ToastStatusView` | Shows short action feedback. | UI events. |

## Layout Contract

Use the current Unity baseline:

| Property | Value |
| --- | --- |
| Orientation | Portrait only. |
| Reference resolution | `1080 x 1920`. |
| Main safe margin | At least `24 px`; increase near cutouts/navigation bars. |
| Primary tap target | Large, lower-middle, reachable by thumb. |
| Button minimum height | `72 px` or larger for production. |
| Text scaling | Do not scale fonts with viewport width. Use responsive wrapping and compact number formatting. |

Recommended production structure:

| Region | Content | Behavior |
| --- | --- | --- |
| Header | `Chai Empire`, current highest location, demand multiplier. | Always visible at top of scroll. |
| Stall Art | Procedural kettle, stove, burner, flame, steam, customer queue, and UPI QR prop shapes. | First-screen visual anchor; steam wisps animate, customer/payment props stay visible, and backdrop palette follows the highest unlocked location. |
| Stats | Rupees, passive/sec, tap value, chai served or lifetime. | Refresh at fixed cadence. |
| Tutorial | First tap and first upgrade prompt. | Visible only until Strong Tea Leaves is bought. |
| Actions | Tap Kettle, Serve Queue, Rush Hour. | Highest priority interaction area. |
| Upgrades | 3-5 visible upgrade cards before scroll continues. | Cards show level, category/effect, cost, enabled state. |
| Locations | Near-term location cards. | Cards show demand multiplier and locked/unlocked/cost. |
| Prestige | Secret Masala preview. | Preview only until prestige update. |

## Component Prefabs

### Upgrade Card

Required fields:

| Field | Example |
| --- | --- |
| Display name | `Strong Tea Leaves` |
| Level | `Lv 0` |
| Category | `Brew Craft` |
| Effect label | `+1 tap flat` |
| Cost label | `Rs 10` |
| State | Affordable, locked/insufficient, purchased level. |

Button behavior:

1. On tap, call `ChaiGame.TryBuyUpgrade(upgradeId)`.
2. If successful, refresh stats, upgrade card, and status toast.
3. If unsuccessful, keep button disabled or show subtle insufficient feedback.

### Location Card

Required fields:

| Field | Example |
| --- | --- |
| Display name | `Railway Platform` |
| Demand multiplier | `x1.65` |
| State label | `Unlocked`, `Rs 2K`, or `BUY Rs 2K`. |

Button behavior:

1. On tap, call `ChaiGame.TryUnlockLocation(locationId)`.
2. If successful, update demand multiplier, location state, stats, and status toast.
3. Highest unlocked location becomes the displayed location in the header.

### Action Buttons

| Button | Binding | State |
| --- | --- | --- |
| Tap Kettle | `ChaiGame.TapKettle()` | Always enabled. |
| Serve Queue | `ChaiGame.TapCustomerQueue()` | Always enabled in current prototype. |
| Rush Hour | `ChaiGame.TryTriggerRushHour()` | Enabled only when cooldown is `0`. |

Rush label states:

| State | Label |
| --- | --- |
| Ready | `Rush ready` or `Rush Hour` with ready sublabel. |
| Active | `Rush active <seconds> sec`. |
| Cooldown | `Rush ready in <seconds> sec`. |

### Tutorial Guide

Current runtime implementation uses `ChaiTutorial` to derive guidance from game state without adding save fields.

| State | Title | Primary action |
| --- | --- | --- |
| No chai served | `Brew your first chai` | Tap Kettle. |
| Some chai served, less than first upgrade cost | `Save for Strong Tea Leaves` | Tap Kettle. |
| First upgrade affordable | `First upgrade ready` | Buy Strong Tea Leaves. |
| Strong Tea Leaves bought | Hidden. | None. |

## Modal Contracts

### Offline Reward Modal

Current code shows the offline reward in a modal overlay and also posts a short return status.

Fields:

| Field | Source |
| --- | --- |
| Reward amount | `LoadResult.OfflineReward.RupeesEarned` |
| Offline time | `OfflineReward.RawSeconds` formatted compactly. |
| Efficiency | `ChaiContent.OfflineEfficiency`, currently `75%`. |
| Cap | `ChaiContent.OfflineCapSeconds`, currently `8h`. |
| Claim button | Dismisses modal only; reward is already applied on load. |

Rules:

- Show only when `LoadResult.HasOfflineReward` is true.
- Do not include rewarded-ad doubling in v1 unless monetization is explicitly implemented later.
- If elapsed time was capped, show cap text; otherwise the cap row can still be shown as a rule reminder.

### Upgrade Detail Modal

Current code does not implement this modal. It is a planned UI improvement for reducing clutter on upgrade cards.

Fields:

| Field | Source |
| --- | --- |
| Upgrade name | `UpgradeDefinition.DisplayName` |
| Category | `UpgradeDefinition.Category` |
| Level | `ChaiGameState.GetUpgradeLevel(id)` |
| Effect per level | `UpgradeDefinition.ValuePerLevel` and `UpgradeKind` |
| Current cost | `UpgradeDefinition.GetCost(currentLevel)` |
| Scaling | `UpgradeDefinition.CostMultiplier` |

Rules:

- Main upgrade card remains concise.
- Detail modal contains secondary math and flavor.
- Buy button in modal calls the same `TryBuyUpgrade(id)` path as the card.

## Screen-State Rules

### Fresh Stall

- Rupees are `Rs 0`.
- Passive/sec is `Rs 0/s`.
- Tap value is `Rs 1`.
- Tutorial guide says `Brew your first chai` and exposes a `Tap Kettle` action.
- Strong Tea Leaves is visible but disabled until `Rs 10`.
- Secret Masala is locked.

### First Upgrade Ready

- Strong Tea Leaves card becomes affordable at `Rs 10`.
- Status/toast may say `Strong Tea Leaves available`.
- Buying it increases tap value from `Rs 1` to `Rs 2`.

### Automation Running

- Passive/sec is greater than zero after passive upgrades.
- Automation upgrades should read as management tools, not chores.
- Location unlocks start becoming meaningful alternatives to more passive upgrades.

### Location Unlock Ready

- Affordable location cards use active visual state.
- Demand multiplier only changes after unlock.
- Header location should update to the highest unlocked demand location.

### Rush Hour Active

- Rush state should be obvious without adding explanatory text.
- Timer and/or progress meter must be visible.
- Tap value and passive/sec already include the 2x multiplier while active.
- Offline reward must not include rush.

### Prestige Preview Locked

- Show Secret Masala as locked until Airport Lounge and `1B` lifetime rupees are reached.
- Keep prestige reset separate from the Settings save wipe. Settings reset is a two-step local save delete, not a prestige action.
- Keep wording aspirational and clear.

## Data Binding Map

| UI value | Source |
| --- | --- |
| Rupees | `ChaiGameState.Rupees` |
| Passive/sec | `ChaiGame.GetPassiveRupeesPerSecond()` |
| Tap value | `ChaiGame.GetTapValue()` |
| Chai served | `ChaiGameState.ChaiServed` |
| Lifetime rupees | `ChaiGameState.TotalLifetimeRupees` |
| Masala Legacy | `ChaiGameState.Prestige.MasalaLegacy` |
| Upgrade level | `ChaiGameState.GetUpgradeLevel(id)` |
| Upgrade cost | `UpgradeDefinition.GetCost(level)` |
| Location unlocked | `ChaiGameState.IsLocationUnlocked(id)` |
| Demand multiplier | `ChaiGame.GetDemandMultiplier()` |
| Prestige preview | `ChaiGame.GetPrestigePreview()` |
| Rush timer | `ChaiGameState.RushRemainingSeconds` |
| Rush cooldown | `ChaiGameState.RushCooldownSeconds` |

## Feedback And Animation Contract

Keep production feedback lightweight for low-end Android devices:

| Action | Feedback |
| --- | --- |
| Any enabled button | Short generated click. |
| Tap Kettle | Button scale, tiny floating `+Rs`, persistent kettle steam ambience. |
| Serve Queue | Short status toast and rupee float. |
| Buy Upgrade | Card pulse, level increment, toast, generated purchase cue. |
| Unlock Location | Location card unlock pulse, header location update, generated unlock cue. |
| Rush Hour | Timer/progress meter, 2x state on action area, optional haptic. |
| Offline Claim | Modal dismiss and brief rupee total pulse. |

## Accessibility And Mobile Rules

- Use large text for rupees and rates.
- Keep button labels short.
- All tappable controls should be comfortable for one-thumb use.
- Disabled state must be visually distinct without relying on color alone.
- Important text must not clip at smaller Android resolutions.
- Avoid hover-only affordances.
- Avoid dense tables in runtime UI.

## Testing Checklist For Unity Implementation

- Fresh save shows the `main-stall-fresh` state.
- Fresh save shows the tutorial guide and advances after the tutorial primary action.
- At `Rs 10`, Strong Tea Leaves becomes tappable.
- Buying Strong Tea Leaves updates tap value immediately.
- Passive upgrades update passive/sec without restarting the scene.
- Rush Hour updates timer, disables the button, and restores cooldown state.
- Offline reward modal only appears after meaningful absence.
- Location unlock updates demand and header location.
- Prestige preview stays locked before Airport Lounge plus `1B` lifetime rupees.
- Scroll works on low-end Android portrait devices.
- No button text clips at `1080 x 1920`, `720 x 1280`, and devices with font scaling.

## Handoff Rule

Treat the HTML wireframes as UX truth for structure and state logic. Treat this Markdown file as the Unity implementation contract. Final visual design, art, animation, and audio should be designed after this structure is approved.
