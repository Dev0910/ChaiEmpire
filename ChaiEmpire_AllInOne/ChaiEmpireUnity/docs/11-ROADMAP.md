# Roadmap

This roadmap organizes Chai Empire into practical development phases.

## Current Prototype

Already implemented:

- Unity 6 Android project.
- Portrait runtime UI.
- Manual tapping.
- Queue serving.
- Upgrade purchases.
- Passive automation.
- Location unlocks.
- Rush Hour.
- Local JSON save.
- Offline rewards.
- Big-number support.
- Prestige preview.
- Prestige reset and skill tree.
- Optional live events.
- Optional monetization-safe rewards and cosmetics.
- Local production-readiness services for privacy, consent, achievements, analytics, crash state, and cloud-save payloads.
- Edit-mode tests.
- Android APK build method.

## Phase 1: Stabilize MVP

Goal:

- Turn the current prototype into a stable, understandable first playable.

Work:

- Add a short tutorial overlay or guided first taps.
- Add a reset-save option in settings.
- Add malformed save recovery.
- Add number formatter tests.
- Add content validation tests.
- Improve early balance based on first 5-minute playtests.
- Make offline reward more visible, ideally in a modal.

Acceptance:

- A new player understands the first action in under 10 seconds.
- Save/load survives force close.
- First automation arrives quickly enough in playtests.

## Phase 2: Visual And Audio Identity

Goal:

- Make the game feel like a chai stall, not just a spreadsheet.

Work:

- Add kettle/stove art.
- Add steam animation.
- Add customer queue visuals.
- Add UPI QR prop.
- Add location-specific backgrounds.
- Add button press sounds.
- Add purchase/unlock sounds.
- Add optional haptics.

Acceptance:

- First screen visually communicates chai.
- Tap action has tactile feedback.
- New locations feel different.

## Phase 3: Data-Driven Content

Goal:

- Make content easier to tune without editing core code.

Work:

- Move upgrades and locations to ScriptableObjects or external data.
- Add editor validation for duplicate IDs and invalid values.
- Add content export/import workflow if using spreadsheets.
- Keep `ChaiContent.CreateDefault()` or equivalent deterministic test content.

Acceptance:

- Designer can adjust upgrade costs without editing `ChaiGame`.
- Tests still pass against default content.

## Phase 4: Prestige And Skill Tree

Goal:

- Implement Secret Masala as the long-term progression layer.

Work:

- Add prestige confirmation UI.
- Add reset logic.
- Add skill definitions.
- Add skill tree UI.
- Apply skill effects to formulas.
- Add prestige tests.
- Add save migration if needed.

Acceptance:

- Prestige unlocks only after airport/lounge and 1B lifetime rupees.
- Reset preserves Masala Legacy and skills.
- First post-prestige run is clearly faster.

## Phase 5: Events And Live Content

Goal:

- Add recurring reasons to return without making the game stressful.

Possible events:

- Monsoon Chai Rush.
- Diwali Sweet Combo.
- Cricket Match Night.
- Exam Season.
- Office Deadline Week.
- Winter Morning Chai.

Work:

- Add event state.
- Add event multipliers or temporary upgrades.
- Add event UI panel.
- Add event save fields.
- Add event tests.

Acceptance:

- Events are optional bonuses.
- Events do not break base economy.
- Event timers are clear.

## Phase 6: Monetization-Safe Options

Goal:

- Add optional monetization without damaging trust.

Allowed:

- Rewarded ad for temporary 2x offline claim.
- Rewarded ad for short production boost.
- Cosmetic stall themes.
- Optional no-ads purchase.
- Cosmetic cup/signboard packs.

Avoid:

- Forced ads after taps.
- Paywalls for core progression.
- Random paid gacha for power.
- Punishing players who do not watch ads.

Acceptance:

- A non-paying player can enjoy the full core loop.
- Ads are opt-in and clearly rewarded.

## Phase 7: Cloud And Production Readiness

Goal:

- Prepare for real release.

Work:

- Add cloud save or account sync if desired.
- Add Play Games achievements.
- Add analytics for progression tuning.
- Add crash reporting.
- Add privacy policy link.
- Add consent handling if analytics/ads are used.
- Add Play Integrity only if needed.

Acceptance:

- Release build can be tested through internal Play track.
- Data collection is compliant and minimal.
- Save loss risk is reduced.

## Future Expansion Ideas

### Dabbawala Network Mode

Manual tiffin sorting evolves into automated delivery routes and city logistics.

### Festival Bazaar Event

Temporary multi-stall market with lights, sweets, decorations, and crowd waves.

### Spice Route Tycoon Layer

Ingredient sourcing expands to farms, ports, masala blends, and export contracts.

### Handloom Collaboration Cosmetics

Regional stall skins and fabric awnings inspired by Indian textile traditions.

### Gully Cricket Event

Cricket match nights create timed demand spikes and snack combos.

## Development Priorities

Recommended next order:

1. Save robustness.
2. Tutorial.
3. Early balance pass.
4. Visual identity.
5. Data-driven content.
6. Prestige.
7. Events.
8. Monetization.
9. Cloud/analytics.

Reasoning:

- Save robustness protects player trust.
- Tutorial and balance improve retention before adding more content.
- Visual identity makes the game marketable.
- Data-driven content makes later balancing faster.
- Prestige should be added after the base loop is satisfying.
