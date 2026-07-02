using System;
using System.Collections.Generic;
using BreakInfinity;

namespace ChaiEmpire
{
    [Serializable]
    public sealed class ChaiGameState
    {
        public int SaveVersion = 1;
        public BigDouble Rupees;
        public BigDouble TotalLifetimeRupees;
        public BigDouble ChaiServed;
        public long LastSavedUtcTicks;
        public double RushRemainingSeconds;
        public double RushCooldownSeconds;
        public PrestigeState Prestige = new PrestigeState();
        public EventState Event = new EventState();
        public MonetizationState Monetization = new MonetizationState();
        public CosmeticState Cosmetics = CosmeticState.CreateDefault();
        public List<UpgradeLevelEntry> UpgradeLevels = new List<UpgradeLevelEntry>();
        public List<LocationUnlockEntry> UnlockedLocations = new List<LocationUnlockEntry>();

        public static ChaiGameState CreateNew()
        {
            ChaiGameState state = new ChaiGameState
            {
                SaveVersion = 1,
                Rupees = BigDouble.Zero,
                TotalLifetimeRupees = BigDouble.Zero,
                ChaiServed = BigDouble.Zero,
                LastSavedUtcTicks = DateTime.UtcNow.Ticks,
                Prestige = new PrestigeState(),
                Event = new EventState(),
                Monetization = new MonetizationState(),
                Cosmetics = CosmeticState.CreateDefault()
            };
            state.UnlockLocation("gali-tapri");
            return state;
        }

        public int GetUpgradeLevel(string upgradeId)
        {
            foreach (UpgradeLevelEntry entry in UpgradeLevels)
            {
                if (entry.Id == upgradeId)
                {
                    return entry.Level;
                }
            }

            return 0;
        }

        public void SetUpgradeLevel(string upgradeId, int level)
        {
            for (int i = 0; i < UpgradeLevels.Count; i++)
            {
                if (UpgradeLevels[i].Id == upgradeId)
                {
                    UpgradeLevels[i].Level = Math.Max(0, level);
                    return;
                }
            }

            UpgradeLevels.Add(new UpgradeLevelEntry { Id = upgradeId, Level = Math.Max(0, level) });
        }

        public bool IsLocationUnlocked(string locationId)
        {
            foreach (LocationUnlockEntry entry in UnlockedLocations)
            {
                if (entry.Id == locationId)
                {
                    return true;
                }
            }

            return false;
        }

        public void UnlockLocation(string locationId)
        {
            if (!IsLocationUnlocked(locationId))
            {
                UnlockedLocations.Add(new LocationUnlockEntry { Id = locationId });
            }
        }
    }

    [Serializable]
    public sealed class PrestigeState
    {
        public BigDouble MasalaLegacy;
        public int UnspentSkillPoints;
        public List<PrestigeSkillEntry> Skills = new List<PrestigeSkillEntry>();

        public int GetSkillLevel(string skillId)
        {
            EnsureSkills();
            foreach (PrestigeSkillEntry entry in Skills)
            {
                if (entry.Id == skillId)
                {
                    return entry.Level;
                }
            }

            return 0;
        }

        public void SetSkillLevel(string skillId, int level)
        {
            EnsureSkills();
            for (int i = 0; i < Skills.Count; i++)
            {
                if (Skills[i].Id == skillId)
                {
                    Skills[i].Level = Math.Max(0, level);
                    return;
                }
            }

            Skills.Add(new PrestigeSkillEntry { Id = skillId, Level = Math.Max(0, level) });
        }

        private void EnsureSkills()
        {
            if (Skills == null)
            {
                Skills = new List<PrestigeSkillEntry>();
            }
        }
    }

    [Serializable]
    public sealed class EventState
    {
        public string ActiveEventId;
        public double RemainingSeconds;
        public double CooldownSeconds;
        public int CompletedCount;

        public bool IsActive => !string.IsNullOrWhiteSpace(ActiveEventId) && RemainingSeconds > 0;
    }

    [Serializable]
    public sealed class MonetizationState
    {
        public bool NoAdsPurchased;
        public double ProductionBoostRemainingSeconds;
        public double ProductionBoostCooldownSeconds;
        public int RewardedOfflineBonusClaims;
    }

    [Serializable]
    public sealed class CosmeticState
    {
        public string StallThemeId;
        public string CupPackId;
        public string SignboardPackId;

        public static CosmeticState CreateDefault()
        {
            return new CosmeticState
            {
                StallThemeId = "classic-tapri",
                CupPackId = "kulhad-cups",
                SignboardPackId = "painted-board"
            };
        }
    }

    [Serializable]
    public sealed class UpgradeLevelEntry
    {
        public string Id;
        public int Level;
    }

    [Serializable]
    public sealed class LocationUnlockEntry
    {
        public string Id;
    }

    [Serializable]
    public sealed class PrestigeSkillEntry
    {
        public string Id;
        public int Level;
    }
}
