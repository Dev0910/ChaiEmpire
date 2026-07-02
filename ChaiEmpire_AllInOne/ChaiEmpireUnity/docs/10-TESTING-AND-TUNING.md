# Testing And Tuning

This document defines current tests and recommended QA/tuning scenarios.

## Current Automated Tests

Path:

```text
Assets/ChaiEmpire/Tests/EditMode/ChaiGameEconomyTests.cs
```

Current test cases:

| Test | Verifies |
| --- | --- |
| `Manual_taps_scale_with_brew_upgrades_and_legacy_bonus` | Tapping, first tap upgrade, and Masala Legacy multiplier. |
| `Automation_and_location_unlocks_create_meaningful_passive_income` | Passive upgrades, Bus Stand unlock, and passive income formula. |
| `Offline_progress_is_capped_and_uses_current_passive_rate` | 8-hour offline cap and 0.75 offline efficiency. |
| `Early_balance_reaches_first_upgrade_and_first_automation_in_target_window` | First upgrade by 1 minute and Helper Boy inside the first 3-5 minutes for a casual tap cadence. |
| `Number_formatter_outputs_readable_rupees_suffixes_rates_and_large_fallback` | Compact rupee display, negative suffixes, per-second labels, and high exponent fallback. |
| `Default_upgrade_catalog_has_unique_valid_progression_values` | Unique upgrade IDs, positive costs/effects, scaling multipliers, and automation flags. |
| `Default_location_catalog_has_one_start_and_ordered_unlock_progression` | One default location, valid unlock costs, and ordered demand progression. |
| `Save_round_trip_preserves_v1_and_future_prestige_fields` | JSON save/load for currency, upgrades, locations, and prestige fields. |
| `Save_round_trip_preserves_high_exponent_big_numbers` | Save/load keeps large `BigDouble` mantissa and exponent values. |
| `Repository_load_backs_up_malformed_save_and_starts_new_game` | Malformed JSON recovery and corrupt-save backup. |
| `Repository_load_backs_up_invalid_number_save_and_starts_new_game` | Invalid saved numbers recover safely. |
| `Repository_load_ignores_invalid_saved_ticks_without_resetting_valid_state` | Invalid timestamps skip offline reward without wiping valid state. |
| `Repository_delete_save_removes_existing_file_and_tolerates_missing_file` | Settings reset can delete the local save safely. |
| Tutorial prompt tests | First-tap, save-for-upgrade, buy-upgrade, and completion states. |
| `Prestige_preview_stays_locked_until_first_empire_arc_is_complete` | Prestige preview condition and projected Masala Legacy formula. |

## Running Edit-Mode Tests

Unity batch command pattern:

```text
Unity.exe -batchmode -projectPath <project> -runTests -testPlatform editmode -testResults <results.xml> -logFile <log.txt>
```

Tests should produce:

```text
total="18" passed="18" failed="0"
```

## First 5 Minutes Test

Purpose:

- Validate onboarding and early progression.

Checklist:

| Check | Expected |
| --- | --- |
| Start state | 0 rupees, Gali Tapri, tap value 1. |
| First tap | Rupees increase immediately. |
| First 10 taps | Player can buy Strong Tea Leaves. |
| First upgrade | Tap value increases from 1 to 2. |
| Early automation | Helper Boy is bought inside the 3-5 minute target window in the automated casual-tap simulation. |
| UI readability | Rupees, tap value, production, and upgrade costs are readable. |

Pass criteria:

- Player understands what to tap without instruction.
- Player buys at least one upgrade quickly.
- Player sees automation goal before boredom.

## First 30 Minutes Test

Purpose:

- Validate early-mid economy choice.

Checklist:

| Check | Expected |
| --- | --- |
| Automation | Player has at least Helper Boy or Bulk Kettle. |
| Choice | Player can choose between tap upgrade, automation, or Bus Stand. |
| Location | Bus Stand should feel reachable. |
| Rush Hour | Player understands when rush is active and when cooldown remains. |
| Passive income | Production per second visibly changes after purchases. |
| Save | Closing and reopening keeps state. |

Pass criteria:

- Player has at least two meaningful spending choices.
- Waiting time for next goal is not excessive.
- Automation begins to carry the economy.

## Offline Tests

Manual test scenarios:

| Scenario | Expected |
| --- | --- |
| Close with no passive income | No meaningful offline reward. |
| Close for 10 minutes with passive income | Reward equals passive/sec * 600 * 0.75. |
| Close for 1 hour with passive income | Reward equals passive/sec * 3600 * 0.75. |
| Close for 12 hours with passive income | Reward caps at 8 hours. |
| Close during Rush Hour | Offline reward excludes rush multiplier. |
| Device time moved backward | Reward should not become negative. |

Automated coverage currently exists for the 12-hour cap case.

## Economy Simulation Checks

Create a spreadsheet or script that simulates:

- Taps per minute.
- Passive income per second.
- Upgrade purchases by best payback.
- Location unlock timing.
- Offline rewards at 10 minutes, 1 hour, 8 hours.
- Prestige preview timing.

Important outputs:

| Metric | Reason |
| --- | --- |
| Time to first upgrade | First dopamine beat. |
| Time to first automation | Core idle transition. |
| Time to Bus Stand | First expansion milestone. |
| Time to Railway Platform | First major demand jump. |
| Time to Airport Lounge | Prestige readiness pacing. |
| Income share active vs passive | Ensures active play and idle play both matter. |

## Regression Checklist

Run this after any economy change:

- Existing edit-mode tests pass.
- New upgrade cost is positive.
- New cost multiplier is greater than 1 unless intentionally capped.
- New passive values do not dwarf all previous content immediately.
- New IDs are unique.
- Existing save files still load.
- Default `gali-tapri` remains unlocked.
- Offline reward remains capped.
- Rush cannot be triggered during cooldown.
- Prestige preview remains locked until airport/lounge and 1B lifetime rupees.

## UI Regression Checklist

Run this after UI changes:

- Portrait orientation still applies.
- Main actions are large enough.
- Long upgrade names do not overflow.
- Disabled buttons look disabled.
- Rush status changes correctly.
- Offline reward modal appears on meaningful return and dismisses with Claim.
- Settings reset requires a confirmation tap and returns to a fresh save.
- Save still occurs on pause.
- Scroll view works on smaller screens.

## Phase 2 Visual Identity Verification

Latest Phase 2 verification:

```text
EditMode: passed=18, failed=0, skipped=0
Visual identity smoke: passed
Final compile/import: UnityExitCode=0
```

The visual identity smoke verifies:

- Kettle/stove art objects exist.
- Steam wisps animate upward.
- Customer queue and UPI QR prop objects exist.
- Button, purchase, and unlock audio clips are generated.
- Haptics toggle switches on and off.
- Stall backdrop changes after a location unlock.

Known log noise:

- Unity licensing handshake/access-token errors in batchmode.
- `Curl error 42` during shutdown.

## Android Device Checklist

Test on real device before release:

- Install APK.
- Launch fresh.
- Tap and buy first upgrade.
- Background app, return within 30 seconds.
- Background app for more than 30 seconds.
- Force close and reopen.
- Rotate device and confirm portrait behavior.
- Check save persists after app restart.
- Check performance with many upgrade levels.
- Check readable UI on low resolution.

## Tuning Workflow

Recommended loop:

1. Pick one target, such as time to first automation.
2. Change one variable.
3. Run automated tests.
4. Simulate first 30 minutes.
5. Play manually.
6. Record before/after values.
7. Only then tune the next variable.

Avoid:

- Changing several multipliers at once.
- Balancing only from late-game numbers.
- Ignoring manual feel.
- Letting offline rewards dominate progression.

## Future Tests To Add

| Future test | Why |
| --- | --- |
| Rush cooldown tests | Lock burst behavior. |
| Prestige reset tests | Required when prestige is implemented. |
| Skill tree tests | Required when skills affect formulas. |
| Presenter play-mode smoke test | Validate scene boots and buttons exist. |
