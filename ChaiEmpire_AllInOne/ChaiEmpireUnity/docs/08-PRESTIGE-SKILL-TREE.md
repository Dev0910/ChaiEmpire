# Prestige And Skill Tree

This document defines the implemented Masala Legacy prestige system and the first skill tree.

## Current Implementation

Implemented:

- `PrestigeState.MasalaLegacy`
- `PrestigeState.UnspentSkillPoints`
- `PrestigeState.Skills`
- `PrestigeSkillEntry.Id`
- `PrestigeSkillEntry.Level`
- `ChaiGame.GetPrestigePreview()`
- `ChaiGame.TryPrestige()`
- `ChaiGame.TrySpendSkillPoint()`
- `ChaiPrestigeSkills`
- Secret Masala confirmation UI
- Skill tree UI and skill spending
- Skill effects in tap, passive, cost, offline, rush, and global formulas

## Current Unlock Condition

Prestige preview becomes available when:

```text
airport-lounge is unlocked
AND TotalLifetimeRupees >= 1,000,000,000
```

If locked, current message:

```text
Reach the Airport Lounge and 1B lifetime rupees to preserve Secret Masala.
```

If unlocked, current message:

```text
Secret Masala is ready. Prestige skill tree arrives in a future update.
```

Current unlocked message:

```text
Secret Masala is ready. Preserve it to reset this run and earn skill points.
```

## Current Projected Reward Formula

```text
projectedMasalaLegacy = floor(sqrt(TotalLifetimeRupees / 10,000,000))
```

Example:

| Total lifetime rupees | Projected Masala Legacy |
| ---: | ---: |
| 1,000,000,000 | 10 |
| 4,000,000,000 | 20 |
| 9,000,000,000 | 30 |

## Current Legacy Multiplier

Current production formulas already include:

```text
legacyMultiplier = 1 + MasalaLegacy * 0.01
```

This means each Masala Legacy gives +1 percent to tap and passive income if the field is set.

## Design Goal

Prestige should feel like preserving the secret recipe, not deleting progress.

The first prestige should:

- Arrive after the first full empire arc.
- Make early game dramatically faster.
- Unlock visible new decisions through the skill tree.
- Avoid making the player repeat slow manual tapping for too long.

## Prestige Reset Rules

Reset:

- Rupees.
- Upgrade levels.
- Location unlocks except default `gali-tapri`.
- Rush timers.
- Current run counters if added later.

Keep:

- Masala Legacy.
- Skill tree levels.
- Unspent skill points.
- Lifetime prestige count if added.
- Cosmetics if added.
- Settings.
- Possibly total historical rupees if needed for stats.

Implementation contract:

```text
Preview prestige reward.
Ask for confirmation.
If confirmed:
  Add projected reward to MasalaLegacy.
  Add skill points if using separate points.
  Reset run state.
  Save immediately.
  Refresh UI.
```

## Skill Tree Branches

The first implementation keeps five branches and exposes them as spendable rows in the Secret Masala panel.

| Branch | Fantasy | Mechanical focus |
| --- | --- | --- |
| Brew Craft | Better recipe mastery. | Manual tap value, recipe multipliers, active bursts. |
| Operations | Better staff and workflow. | Automation, save cadence, idle speed, rush management. |
| Supply | Better ingredients and procurement. | Upgrade cost reduction, supply contracts, offline income. |
| Brand | Better demand and loyalty. | Global multipliers, location demand, events. |
| Expansion | Better rollout strategy. | Faster location unlocks, franchise bonuses, map expansion. |

## Implemented Skills

Current values are starting balance and should be tuned after late-game playtests.

### Brew Craft

| Skill ID | Name | Max level | Effect |
| --- | --- | ---: | --- |
| `brew-stronger-start` | Stronger First Pour | 5 | +20% tap value per level. |
| `brew-rush-taps` | Rush Brewing | 5 | +10% tap value during Rush Hour per level. |

### Operations

| Skill ID | Name | Max level | Effect |
| --- | --- | ---: | --- |
| `ops-helper-training` | Helper Training | 5 | +10% passive income per level. |
| `ops-fast-rush` | Faster Rush Prep | 4 | Reduce Rush Hour cooldown by 5 seconds per level. |

### Supply

| Skill ID | Name | Max level | Effect |
| --- | --- | ---: | --- |
| `supply-bulk-buying` | Bulk Buying | 5 | Reduce upgrade costs by 2% per level. |
| `supply-offline-flask` | Offline Flask | 5 | Increase offline efficiency by 5% per level. |
| `supply-long-storage` | Long Storage | 4 | Increase offline cap by 1 hour per level. |

### Brand

| Skill ID | Name | Max level | Effect |
| --- | --- | ---: | --- |
| `brand-loyal-regulars` | Loyal Regulars | 5 | +5% global income per level. |

### Expansion

| Skill ID | Name | Max level | Effect |
| --- | --- | ---: | --- |
| `expand-cheaper-locations` | Better Rent Deals | 5 | Reduce location costs by 3% per level. |

## Skill Effect Integration Points

Skill effects are applied in `ChaiGame` through helper methods:

| Effect type | Suggested integration |
| --- | --- |
| Tap multiplier | Multiplied in `GetTapValue()`. |
| Rush tap multiplier | Added while Rush Hour is active in `GetTapValue()`. |
| Passive multiplier | Multiplied in `GetPassiveRupeesPerSecond()`. |
| Global multiplier | Added in `GetGlobalMultiplier()`. |
| Upgrade cost reduction | Applied in `GetUpgradeCost()` and `TryBuyUpgrade()`. |
| Location cost reduction | Applied in `GetLocationUnlockCost()` and `TryUnlockLocation()`. |
| Offline efficiency | Applied in `ApplyOfflineProgress()` through `GetOfflineEfficiency()`. |
| Offline cap | Applied in `ApplyOfflineProgress()` through `GetOfflineCapSeconds()`. |
| Rush cooldown | Skill-adjusted in `TryTriggerRushHour()`. |

## Prestige UI Requirements

Prestige UI shows:

- Current Masala Legacy.
- Projected Masala Legacy gain.
- What will reset.
- What will be kept.
- A clear confirmation button.
- Skill tree preview before first prestige if possible.
- Spendable skill rows with branch, level, and effect text.

Confirmation copy should be explicit:

```text
Preserve Secret Masala?
This resets rupees, upgrades, and locations, but keeps Masala Legacy and skill tree progress.
```

## Prestige Testing Requirements

Tests cover:

- Prestige remains locked before airport/lounge and 1B lifetime rupees.
- Projected reward formula is correct.
- Confirming prestige adds reward.
- Reset clears run state.
- Reset preserves Masala Legacy.
- Reset preserves skill levels.
- Skill effects apply after reset.
- Save/load preserves post-prestige state.
