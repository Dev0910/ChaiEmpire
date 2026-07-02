# Chai Empire Documentation Index

This documentation pack describes the current Chai Empire Unity prototype and the intended design direction for expanding it into a fuller Android incremental clicker.

The current codebase is the factual source of truth for implemented behavior. Future systems are labeled as planned or future so they are not confused with shipped mechanics.

## Current Prototype Snapshot

| Area | Current status |
| --- | --- |
| Engine | Unity `6000.4.9f1` |
| Target | Android portrait |
| Main scene | `Assets/ChaiEmpire/Scenes/ChaiEmpire.unity` |
| Main runtime assembly | `Assets/ChaiEmpire/Runtime/ChaiEmpire.Runtime.asmdef` |
| Main editor assembly | `Assets/ChaiEmpire/Editor/ChaiEmpire.Editor.asmdef` |
| UI system | Runtime-created Unity UI using `UnityEngine.UI` |
| Economy numbers | `BreakInfinity.BigDouble`, vendored under `Runtime/ThirdParty/BreakInfinity` |
| Save system | Local JSON file at `Application.persistentDataPath/chai-empire-save.json` |
| Tests | Edit-mode tests in `Assets/ChaiEmpire/Tests/EditMode` |
| Android build method | `ChaiEmpire.Editor.ChaiEmpireBuild.BuildAndroid` |
| Production readiness | Local cloud-export payloads, consent state, privacy link, achievements, analytics queue, and crash-report state |

## Reading Order

1. `01-GAME-DESIGN.md` for the product vision, audience, tone, and game fantasy.
2. `02-CORE-LOOPS.md` for how the player moves from manual action to automation.
3. `03-ECONOMY-BALANCING.md` for formulas, tables, and tuning targets.
4. `04-CONTENT-CATALOG.md` for exact current content and future content ideas.
5. `05-SYSTEM-DESIGN.md` for architecture and code responsibilities.
6. `06-SAVE-DATA-AND-OFFLINE.md` for persistence, offline progress, and migration rules.
7. `07-UI-UX-ANDROID.md` for mobile layout and interaction guidelines.
8. `08-PRESTIGE-SKILL-TREE.md` for the future Masala Legacy system.
9. `09-UNITY-IMPLEMENTATION-GUIDE.md` for rebuilding or extending in Unity.
10. `10-TESTING-AND-TUNING.md` for QA, balance validation, and regression checks.
11. `11-ROADMAP.md` for phased development after the current prototype.
12. `12-HTML-WIREFRAMES-UNITY-UI-SPEC.md` for converting the HTML wireframes into Unity UI components.
13. `13-ASSET-REVIEW.md` for generated asset inventory and review notes.
14. `14-PRODUCTION-READINESS.md` for release-readiness, compliance, cloud, Play Games, analytics, crash reporting, and Play Integrity notes.

## File Map

| Doc | Purpose |
| --- | --- |
| `01-GAME-DESIGN.md` | Defines what Chai Empire is and what it should feel like. |
| `02-CORE-LOOPS.md` | Describes moment-to-moment and long-term gameplay loops. |
| `03-ECONOMY-BALANCING.md` | Captures exact formulas and current balance constants. |
| `04-CONTENT-CATALOG.md` | Lists current upgrades, locations, descriptions, and planned content. |
| `05-SYSTEM-DESIGN.md` | Explains the Unity architecture and extension seams. |
| `06-SAVE-DATA-AND-OFFLINE.md` | Documents save JSON, offline rewards, versioning, and anti-cheat notes. |
| `07-UI-UX-ANDROID.md` | Gives Android portrait UI rules and expected screen behavior. |
| `08-PRESTIGE-SKILL-TREE.md` | Specifies the future prestige system contract. |
| `09-UNITY-IMPLEMENTATION-GUIDE.md` | Provides concrete implementation and build steps. |
| `10-TESTING-AND-TUNING.md` | Defines testing scenarios and tuning workflow. |
| `11-ROADMAP.md` | Organizes future work into shippable phases. |
| `12-HTML-WIREFRAMES-UNITY-UI-SPEC.md` | Maps the HTML wireframes to Unity Canvas hierarchy, prefabs, bindings, and modal contracts. |
| `13-ASSET-REVIEW.md` | Reviews generated visual/audio assets and import/licensing status. |
| `14-PRODUCTION-READINESS.md` | Documents Phase 7 production-readiness decisions and verification results. |

## Implemented Source Files

| File | Responsibility |
| --- | --- |
| `Assets/ChaiEmpire/Runtime/ChaiContent.cs` | Hard-coded content catalog, upgrade definitions, location definitions, offline constants, prestige unlock threshold. |
| `Assets/ChaiEmpire/Runtime/ChaiGame.cs` | Runtime economy simulation, tapping, passive income, rush hour, purchases, location unlocks, offline rewards, prestige preview. |
| `Assets/ChaiEmpire/Runtime/ChaiGameState.cs` | Serializable player state, upgrade levels, unlocked locations, prestige state fields. |
| `Assets/ChaiEmpire/Runtime/ChaiSaveCodec.cs` | JSON conversion between `ChaiGameState` and a DTO shape. |
| `Assets/ChaiEmpire/Runtime/ChaiSaveRepository.cs` | File read/write and offline reward application on load. |
| `Assets/ChaiEmpire/Runtime/ChaiNumberFormatter.cs` | Compact display formatting for rupees, rates, and large numbers. |
| `Assets/ChaiEmpire/Runtime/ChaiGamePresenter.cs` | Runtime-created UI, button handlers, refresh cadence, save cadence, app pause/quit save hooks. |
| `Assets/ChaiEmpire/Runtime/ChaiProductionServices.cs` | Privacy URL and local Play Games achievement definitions for production adapters. |
| `Assets/ChaiEmpire/Editor/ChaiEmpireSceneBuilder.cs` | Editor scene generation and portrait project settings. |
| `Assets/ChaiEmpire/Editor/ChaiEmpireBuild.cs` | Android APK build entrypoint. |
| `Assets/ChaiEmpire/Tests/EditMode/ChaiGameEconomyTests.cs` | Current edit-mode tests for economy, offline, save round trip, and prestige preview. |

## Documentation Rules

- Implemented facts should match code.
- Planned systems must be labeled as planned or future.
- Balance values in these docs should not be changed unless `ChaiContent.cs` or `ChaiGame.cs` is changed too.
- Save schema changes must be accompanied by migration notes in `06-SAVE-DATA-AND-OFFLINE.md`.
- Any future monetization should remain optional and non-hostile.
