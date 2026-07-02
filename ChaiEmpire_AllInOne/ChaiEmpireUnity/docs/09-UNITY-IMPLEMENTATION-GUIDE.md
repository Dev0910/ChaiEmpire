# Unity Implementation Guide

This guide explains how to rebuild or extend the current prototype in Unity.

## Unity Version

Current project version:

```text
Unity 6000.4.9f1
```

Project version file:

```text
ProjectSettings/ProjectVersion.txt
```

## Packages

Current `Packages/manifest.json` dependencies:

| Package | Version |
| --- | --- |
| `com.unity.2d.sprite` | `1.0.0` |
| `com.unity.test-framework` | `1.5.1` |
| `com.unity.textmeshpro` | `3.2.0-pre.14` |
| `com.unity.ugui` | `2.0.0` |

Note: Unity may resolve package versions in `packages-lock.json`.

## Folder Structure

```text
Assets/
  ChaiEmpire/
    Runtime/
      ChaiContent.cs
      ChaiGame.cs
      ChaiGameState.cs
      ChaiSaveCodec.cs
      ChaiSaveRepository.cs
      ChaiNumberFormatter.cs
      ChaiGamePresenter.cs
      ThirdParty/
        BreakInfinity/
          BigDouble.cs
          LICENSE.txt
    Editor/
      ChaiEmpireSceneBuilder.cs
      ChaiEmpireBuild.cs
    Scenes/
      ChaiEmpire.unity
    Tests/
      EditMode/
        ChaiGameEconomyTests.cs
```

## Rebuilding The Main Scene

Menu path:

```text
Chai Empire/Rebuild Main Scene
```

Batch method:

```text
ChaiEmpire.Editor.ChaiEmpireSceneBuilder.Build
```

The scene builder:

- Creates an empty scene.
- Adds `Main Camera`.
- Adds `Chai Empire App` with `ChaiGamePresenter`.
- Saves to `Assets/ChaiEmpire/Scenes/ChaiEmpire.unity`.
- Sets that scene in build settings.
- Sets product metadata and portrait orientation.

## Main Scene Requirements

The generated scene should contain:

| Object | Required component |
| --- | --- |
| `Main Camera` | `Camera`, tagged `MainCamera` |
| `Chai Empire App` | `ChaiGamePresenter` |

The canvas and UI are created at runtime, not stored in the scene.

## Android Build

Batch method:

```text
ChaiEmpire.Editor.ChaiEmpireBuild.BuildAndroid
```

Current behavior:

- Rebuilds the main scene.
- Switches build target to Android.
- Builds APK, not Android App Bundle.
- Sets application identifier to `com.taprilabs.chaiempire`.
- Writes APK to `outputs/ChaiEmpire.apk` when run from this project layout.

Current output path code:

```text
Path.Combine(Application.dataPath, "../../ChaiEmpire.apk")
```

## Runtime Implementation Steps From Scratch

1. Create a Unity 6 project.
2. Add packages for UGUI and Test Framework.
3. Add `BreakInfinity.BigDouble`.
4. Create runtime assembly definition.
5. Implement content definitions:
   - `UpgradeKind`
   - `UpgradeDefinition`
   - `LocationDefinition`
   - `ChaiContent`
6. Implement state:
   - `ChaiGameState`
   - `PrestigeState`
   - upgrade/location/skill entry types
7. Implement simulation:
   - tap value
   - passive income
   - purchases
   - location unlocks
   - rush timers
   - offline rewards
   - prestige preview
8. Implement save codec and repository.
9. Implement number formatter.
10. Implement presenter or a more polished UI layer.
11. Add edit-mode tests.
12. Add editor scene builder and Android build method.

## Runtime API Contract

Keep these public methods stable if possible:

| API | Purpose |
| --- | --- |
| `ChaiContent.CreateDefault()` | Loads the JSON content catalog with a built-in fallback. |
| `ChaiGame.NewGame(content)` | Creates a fresh game. |
| `ChaiGame.FromState(content, state)` | Wraps loaded state. |
| `ChaiGame.GetTapValue()` | Returns current tap value. |
| `ChaiGame.GetPassiveRupeesPerSecond()` | Returns current passive rate. |
| `ChaiGame.TapKettle(int taps = 1)` | Applies manual kettle taps. |
| `ChaiGame.TapCustomerQueue()` | Applies queue action. |
| `ChaiGame.Tick(double deltaSeconds)` | Advances passive income and timers. |
| `ChaiGame.TryTriggerRushHour()` | Starts Rush Hour if possible. |
| `ChaiGame.TryBuyUpgrade(string id)` | Attempts purchase. |
| `ChaiGame.TryUnlockLocation(string id)` | Attempts location unlock. |
| `ChaiGame.ApplyOfflineProgress(TimeSpan elapsed)` | Applies offline reward. |
| `ChaiGame.GetPrestigePreview()` | Returns prestige readiness. |

## Content Expansion Steps

To add an upgrade:

1. Add an entry to `Assets/ChaiEmpire/Resources/ChaiEmpire/default-content.json`.
2. Choose an existing `UpgradeKind`, or add a new one.
3. If adding a new kind, update:
   - `ChaiUpgradeData.ToDefinition()`
   - `ChaiContentValidator`
   - `ChaiGame.GetTapValue()`
   - `ChaiGame.GetPassiveRupeesPerSecond()`
   - `ChaiGamePresenter.DescribeUpgrade()`
   - tests
4. Run `Chai Empire > Content > Validate Default Catalog`.
5. Add balance notes to docs.

To add a location:

1. Add an entry to `Assets/ChaiEmpire/Resources/ChaiEmpire/default-content.json`.
2. Ensure the ID is stable.
3. Choose unlock cost and demand multiplier.
4. Verify location UI displays it.
5. Add tests if it changes progression expectations.
6. Run `Chai Empire > Content > Validate Default Catalog`.

## Data-Driven Content

Current path:

1. Store default upgrade and location definitions in `Assets/ChaiEmpire/Resources/ChaiEmpire/default-content.json`.
2. Load the JSON with `Resources.Load<TextAsset>("ChaiEmpire/default-content")`.
3. Keep `UpgradeDefinition` and `LocationDefinition` immutable.
4. Keep tests comparing the JSON catalog to a deterministic built-in content set.
5. Use editor validation tools for:
   - duplicate ID detection
   - missing display names
   - invalid cost/multiplier values
   - unreachable location costs
6. Use `Chai Empire > Content > Export Built-In Catalog JSON` to regenerate JSON from the built-in fallback when the fallback changes.

## Testing Approach

Current edit-mode tests validate:

- Manual taps and legacy multiplier.
- Automation and location passive income.
- Offline cap and reward formula.
- Save round trip.
- Prestige preview lock/unlock condition.
- JSON content parsing and validation.

Use edit-mode tests for economy because they are fast and do not require scene loading.

Use play-mode tests later for:

- UI button wiring.
- Save on pause.
- Scene boot.
- Offline reward modal.

## Build Notes

Before Android release:

- Confirm portrait orientation.
- Confirm package identifier.
- Confirm version code/version name.
- Confirm save path works on device.
- Confirm no debug-only UI.
- Confirm audio/haptics settings if added.
- Test on a low-end Android device.
