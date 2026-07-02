# Economy Balancing

This document captures the current implemented economy values and gives guidance for future tuning.

## Currency And Counters

| Name | Type | Current implementation | Use |
| --- | --- | --- | --- |
| Rupees | `BigDouble` | `ChaiGameState.Rupees` | Spendable primary currency. |
| Total Lifetime Rupees | `BigDouble` | `ChaiGameState.TotalLifetimeRupees` | Tracks all earned rupees for prestige preview. |
| Chai Served | `BigDouble` | `ChaiGameState.ChaiServed` | Lifetime-ish count of manual cups served. |
| Masala Legacy | `BigDouble` | `PrestigeState.MasalaLegacy` | Future prestige currency, currently persisted and used as multiplier if set. |
| Skill Points | `int` | `PrestigeState.UnspentSkillPoints` | Future skill tree points, currently persisted only. |

All large economy values use `BreakInfinity.BigDouble`.

## Core Formulas

### Upgrade Cost

```text
upgradeCost(currentLevel) = baseCost * costMultiplier ^ currentLevel
```

Notes:

- `currentLevel` starts at 0.
- Most upgrades have unlimited levels because `maxLevel` defaults to 0.
- A `maxLevel` greater than 0 means the upgrade cannot exceed that level.

### Tap Value

```text
tapValue =
  (1 + tapFlatTotal)
  * tapMultiplier
  * demandMultiplier
  * globalMultiplier
  * legacyMultiplier
  * rushMultiplier

tapFlatTotal = sum(TapFlat.valuePerLevel * level)
tapMultiplier = 1 + sum(TapMultiplier.valuePerLevel * level)
```

### Passive Rupees Per Second

```text
passivePerSecond =
  passiveFlatTotal
  * demandMultiplier
  * globalMultiplier
  * legacyMultiplier
  * rushMultiplierIfIncluded

passiveFlatTotal = sum(PassiveFlat.valuePerLevel * level)
```

### Demand Multiplier

```text
demandMultiplier = max(unlockedLocation.demandMultiplier)
```

### Global Multiplier

```text
globalMultiplier = 1 + sum(GlobalMultiplier.valuePerLevel * level)
```

### Legacy Multiplier

```text
legacyMultiplier = 1 + MasalaLegacy * 0.01
```

This means each Masala Legacy is currently worth +1 percent to production if manually granted in state. Actual prestige earning/reset is planned, not implemented.

### Rush Multiplier

```text
rushMultiplier = 2 while RushRemainingSeconds > 0, otherwise 1
```

### Offline Reward

```text
rawSeconds = max(0, elapsedSeconds)
cappedSeconds = min(rawSeconds, 28800)
offlineReward = passivePerSecondWithoutRush * cappedSeconds * 0.75
```

## Current Global Constants

| Constant | Value | Source |
| --- | --- | --- |
| Legacy multiplier per Masala Legacy | `0.01` | `ChaiGame.LegacyMultiplierPerMasala` |
| Rush duration | `20` seconds | `ChaiGame.RushDurationSeconds` |
| Rush cooldown | `90` seconds | `ChaiGame.RushCooldownSeconds` |
| Rush multiplier | `2` | `ChaiGame.RushMultiplier` |
| Offline efficiency | `0.75` | `ChaiContent.CreateDefault()` |
| Offline cap | `28800` seconds | `ChaiContent.CreateDefault()` |
| Prestige lifetime rupee threshold | `1,000,000,000` | `ChaiContent.CreateDefault()` |
| Prestige location requirement | `airport-lounge` | `ChaiGame.GetPrestigePreview()` |

## Current Upgrade Balance Table

| ID | Display name | Category | Kind | Base cost | Cost multiplier | Value per level | Automation |
| --- | --- | --- | --- | ---: | ---: | ---: | --- |
| `strong-tea` | Strong Tea Leaves | Brew Craft | TapFlat | 10 | 1.55 | 1 | No |
| `adrak-kick` | Adrak Kick | Brew Craft | TapMultiplier | 45 | 1.6 | 0.15 | No |
| `elaichi-aroma` | Elaichi Aroma | Brew Craft | TapMultiplier | 120 | 1.66 | 0.2 | No |
| `helper-boy` | Helper Boy | Automation | PassiveFlat | 50 | 1.65 | 0.5/sec | Yes |
| `upi-cashier` | UPI Cashier | Automation | PassiveFlat | 95 | 1.68 | 0.35/sec | Yes |
| `bulk-kettle` | Bulk Kettle | Automation | PassiveFlat | 180 | 1.7 | 2/sec | Yes |
| `samosa-counter` | Samosa Counter | Add-ons | PassiveFlat | 500 | 1.72 | 5/sec | Yes |
| `bun-maska-tray` | Bun Maska Tray | Add-ons | PassiveFlat | 1,250 | 1.75 | 12/sec | Yes |
| `kulhad-stack` | Kulhad Stack | Add-ons | PassiveFlat | 3,500 | 1.78 | 28/sec | Yes |
| `painted-signboard` | Painted Signboard | Brand | GlobalMultiplier | 1,000 | 1.85 | +10% all | No |
| `influencer-reel` | Influencer Reel | Brand | GlobalMultiplier | 5,000 | 1.9 | +35% all | No |
| `delivery-partner` | Delivery Partner | Expansion | PassiveFlat | 25,000 | 1.9 | 80/sec | Yes |
| `franchise-kit` | Franchise Kit | Expansion | PassiveFlat | 250,000 | 1.95 | 600/sec | Yes |
| `tea-estate-contract` | Tea Estate Contract | Supply | PassiveFlat | 2,000,000 | 2.0 | 3,000/sec | Yes |
| `export-counter` | Export Counter | Late Game | PassiveFlat | 20,000,000 | 2.05 | 25,000/sec | Yes |

## Current Location Balance Table

| ID | Display name | Unlock cost | Demand multiplier | Default |
| --- | --- | ---: | ---: | --- |
| `gali-tapri` | Gali Tapri | 0 | 1 | Yes |
| `bus-stand` | Bus Stand | 250 | 1.25 | No |
| `railway-platform` | Railway Platform | 2,000 | 1.65 | No |
| `college-canteen` | College Canteen | 12,000 | 2 | No |
| `it-park` | IT Park | 80,000 | 2.75 | No |
| `highway-dhaba` | Highway Dhaba | 500,000 | 3.7 | No |
| `mall-kiosk` | Mall Kiosk | 5,000,000 | 5.2 | No |
| `airport-lounge` | Airport Lounge | 100,000,000 | 8 | No |

## Early Game Reference Calculations

These are useful when checking that the first few minutes feel right.

### Fresh New Game

```text
Rupees = 0
Tap value = 1
Passive per second = 0
Demand multiplier = 1
Global multiplier = 1
Legacy multiplier = 1
```

### After 10 Kettle Taps

```text
Rupees = 10
Chai served = 10
Player can buy Strong Tea Leaves
```

### After Buying Strong Tea Leaves Level 1

```text
Rupees = 0 if bought exactly at 10
Tap value = 2
```

### Test Case Economy Example

The edit-mode test gives this setup:

```text
Start with 600 rupees.
Buy Helper Boy level 1: -50
Buy Bulk Kettle level 1: -180
Unlock Bus Stand: -250
Remaining rupees = 120
Passive base = 0.5 + 2 = 2.5
Demand multiplier = 1.25
Passive per second = 3.125
10 seconds earns 31.25
```

## Tuning Targets

These are design targets. The first upgrade and first automation windows are enforced by the edit-mode early balance regression.

| Time window | Desired player state |
| --- | --- |
| 0-30 seconds | Player understands tap kettle and rupees. |
| 1 minute | Player buys first tap upgrade. |
| 3-5 minutes | Player unlocks first automation. |
| 10-15 minutes | Player chooses between more automation and first location. |
| 30 minutes | Player has at least one location and several automation levels. |
| First day | Player sees medium-term goals like railway platform, college canteen, and brand upgrades. |
| Multi-day | Player approaches late expansion and prestige preview. |

Current first-five-minute balance baseline:

```text
Playtest model: one kettle tap every 6 seconds.
Strong Tea Leaves: bought by 60 seconds.
Helper Boy: bought around 210 seconds.
Passive after Helper Boy: 0.5 rupees/sec before multipliers.
```

This keeps the first minute active and understandable while letting the first idle-production beat land inside the 3-5 minute target window.

## Balance Spreadsheet Guidance

Recommended columns:

| Column | Meaning |
| --- | --- |
| Upgrade ID | Stable ID used in code. |
| Kind | `TapFlat`, `TapMultiplier`, `PassiveFlat`, or `GlobalMultiplier`. |
| Base cost | Cost at level 0. |
| Cost multiplier | Exponential scale per level. |
| Level | Simulated level. |
| Current cost | `baseCost * costMultiplier ^ level`. |
| Effect per level | Flat or multiplier value. |
| Total effect | `effectPerLevel * level`. |
| Current demand multiplier | Highest unlocked location. |
| Global multiplier | Brand multiplier. |
| Passive/sec | Passive formula output. |
| Tap value | Tap formula output. |
| Time to afford next | `nextCost / incomePerSecond`. |

Balancing tips:

- Do not tune only by formula. Play the first 5 minutes repeatedly.
- Avoid adding too many multipliers early; small multiplier changes compound quickly.
- Keep manual tapping relevant in the first session, but let automation take over.
- Location costs should create anticipation, not dead time.
- Rush Hour should feel strong but not mandatory.
- Offline rewards should feel generous, but capped.

## Warning Signs

| Symptom | Likely cause | Fix |
| --- | --- | --- |
| Player waits too long before first upgrade | `strong-tea` too expensive or tap value too low | Lower first cost or add tutorial reward. |
| Manual tapping never matters after automation | Passive too high too early | Lower early passive values or make Rush favor tapping. |
| Player cannot reach next location | Location cost too high relative to passive/sec | Reduce location cost or add mid-tier passive upgrade. |
| Numbers explode before content is unlocked | Multipliers stack too aggressively | Lower global multipliers or cost scaling. |
| Offline rewards replace playing | Cap too high or efficiency too high | Lower cap/efficiency or add active-only bonuses. |
