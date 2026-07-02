# Production Readiness

This document records the Phase 7 release-readiness work and the remaining external prerequisites for a real Play Store launch.

## Current Implementation

Implemented locally:

- Manual cloud-save export/import payloads through `ChaiGame.ExportCloudSavePayload()` and `ChaiGame.TryImportCloudSavePayload(payload)`.
- Local Play Games achievement definitions in `ChaiProductionServices.Achievements`.
- Local achievement unlock state for first upgrade, Bus Stand, first event, Secret Masala, and no-ads ownership.
- Consent-gated local analytics queue in `ProductionState.AnalyticsEvents`.
- Consent-gated local crash-report message capture in `ProductionState.LastCrashReport`.
- Privacy policy URL entrypoint through `ChaiProductionServices.PrivacyPolicyUrl`.
- Runtime Privacy & Services panel with privacy, analytics, ads, crash-report, and cloud-export controls.

Not installed yet:

- Real cloud save or account provider.
- Google Play Games Services SDK.
- Analytics SDK.
- Crash-reporting SDK.
- Ad SDK or billing SDK.

## Privacy And Consent

All new production-service state is local and opt-in.

| Area | Current behavior |
| --- | --- |
| Privacy policy | `Privacy Policy` opens `https://example.com/chai-empire/privacy` and marks the policy acknowledged. |
| Analytics | Events are written only when analytics consent is on. |
| Ads | Ads consent is stored for future ad SDK adapters. |
| Crash reporting | Latest crash message is stored only when crash-report consent is on. |
| Cloud save | Export/import uses the normal JSON save payload and does not send data over the network. |

Before public release, replace the placeholder privacy URL with a real hosted policy that describes saves, optional ads, analytics, crash reporting, and purchases.

## Play Games Achievements

Current local IDs:

| ID | Display name | Unlock condition |
| --- | --- | --- |
| `first-upgrade` | First Upgrade | Buy any upgrade. |
| `bus-stand-open` | Bus Stand Open | Unlock Bus Stand. |
| `first-event` | Event Starter | Complete one optional event. |
| `secret-masala` | Secret Masala | Preserve Secret Masala at least once. |
| `no-ads-owned` | Peaceful Stall | Own the optional no-ads entitlement. |

When Play Games Services is added, map these stable local IDs to Play Console achievement IDs and submit unlocks through a consent/account-aware adapter.

## Play Integrity Decision

Play Integrity is not required for the current local-only prototype because there is no backend, competitive leaderboard, server-authoritative economy, or real-money validation path in code.

Revisit Play Integrity when adding:

- server-side cloud saves,
- account-linked economy recovery,
- real billing validation,
- competitive rankings,
- fraud-sensitive rewards.

## Save-Loss Risk

Current mitigations:

- Local JSON save/load with corrupt-save backup.
- Offline reward load safety.
- Manual cloud-save export/import payloads using the same tested codec.
- Production fields default safely when older saves do not contain them.

Remaining risk:

- No automatic remote backup until a real cloud/account provider is integrated.
- Manual export relies on the player copying/storing the payload.

## Verification Log

Phase 7 verification status:

```text
EditMode tests: passed, 33/33
Privacy & Services UI smoke: passed
Final compile/import: passed, UnityExitCode=0
Android build check: passed, UnityExitCode=0, APK written to C:\Git\ChaiEmpire\ChaiEmpire_AllInOne\ChaiEmpire.apk
```

Known benign batchmode log noise:

- Unity licensing handshake/access-token errors during startup.
- `Curl error 42: Callback aborted` during shutdown.
