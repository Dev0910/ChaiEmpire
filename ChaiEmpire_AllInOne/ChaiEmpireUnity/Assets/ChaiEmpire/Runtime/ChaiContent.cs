using System;
using System.Collections.Generic;
using BreakInfinity;
using UnityEngine;

namespace ChaiEmpire
{
    public sealed class ChaiContent
    {
        private const string DefaultContentResourcePath = "ChaiEmpire/default-content";

        private readonly Dictionary<string, UpgradeDefinition> upgradesById = new Dictionary<string, UpgradeDefinition>();
        private readonly Dictionary<string, LocationDefinition> locationsById = new Dictionary<string, LocationDefinition>();

        private ChaiContent(
            IReadOnlyList<UpgradeDefinition> upgrades,
            IReadOnlyList<LocationDefinition> locations,
            double offlineEfficiency,
            double offlineCapSeconds,
            BigDouble prestigeUnlockRupees)
        {
            Upgrades = upgrades;
            Locations = locations;
            OfflineEfficiency = offlineEfficiency;
            OfflineCapSeconds = offlineCapSeconds;
            PrestigeUnlockRupees = prestigeUnlockRupees;

            foreach (UpgradeDefinition upgrade in upgrades)
            {
                upgradesById[upgrade.Id] = upgrade;
            }

            foreach (LocationDefinition location in locations)
            {
                locationsById[location.Id] = location;
            }
        }

        public IReadOnlyList<UpgradeDefinition> Upgrades { get; }
        public IReadOnlyList<LocationDefinition> Locations { get; }
        public double OfflineEfficiency { get; }
        public double OfflineCapSeconds { get; }
        public BigDouble PrestigeUnlockRupees { get; }

        public UpgradeDefinition GetUpgrade(string id)
        {
            if (!upgradesById.TryGetValue(id, out UpgradeDefinition upgrade))
            {
                throw new ArgumentException($"Unknown upgrade id: {id}", nameof(id));
            }

            return upgrade;
        }

        public bool TryGetUpgrade(string id, out UpgradeDefinition upgrade)
        {
            return upgradesById.TryGetValue(id, out upgrade);
        }

        public LocationDefinition GetLocation(string id)
        {
            if (!locationsById.TryGetValue(id, out LocationDefinition location))
            {
                throw new ArgumentException($"Unknown location id: {id}", nameof(id));
            }

            return location;
        }

        public bool TryGetLocation(string id, out LocationDefinition location)
        {
            return locationsById.TryGetValue(id, out location);
        }

        public static ChaiContent CreateDefault()
        {
            TextAsset contentAsset = Resources.Load<TextAsset>(DefaultContentResourcePath);
            if (contentAsset != null)
            {
                if (ChaiContentData.TryFromJson(contentAsset.text, out ChaiContentData data, out string error))
                {
                    return data.ToContent();
                }

                Debug.LogWarning("Chai Empire default content JSON is invalid; using built-in defaults. " + error);
            }

            return CreateBuiltInDefault();
        }

        public static ChaiContent CreateBuiltInDefault()
        {
            return ChaiContentData.CreateBuiltInDefault().ToContent();
        }

        internal static ChaiContent FromDefinitions(
            IReadOnlyList<UpgradeDefinition> upgrades,
            IReadOnlyList<LocationDefinition> locations,
            double offlineEfficiency,
            double offlineCapSeconds,
            BigDouble prestigeUnlockRupees)
        {
            return new ChaiContent(upgrades, locations, offlineEfficiency, offlineCapSeconds, prestigeUnlockRupees);
        }
    }

    public enum UpgradeKind
    {
        TapFlat,
        TapMultiplier,
        PassiveFlat,
        GlobalMultiplier
    }

    public sealed class UpgradeDefinition
    {
        private UpgradeDefinition(
            string id,
            string displayName,
            string description,
            string category,
            UpgradeKind kind,
            double baseCost,
            double costMultiplier,
            double valuePerLevel,
            bool isAutomation,
            int maxLevel)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            Category = category;
            Kind = kind;
            BaseCost = new BigDouble(baseCost);
            CostMultiplier = costMultiplier;
            ValuePerLevel = valuePerLevel;
            IsAutomation = isAutomation;
            MaxLevel = maxLevel;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public string Category { get; }
        public UpgradeKind Kind { get; }
        public BigDouble BaseCost { get; }
        public double CostMultiplier { get; }
        public double ValuePerLevel { get; }
        public bool IsAutomation { get; }
        public int MaxLevel { get; }

        public BigDouble GetCost(int currentLevel)
        {
            return BaseCost * BigDouble.Pow(new BigDouble(CostMultiplier), Math.Max(0, currentLevel));
        }

        public static UpgradeDefinition TapFlat(string id, string displayName, string description, string category, double baseCost, double costMultiplier, double valuePerLevel, int maxLevel = 0)
        {
            return new UpgradeDefinition(id, displayName, description, category, UpgradeKind.TapFlat, baseCost, costMultiplier, valuePerLevel, false, maxLevel);
        }

        public static UpgradeDefinition TapMultiplier(string id, string displayName, string description, string category, double baseCost, double costMultiplier, double valuePerLevel, int maxLevel = 0)
        {
            return new UpgradeDefinition(id, displayName, description, category, UpgradeKind.TapMultiplier, baseCost, costMultiplier, valuePerLevel, false, maxLevel);
        }

        public static UpgradeDefinition PassiveFlat(string id, string displayName, string description, string category, double baseCost, double costMultiplier, double valuePerLevel, bool isAutomation, int maxLevel = 0)
        {
            return new UpgradeDefinition(id, displayName, description, category, UpgradeKind.PassiveFlat, baseCost, costMultiplier, valuePerLevel, isAutomation, maxLevel);
        }

        public static UpgradeDefinition GlobalMultiplier(string id, string displayName, string description, string category, double baseCost, double costMultiplier, double valuePerLevel, int maxLevel = 0)
        {
            return new UpgradeDefinition(id, displayName, description, category, UpgradeKind.GlobalMultiplier, baseCost, costMultiplier, valuePerLevel, false, maxLevel);
        }
    }

    public sealed class LocationDefinition
    {
        public LocationDefinition(string id, string displayName, string description, double unlockCost, double demandMultiplier, bool unlockedByDefault)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            UnlockCost = new BigDouble(unlockCost);
            DemandMultiplier = demandMultiplier;
            UnlockedByDefault = unlockedByDefault;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public BigDouble UnlockCost { get; }
        public double DemandMultiplier { get; }
        public bool UnlockedByDefault { get; }
    }
}
