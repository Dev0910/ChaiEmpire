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
- [x] Add button press sounds.
- [x] Add purchase/unlock sounds.
- [x] Add optional haptics.
- [x] Review and document all imported or generated assets.
- [x] Run visual/play checks, inspect logs, update docs, commit, and push.

## Phase 3: Data-Driven Content

- [x] Move upgrades and locations to ScriptableObjects or external data.
- [x] Add editor validation for duplicate IDs and invalid values.
- [x] Add content export/import workflow if using spreadsheets.
- [x] Keep deterministic default content for tests.
- [x] Run content/economy tests, update docs, commit, and push.

## Phase 4: Prestige And Skill Tree

- [x] Add prestige confirmation UI.
- [x] Add reset logic.
- [x] Add skill definitions.
- [x] Add skill tree UI.
- [x] Apply skill effects to formulas.
- [x] Add prestige tests.
- [x] Add save migration if needed.
- [x] Run tests/play checks, inspect logs, update docs, commit, and push.

## Phase 5: Events And Live Content

- [x] Add event state.
- [x] Add event multipliers or temporary upgrades.
- [x] Add event UI panel.
- [x] Add event save fields.
- [x] Add event tests.
- [x] Run tests/play checks, inspect logs, update docs, commit, and push.

## Phase 6: Monetization-Safe Options

- [x] Add optional rewarded ad for temporary 2x offline claim.
- [x] Add optional rewarded ad for short production boost.
- [x] Add cosmetic stall themes.
- [x] Add optional no-ads purchase.
- [x] Add cosmetic cup/signboard packs.
- [x] Verify a non-paying player can enjoy the full core loop.
- [x] Run tests/play checks, inspect logs, update docs, commit, and push.

## Phase 7: Cloud And Production Readiness

- [x] Add cloud save or account sync if desired.
  - [x] Added local cloud-save export/import payloads; real provider integration is not desired until an account backend is selected.
- [x] Add Play Games achievements.
  - [x] Added stable local achievement definitions and unlock state; Play Console ID mapping remains an external SDK step.
- [x] Add analytics for progression tuning.
  - [x] Added consent-gated local analytics event queue for progression actions.
- [x] Add crash reporting.
  - [x] Added consent-gated local crash-report state; real crash SDK remains an external integration step.
- [x] Add privacy policy link.
  - [x] Added Privacy & Services panel and documented placeholder URL replacement requirement.
- [x] Add consent handling if analytics or ads are used.
  - [x] Added analytics, ads, and crash-report consent toggles.
- [x] Add Play Integrity only if needed.
  - [x] Not needed for the local-only prototype because there is no backend, competitive surface, or real-money validation path yet.
- [x] Verify release build readiness, compliance, and save-loss risk.
  - [x] Verified EditMode tests, UI smoke, final compile, and Android APK build; documented remaining external production prerequisites.
- [x] Run tests/build checks, inspect logs, update docs, commit, and push.
  - [x] Implementation commit pushed as `d1c3c26`.

## Future Expansion Ideas

These ideas are not planned for the current roadmap pass; keep them as future scope seeds rather than active tasks.

- [x] Dabbawala Network Mode. Not planned for MVP; future expansion.
- [x] Festival Bazaar Event. Not planned for MVP; future expansion.
- [x] Spice Route Tycoon Layer. Not planned for MVP; future expansion.
- [x] Handloom Collaboration Cosmetics. Not planned for MVP; future expansion.
- [x] Gully Cricket Event. Not planned for MVP; future expansion.
