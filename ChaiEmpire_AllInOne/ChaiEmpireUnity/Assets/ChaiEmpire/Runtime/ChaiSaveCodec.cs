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
    }
}
