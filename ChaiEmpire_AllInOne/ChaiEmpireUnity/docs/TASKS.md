# Chai Empire Task Tracker

This tracker is the source of truth for roadmap completion. Do not mark a task complete until implementation, tests, Unity logs, play checks when needed, documentation, commit, and push are complete.

## Phase 1: Stabilize MVP

- [x] Save robustness
  - [x] Add malformed save recovery.
  - [x] Add save/load safety tests.
  - [x] Document recovery behavior and migration notes.
  - [x] Run targeted EditMode tests.
  - [x] Inspect Unity logs.
  - [x] Commit and push.
- [x] Tutorial
  - [x] Add a short tutorial overlay or guided first taps.
  - [x] Verify a new player can identify the first action quickly.
  - [x] Add or update relevant tests.
  - [x] Run play check and inspect logs.
  - [x] Update docs, commit, and push.
- [x] Early balance pass
  - [x] Add reset-save option in settings.
  - [x] Add number formatter tests.
  - [x] Add content validation tests.
  - [x] Improve early balance based on first 5-minute playtest targets.
  - [x] Make offline reward more visible, ideally in a modal.
  - [x] Run tests/play checks and inspect logs.
  - [x] Update docs, commit, and push.

## Phase 2: Visual And Audio Identity

- [x] Add kettle/stove art.
- [x] Add steam animation.
- [x] Add customer queue visuals.
- [x] Add UPI QR prop.
- [x] Add location-specific backgrounds.
- [ ] Add button press sounds.
- [ ] Add purchase/unlock sounds.
- [ ] Add optional haptics.
- [ ] Review and document all imported or generated assets.
- [ ] Run visual/play checks, inspect logs, update docs, commit, and push.

## Phase 3: Data-Driven Content

- [ ] Move upgrades and locations to ScriptableObjects or external data.
- [ ] Add editor validation for duplicate IDs and invalid values.
- [ ] Add content export/import workflow if using spreadsheets.
- [ ] Keep deterministic default content for tests.
- [ ] Run content/economy tests, update docs, commit, and push.

## Phase 4: Prestige And Skill Tree

- [ ] Add prestige confirmation UI.
- [ ] Add reset logic.
- [ ] Add skill definitions.
- [ ] Add skill tree UI.
- [ ] Apply skill effects to formulas.
- [ ] Add prestige tests.
- [ ] Add save migration if needed.
- [ ] Run tests/play checks, inspect logs, update docs, commit, and push.

## Phase 5: Events And Live Content

- [ ] Add event state.
- [ ] Add event multipliers or temporary upgrades.
- [ ] Add event UI panel.
- [ ] Add event save fields.
- [ ] Add event tests.
- [ ] Run tests/play checks, inspect logs, update docs, commit, and push.

## Phase 6: Monetization-Safe Options

- [ ] Add optional rewarded ad for temporary 2x offline claim.
- [ ] Add optional rewarded ad for short production boost.
- [ ] Add cosmetic stall themes.
- [ ] Add optional no-ads purchase.
- [ ] Add cosmetic cup/signboard packs.
- [ ] Verify a non-paying player can enjoy the full core loop.
- [ ] Run tests/play checks, inspect logs, update docs, commit, and push.

## Phase 7: Cloud And Production Readiness

- [ ] Add cloud save or account sync if desired.
- [ ] Add Play Games achievements.
- [ ] Add analytics for progression tuning.
- [ ] Add crash reporting.
- [ ] Add privacy policy link.
- [ ] Add consent handling if analytics or ads are used.
- [ ] Add Play Integrity only if needed.
- [ ] Verify release build readiness, compliance, and save-loss risk.
- [ ] Run tests/build checks, inspect logs, update docs, commit, and push.

## Future Expansion Ideas

- [ ] Dabbawala Network Mode.
- [ ] Festival Bazaar Event.
- [ ] Spice Route Tycoon Layer.
- [ ] Handloom Collaboration Cosmetics.
- [ ] Gully Cricket Event.
