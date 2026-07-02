using System;
using System.Collections.Generic;
using BreakInfinity;
using UnityEngine;

namespace ChaiEmpire
{
    public static class ChaiSaveCodec
    {
        public static string ToJson(ChaiGameState state)
        {
            ChaiSaveDto dto = new ChaiSaveDto
            {
                saveVersion = state.SaveVersion,
                rupees = SerializeNumber(state.Rupees),
                totalLifetimeRupees = SerializeNumber(state.TotalLifetimeRupees),
                chaiServed = SerializeNumber(state.ChaiServed),
                lastSavedUtcTicks = state.LastSavedUtcTicks,
                rushRemainingSeconds = state.RushRemainingSeconds,
                rushCooldownSeconds = state.RushCooldownSeconds,
                eventState = new EventDto
                {
                    activeEventId = state.Event?.ActiveEventId,
                    remainingSeconds = state.Event?.RemainingSeconds ?? 0,
                    cooldownSeconds = state.Event?.CooldownSeconds ?? 0,
                    completedCount = state.Event?.CompletedCount ?? 0
                },
                monetization = new MonetizationDto
                {
                    noAdsPurchased = state.Monetization?.NoAdsPurchased ?? false,
                    productionBoostRemainingSeconds = state.Monetization?.ProductionBoostRemainingSeconds ?? 0,
                    productionBoostCooldownSeconds = state.Monetization?.ProductionBoostCooldownSeconds ?? 0,
                    rewardedOfflineBonusClaims = state.Monetization?.RewardedOfflineBonusClaims ?? 0
                },
                cosmetics = new CosmeticDto
                {
                    stallThemeId = state.Cosmetics?.StallThemeId,
                    cupPackId = state.Cosmetics?.CupPackId,
                    signboardPackId = state.Cosmetics?.SignboardPackId
                },
                production = new ProductionDto
                {
                    analyticsConsent = state.Production?.AnalyticsConsent ?? false,
                    adsConsent = state.Production?.AdsConsent ?? false,
                    crashReportingConsent = state.Production?.CrashReportingConsent ?? false,
                    privacyPolicyAcknowledged = state.Production?.PrivacyPolicyAcknowledged ?? false,
                    cloudSaveExportCount = Math.Max(0, state.Production?.CloudSaveExportCount ?? 0),
                    lastCrashReport = state.Production?.LastCrashReport,
                    achievements = new List<AchievementDto>(),
                    analyticsEvents = new List<AnalyticsEventDto>()
                },
                prestige = new PrestigeDto
                {
                    masalaLegacy = SerializeNumber(state.Prestige.MasalaLegacy),
                    unspentSkillPoints = state.Prestige.UnspentSkillPoints,
                    skills = new List<PrestigeSkillDto>()
                },
                upgradeLevels = new List<UpgradeLevelDto>(),
                unlockedLocations = new List<LocationUnlockDto>()
            };

            foreach (UpgradeLevelEntry entry in state.UpgradeLevels)
            {
                dto.upgradeLevels.Add(new UpgradeLevelDto { id = entry.Id, level = entry.Level });
            }

            foreach (LocationUnlockEntry entry in state.UnlockedLocations)
            {
                dto.unlockedLocations.Add(new LocationUnlockDto { id = entry.Id });
            }

            foreach (PrestigeSkillEntry entry in state.Prestige.Skills)
            {
                dto.prestige.skills.Add(new PrestigeSkillDto { id = entry.Id, level = entry.Level });
            }

            if (state.Production?.Achievements != null)
            {
                foreach (AchievementEntry entry in state.Production.Achievements)
                {
                    if (entry != null && !string.IsNullOrWhiteSpace(entry.Id))
                    {
                        dto.production.achievements.Add(new AchievementDto { id = entry.Id, unlockedUtcTicks = entry.UnlockedUtcTicks });
                    }
                }
            }

            if (state.Production?.AnalyticsEvents != null)
            {
                foreach (AnalyticsEventEntry entry in state.Production.AnalyticsEvents)
                {
                    if (entry != null && !string.IsNullOrWhiteSpace(entry.Name))
                    {
                        dto.production.analyticsEvents.Add(new AnalyticsEventDto { name = entry.Name, utcTicks = entry.UtcTicks, detail = entry.Detail });
                    }
                }
            }

            return JsonUtility.ToJson(dto, true);
        }

        public static ChaiGameState FromJson(string json)
        {
            TryFromJson(json, out ChaiGameState state);
            return state;
        }

        public static bool TryFromJson(string json, out ChaiGameState state)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                state = ChaiGameState.CreateNew();
                return true;
            }

            try
            {
                state = FromJsonUnchecked(json);
                return true;
            }
            catch (Exception exception) when (IsRecoverableLoadException(exception))
            {
                state = ChaiGameState.CreateNew();
                return false;
            }
        }

        private static ChaiGameState FromJsonUnchecked(string json)
        {
            if (!json.TrimStart().StartsWith("{", StringComparison.Ordinal))
            {
                throw new ArgumentException("Save JSON must be an object.");
            }

            ChaiSaveDto dto = JsonUtility.FromJson<ChaiSaveDto>(json);
            if (dto == null)
            {
                return ChaiGameState.CreateNew();
            }

            ChaiGameState state = new ChaiGameState
            {
                SaveVersion = dto.saveVersion <= 0 ? 1 : dto.saveVersion,
                Rupees = ParseNumber(dto.rupees),
                TotalLifetimeRupees = ParseNumber(dto.totalLifetimeRupees),
                ChaiServed = ParseNumber(dto.chaiServed),
                LastSavedUtcTicks = dto.lastSavedUtcTicks,
                RushRemainingSeconds = Math.Max(0, dto.rushRemainingSeconds),
                RushCooldownSeconds = Math.Max(0, dto.rushCooldownSeconds),
                Event = new EventState
                {
                    ActiveEventId = string.IsNullOrWhiteSpace(dto.eventState?.activeEventId) ? null : dto.eventState.activeEventId,
                    RemainingSeconds = Math.Max(0, dto.eventState?.remainingSeconds ?? 0),
                    CooldownSeconds = Math.Max(0, dto.eventState?.cooldownSeconds ?? 0),
                    CompletedCount = Math.Max(0, dto.eventState?.completedCount ?? 0)
                },
                Monetization = new MonetizationState
                {
                    NoAdsPurchased = dto.monetization?.noAdsPurchased ?? false,
                    ProductionBoostRemainingSeconds = Math.Max(0, dto.monetization?.productionBoostRemainingSeconds ?? 0),
                    ProductionBoostCooldownSeconds = Math.Max(0, dto.monetization?.productionBoostCooldownSeconds ?? 0),
                    RewardedOfflineBonusClaims = Math.Max(0, dto.monetization?.rewardedOfflineBonusClaims ?? 0)
                },
                Cosmetics = new CosmeticState
                {
                    StallThemeId = NormalizeCosmeticId(ChaiCosmetics.StallThemes, dto.cosmetics?.stallThemeId),
                    CupPackId = NormalizeCosmeticId(ChaiCosmetics.CupPacks, dto.cosmetics?.cupPackId),
                    SignboardPackId = NormalizeCosmeticId(ChaiCosmetics.SignboardPacks, dto.cosmetics?.signboardPackId)
                },
                Production = new ProductionState
                {
                    AnalyticsConsent = dto.production?.analyticsConsent ?? false,
                    AdsConsent = dto.production?.adsConsent ?? false,
                    CrashReportingConsent = dto.production?.crashReportingConsent ?? false,
                    PrivacyPolicyAcknowledged = dto.production?.privacyPolicyAcknowledged ?? false,
                    CloudSaveExportCount = Math.Max(0, dto.production?.cloudSaveExportCount ?? 0),
                    LastCrashReport = dto.production?.lastCrashReport,
                    Achievements = new List<AchievementEntry>(),
                    AnalyticsEvents = new List<AnalyticsEventEntry>()
                },
                Prestige = new PrestigeState
                {
                    MasalaLegacy = ParseNumber(dto.prestige?.masalaLegacy),
                    UnspentSkillPoints = dto.prestige?.unspentSkillPoints ?? 0,
                    Skills = new List<PrestigeSkillEntry>()
                },
                UpgradeLevels = new List<UpgradeLevelEntry>(),
                UnlockedLocations = new List<LocationUnlockEntry>()
            };

            if (dto.upgradeLevels != null)
            {
                foreach (UpgradeLevelDto entry in dto.upgradeLevels)
                {
                    if (entry != null && !string.IsNullOrWhiteSpace(entry.id))
                    {
                        state.SetUpgradeLevel(entry.id, entry.level);
                    }
                }
            }

            if (dto.unlockedLocations != null)
            {
                foreach (LocationUnlockDto entry in dto.unlockedLocations)
                {
                    if (entry != null && !string.IsNullOrWhiteSpace(entry.id))
                    {
                        state.UnlockLocation(entry.id);
                    }
                }
            }

            if (!state.IsLocationUnlocked("gali-tapri"))
            {
                state.UnlockLocation("gali-tapri");
            }

            if (dto.prestige?.skills != null)
            {
                foreach (PrestigeSkillDto entry in dto.prestige.skills)
                {
                    if (entry != null && !string.IsNullOrWhiteSpace(entry.id))
                    {
                        state.Prestige.Skills.Add(new PrestigeSkillEntry { Id = entry.id, Level = entry.level });
                    }
                }
            }

            if (dto.production?.achievements != null)
            {
                foreach (AchievementDto entry in dto.production.achievements)
                {
                    if (entry != null && !string.IsNullOrWhiteSpace(entry.id))
                    {
                        state.Production.Achievements.Add(new AchievementEntry { Id = entry.id, UnlockedUtcTicks = entry.unlockedUtcTicks });
                    }
                }
            }

            if (dto.production?.analyticsEvents != null)
            {
                foreach (AnalyticsEventDto entry in dto.production.analyticsEvents)
                {
                    if (entry != null && !string.IsNullOrWhiteSpace(entry.name))
                    {
                        state.Production.AnalyticsEvents.Add(new AnalyticsEventEntry { Name = entry.name, UtcTicks = entry.utcTicks, Detail = entry.detail });
                    }
                }
            }

            return state;
        }

        private static bool IsRecoverableLoadException(Exception exception)
        {
            return exception is ArgumentException ||
                exception is FormatException ||
                exception is OverflowException ||
                exception is NullReferenceException;
        }

        private static string SerializeNumber(BigDouble value)
        {
            return value.ToString("G");
        }

        private static BigDouble ParseNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return BigDouble.Zero;
            }

            try
            {
                BigDouble parsed = BigDouble.Parse(value.Trim().Replace('E', 'e'));
                if (BigDouble.IsNaN(parsed) || BigDouble.IsInfinity(parsed))
                {
                    throw new FormatException("Saved number must be finite.");
                }

                return parsed;
            }
            catch (Exception exception) when (IsRecoverableLoadException(exception))
            {
                throw new FormatException("Saved number is invalid.", exception);
            }
        }

        private static string NormalizeCosmeticId(IReadOnlyList<CosmeticDefinition> definitions, string id)
        {
            if (ChaiCosmetics.Contains(definitions, id))
            {
                return id;
            }

            return definitions.Count > 0 ? definitions[0].Id : string.Empty;
        }

        [Serializable]
        private sealed class ChaiSaveDto
        {
            public int saveVersion;
            public string rupees;
            public string totalLifetimeRupees;
            public string chaiServed;
            public long lastSavedUtcTicks;
            public double rushRemainingSeconds;
            public double rushCooldownSeconds;
            public EventDto eventState;
            public MonetizationDto monetization;
            public CosmeticDto cosmetics;
            public ProductionDto production;
            public PrestigeDto prestige;
            public List<UpgradeLevelDto> upgradeLevels;
            public List<LocationUnlockDto> unlockedLocations;
        }

        [Serializable]
        private sealed class EventDto
        {
            public string activeEventId;
            public double remainingSeconds;
            public double cooldownSeconds;
            public int completedCount;
        }

        [Serializable]
        private sealed class MonetizationDto
        {
            public bool noAdsPurchased;
            public double productionBoostRemainingSeconds;
            public double productionBoostCooldownSeconds;
            public int rewardedOfflineBonusClaims;
        }

        [Serializable]
        private sealed class CosmeticDto
        {
            public string stallThemeId;
            public string cupPackId;
            public string signboardPackId;
        }

        [Serializable]
        private sealed class ProductionDto
        {
            public bool analyticsConsent;
            public bool adsConsent;
            public bool crashReportingConsent;
            public bool privacyPolicyAcknowledged;
            public int cloudSaveExportCount;
            public string lastCrashReport;
            public List<AchievementDto> achievements;
            public List<AnalyticsEventDto> analyticsEvents;
        }

        [Serializable]
        private sealed class PrestigeDto
        {
            public string masalaLegacy;
            public int unspentSkillPoints;
            public List<PrestigeSkillDto> skills;
        }

        [Serializable]
        private sealed class UpgradeLevelDto
        {
            public string id;
            public int level;
        }

        [Serializable]
        private sealed class LocationUnlockDto
        {
            public string id;
        }

        [Serializable]
        private sealed class PrestigeSkillDto
        {
            public string id;
            public int level;
        }

        [Serializable]
        private sealed class AchievementDto
        {
            public string id;
            public long unlockedUtcTicks;
        }

        [Serializable]
        private sealed class AnalyticsEventDto
        {
            public string name;
            public long utcTicks;
            public string detail;
        }
    }
}
