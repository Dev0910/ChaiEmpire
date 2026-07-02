# Save Data And Offline Progress

This document describes the current save system and offline reward logic.

## Save Location

Current save path:

```text
Application.persistentDataPath/chai-empire-save.json
```

The file name is defined in `ChaiSaveRepository`:

```text
chai-empire-save.json
```

## Save Load Flow

`ChaiSaveRepository.LoadOrCreate(content)`:

1. Resolves the save path.
2. If the file exists, reads JSON and calls `ChaiSaveCodec.TryFromJson(json, out state)`.
3. If the file does not exist, creates `ChaiGameState.CreateNew()`.
4. If an existing save cannot be read or parsed, moves it to a timestamped `.corrupt-*.bak` file beside the save and creates a new game state.
5. Checks `LastSavedUtcTicks`.
6. Ignores invalid UTC ticks instead of attempting offline reward calculation.
7. If elapsed time is greater than 30 seconds, applies offline progress.
8. Updates `LastSavedUtcTicks` to current UTC ticks.
9. Returns `LoadResult`.

`LoadResult` reports recovery metadata:

| Field | Type | Meaning |
| --- | --- | --- |
| `RecoveredFromCorruptSave` | `bool` | True when an existing save could not be safely loaded and a new state was created. |
| `CorruptSaveBackupPath` | `string` | Path to the moved corrupt-save backup, or null if backup could not be created. |

## Save Write Flow

`ChaiSaveRepository.Save(state)`:

1. Resolves the save path.
2. Ensures the save directory exists.
3. Sets `LastSavedUtcTicks` to current UTC ticks.
4. Writes `ChaiSaveCodec.ToJson(state)` to disk.

Current save triggers:

| Trigger | Source |
| --- | --- |
| Every 10 seconds | `ChaiGamePresenter.Update()` |
| App pause | `ChaiGamePresenter.OnApplicationPause(true)` |
| App quit | `ChaiGamePresenter.OnApplicationQuit()` |

## Save Data Model

Current runtime state fields:

| Field | Type | Meaning |
| --- | --- | --- |
| `SaveVersion` | `int` | Save schema version, currently `1`. |
| `Rupees` | `BigDouble` | Current spendable currency. |
| `TotalLifetimeRupees` | `BigDouble` | Total earned rupees used for prestige preview. |
| `ChaiServed` | `BigDouble` | Count of manually served chai units. |
| `LastSavedUtcTicks` | `long` | UTC timestamp ticks for offline calculation. |
| `RushRemainingSeconds` | `double` | Current active rush time. |
| `RushCooldownSeconds` | `double` | Remaining rush cooldown. |
| `Prestige` | `PrestigeState` | Future prestige state. |
| `UpgradeLevels` | `List<UpgradeLevelEntry>` | Upgrade IDs and levels. |
| `UnlockedLocations` | `List<LocationUnlockEntry>` | Location IDs that are unlocked. |

## JSON DTO Shape

`ChaiSaveCodec` uses private DTO classes with lowercase JSON-style field names.

Expected JSON fields:

| JSON field | Meaning |
| --- | --- |
| `saveVersion` | Save schema version. |
| `rupees` | String representation of `BigDouble`. |
| `totalLifetimeRupees` | String representation of `BigDouble`. |
| `chaiServed` | String representation of `BigDouble`. |
| `lastSavedUtcTicks` | UTC ticks. |
| `rushRemainingSeconds` | Active rush seconds. |
| `rushCooldownSeconds` | Rush cooldown seconds. |
| `prestige` | Prestige DTO. |
| `upgradeLevels` | List of upgrade level DTOs. |
| `unlockedLocations` | List of location unlock DTOs. |

Prestige DTO:

| JSON field | Meaning |
| --- | --- |
| `masalaLegacy` | String representation of `BigDouble`. |
| `unspentSkillPoints` | Future skill tree points. |
| `skills` | List of skill ID/level pairs. |

Upgrade level DTO:

| JSON field | Meaning |
| --- | --- |
| `id` | Stable upgrade ID. |
| `level` | Upgrade level. |

Location unlock DTO:

| JSON field | Meaning |
| --- | --- |
| `id` | Stable location ID. |

Skill DTO:

| JSON field | Meaning |
| --- | --- |
| `id` | Stable skill ID. |
| `level` | Skill level. |

## BigDouble Serialization

`BigDouble` values are serialized as strings using:

```text
value.ToString("G")
```

They are loaded with:

```text
BigDouble.Parse(value with E/e normalized)
```

Blank or missing number strings load as `BigDouble.Zero`.
Invalid, NaN, or infinity number strings make the save recover as corrupt.

## New Game Defaults

`ChaiGameState.CreateNew()` sets:

| Field | Default |
| --- | --- |
| `SaveVersion` | `1` |
| `Rupees` | `0` |
| `TotalLifetimeRupees` | `0` |
| `ChaiServed` | `0` |
| `LastSavedUtcTicks` | `DateTime.UtcNow.Ticks` |
| `Prestige` | New `PrestigeState` |
| Default location | `gali-tapri` unlocked |

## Load Safety Behavior

Current load behavior:

- Null, empty, or whitespace JSON returns a new game state.
- Missing or invalid version becomes version `1`.
- Malformed JSON or invalid persisted numbers return a new game state.
- `ChaiSaveRepository` moves a corrupt existing save to `chai-empire-save.json.corrupt-<utc>.bak` before returning the new state.
- Invalid `LastSavedUtcTicks` values skip offline reward calculation and are replaced with the current UTC ticks.
- Negative rush timers are clamped to zero.
- `gali-tapri` is always unlocked after load.
- Upgrade levels are clamped to at least zero through `SetUpgradeLevel`.

## Offline Progress Formula

Current formula:

```text
rawSeconds = max(0, elapsed.TotalSeconds)
cappedSeconds = min(rawSeconds, 28800)
offlineReward = passivePerSecondWithoutRush * cappedSeconds * 0.75
```

Current constants:

| Constant | Value |
| --- | --- |
| Minimum elapsed for reward application | Greater than 30 seconds |
| Offline cap | 28800 seconds |
| Offline cap in hours | 8 hours |
| Offline efficiency | 0.75 |
| Rush included | No |

`passivePerSecondWithoutRush` means `ChaiGame.GetPassiveRupeesPerSecond(includeRush: false)`.

## Offline Reward Edge Cases

| Edge case | Current behavior | Future recommendation |
| --- | --- | --- |
| No save file | New state, no reward. | Keep. |
| Elapsed <= 30 seconds | No displayed offline reward. | Keep or make threshold configurable. |
| Elapsed > 8 hours | Reward capped at 8 hours. | Keep for v1. |
| Device clock moved backward | `rawSeconds` becomes 0 due to max clamp. | Add suspicious clock metric later. |
| Save has no passive upgrades | Reward is zero. | UI should avoid showing empty reward. |
| Rush active when closing app | Rush is not included offline. | Keep. |
| Malformed save JSON | Broken save is moved to a timestamped `.corrupt-*.bak`; player starts from a fresh state. | Consider an in-game recovery notice later. |

## Save Versioning Strategy

Current version: `1`.

Future rule:

- Increment `SaveVersion` when changing persisted field meaning or removing fields.
- Do not increment for adding optional fields that default safely.
- Keep old field names readable when possible.
- Add migration functions in `ChaiSaveCodec.FromJson`.

Suggested migration flow:

```text
Read DTO
If version <= 0, treat as version 1
If version == 1, migrate to current
If version is newer than app supports, load cautiously or show warning
Ensure required defaults
Return ChaiGameState
```

## Future Anti-Cheat Notes

Current implementation trusts device time. That is acceptable for a local prototype but weak for production.

Future options:

| Method | Benefit | Cost |
| --- | --- | --- |
| Server timestamp | Stronger offline reward validation. | Requires backend. |
| Play Integrity signal | Helps detect risky device/app state. | Requires Play integration. |
| Monotonic session clock | Detects some clock manipulation during session. | Does not solve app-closed time fully. |
| Offline reward cap | Already implemented, limits abuse impact. | Does not detect cheating. |
| Save hash | Detects casual file edits. | Not secure against determined attackers. |

Recommended production stance:

- Keep the game playable offline.
- Cap offline rewards.
- Avoid competitive leaderboards unless server-authoritative.
- Treat local save security as friction, not true security.
