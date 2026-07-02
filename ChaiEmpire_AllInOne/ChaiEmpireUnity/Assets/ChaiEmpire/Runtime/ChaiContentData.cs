using System;
using System.Collections.Generic;
using BreakInfinity;
using UnityEngine;

namespace ChaiEmpire
{
    [Serializable]
    public sealed class ChaiContentData
    {
        public double offlineEfficiency;
        public double offlineCapSeconds;
        public string prestigeUnlockRupees;
        public ChaiUpgradeData[] upgrades;
        public ChaiLocationData[] locations;

        public static bool TryFromJson(string json, out ChaiContentData data, out string error)
        {
            data = null;
            error = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "Content JSON is empty.";
                return false;
            }

            try
            {
                data = JsonUtility.FromJson<ChaiContentData>(json);
            }
            catch (Exception exception)
            {
                error = "Content JSON could not be parsed: " + exception.Message;
                return false;
            }

            IReadOnlyList<string> errors = ChaiContentValidator.Validate(data);
            if (errors.Count > 0)
            {
                error = string.Join("; ", errors);
                return false;
            }

            return true;
        }

        public static string ToJson(ChaiContentData data)
        {
            return JsonUtility.ToJson(data, true);
        }

        public ChaiContent ToContent()
        {
            List<UpgradeDefinition> upgradeDefinitions = new List<UpgradeDefinition>();
            foreach (ChaiUpgradeData upgrade in upgrades)
            {
                upgradeDefinitions.Add(upgrade.ToDefinition());
            }

            List<LocationDefinition> locationDefinitions = new List<LocationDefinition>();
            foreach (ChaiLocationData location in locations)
            {
                locationDefinitions.Add(location.ToDefinition());
            }

            return ChaiContent.FromDefinitions(
                upgradeDefinitions,
                locationDefinitions,
                offlineEfficiency,
                offlineCapSeconds,
                BigDouble.Parse(prestigeUnlockRupees));
        }

        public static ChaiContentData CreateBuiltInDefault()
        {
            return new ChaiContentData
            {
                offlineEfficiency = 0.75,
                offlineCapSeconds = 28800,
                prestigeUnlockRupees = "1e9",
                upgrades = new[]
                {
                    ChaiUpgradeData.TapFlat("strong-tea", "Strong Tea Leaves", "Each kettle tap brews one extra paid cup.", "Brew Craft", 10, 1.55, 1),
                    ChaiUpgradeData.TapMultiplier("adrak-kick", "Adrak Kick", "Ginger aroma makes every cup worth more.", "Brew Craft", 45, 1.6, 0.15),
                    ChaiUpgradeData.TapMultiplier("elaichi-aroma", "Elaichi Aroma", "Cardamom pulls people in from the next lane.", "Brew Craft", 120, 1.66, 0.2),
                    ChaiUpgradeData.PassiveFlat("helper-boy", "Helper Boy", "Auto-serves the queue while you focus on brewing.", "Automation", 50, 1.65, 0.5, true),
                    ChaiUpgradeData.PassiveFlat("upi-cashier", "UPI Cashier", "A QR stand collects payments automatically.", "Automation", 95, 1.68, 0.35, true),
                    ChaiUpgradeData.PassiveFlat("bulk-kettle", "Bulk Kettle", "A larger kettle keeps chai flowing by itself.", "Automation", 180, 1.7, 2, true),
                    ChaiUpgradeData.PassiveFlat("samosa-counter", "Samosa Counter", "Snacks raise average order value through the day.", "Add-ons", 500, 1.72, 5, true),
                    ChaiUpgradeData.PassiveFlat("bun-maska-tray", "Bun Maska Tray", "Buttery side orders keep office crowds lingering.", "Add-ons", 1250, 1.75, 12, true),
                    ChaiUpgradeData.PassiveFlat("kulhad-stack", "Kulhad Stack", "Clay cups make every location feel memorable.", "Add-ons", 3500, 1.78, 28, true),
                    ChaiUpgradeData.GlobalMultiplier("painted-signboard", "Painted Signboard", "A bright board improves demand everywhere.", "Brand", 1000, 1.85, 0.1),
                    ChaiUpgradeData.GlobalMultiplier("influencer-reel", "Influencer Reel", "A local food reel brings new queues overnight.", "Brand", 5000, 1.9, 0.35),
                    ChaiUpgradeData.PassiveFlat("delivery-partner", "Delivery Partner", "Thermos runs send chai to offices nearby.", "Expansion", 25000, 1.9, 80, true),
                    ChaiUpgradeData.PassiveFlat("franchise-kit", "Franchise Kit", "A repeatable stall setup spreads your recipe.", "Expansion", 250000, 1.95, 600, true),
                    ChaiUpgradeData.PassiveFlat("tea-estate-contract", "Tea Estate Contract", "Direct supply stabilizes the national rollout.", "Supply", 2000000, 2.0, 3000, true),
                    ChaiUpgradeData.PassiveFlat("export-counter", "Export Counter", "Packaged masala chai starts leaving the country.", "Late Game", 20000000, 2.05, 25000, true)
                },
                locations = new[]
                {
                    new ChaiLocationData("gali-tapri", "Gali Tapri", "Your first loyal lane-side queue.", 0, 1, true),
                    new ChaiLocationData("bus-stand", "Bus Stand", "Morning commuters create steady demand.", 250, 1.25, false),
                    new ChaiLocationData("railway-platform", "Railway Platform", "Peak-hour trains turn chai into a rhythm game.", 2000, 1.65, false),
                    new ChaiLocationData("college-canteen", "College Canteen", "Exam nights and gossip tables stretch demand.", 12000, 2, false),
                    new ChaiLocationData("it-park", "IT Park", "Sprint planning runs on cutting chai.", 80000, 2.75, false),
                    new ChaiLocationData("highway-dhaba", "Highway Dhaba", "Truckers and families keep the stove hot.", 500000, 3.7, false),
                    new ChaiLocationData("mall-kiosk", "Mall Kiosk", "Premium cups meet weekend footfall.", 5000000, 5.2, false),
                    new ChaiLocationData("airport-lounge", "Airport Lounge", "Your brand is now national enough to prestige later.", 100000000, 8, false)
                }
            };
        }
    }

    [Serializable]
    public sealed class ChaiUpgradeData
    {
        public string id;
        public string displayName;
        public string description;
        public string category;
        public string kind;
        public double baseCost;
        public double costMultiplier;
        public double valuePerLevel;
        public bool isAutomation;
        public int maxLevel;

        public static ChaiUpgradeData TapFlat(string id, string displayName, string description, string category, double baseCost, double costMultiplier, double valuePerLevel)
        {
            return Create(id, displayName, description, category, "TapFlat", baseCost, costMultiplier, valuePerLevel, false);
        }

        public static ChaiUpgradeData TapMultiplier(string id, string displayName, string description, string category, double baseCost, double costMultiplier, double valuePerLevel)
        {
            return Create(id, displayName, description, category, "TapMultiplier", baseCost, costMultiplier, valuePerLevel, false);
        }

        public static ChaiUpgradeData PassiveFlat(string id, string displayName, string description, string category, double baseCost, double costMultiplier, double valuePerLevel, bool isAutomation)
        {
            return Create(id, displayName, description, category, "PassiveFlat", baseCost, costMultiplier, valuePerLevel, isAutomation);
        }

        public static ChaiUpgradeData GlobalMultiplier(string id, string displayName, string description, string category, double baseCost, double costMultiplier, double valuePerLevel)
        {
            return Create(id, displayName, description, category, "GlobalMultiplier", baseCost, costMultiplier, valuePerLevel, false);
        }

        public UpgradeDefinition ToDefinition()
        {
            switch (kind)
            {
                case "TapFlat":
                    return UpgradeDefinition.TapFlat(id, displayName, description, category, baseCost, costMultiplier, valuePerLevel, maxLevel);
                case "TapMultiplier":
                    return UpgradeDefinition.TapMultiplier(id, displayName, description, category, baseCost, costMultiplier, valuePerLevel, maxLevel);
                case "PassiveFlat":
                    return UpgradeDefinition.PassiveFlat(id, displayName, description, category, baseCost, costMultiplier, valuePerLevel, isAutomation, maxLevel);
                case "GlobalMultiplier":
                    return UpgradeDefinition.GlobalMultiplier(id, displayName, description, category, baseCost, costMultiplier, valuePerLevel, maxLevel);
                default:
                    throw new ArgumentException("Unknown upgrade kind: " + kind);
            }
        }

        private static ChaiUpgradeData Create(string id, string displayName, string description, string category, string kind, double baseCost, double costMultiplier, double valuePerLevel, bool isAutomation)
        {
            return new ChaiUpgradeData
            {
                id = id,
                displayName = displayName,
                description = description,
                category = category,
                kind = kind,
                baseCost = baseCost,
                costMultiplier = costMultiplier,
                valuePerLevel = valuePerLevel,
                isAutomation = isAutomation
            };
        }
    }

    [Serializable]
    public sealed class ChaiLocationData
    {
        public string id;
        public string displayName;
        public string description;
        public double unlockCost;
        public double demandMultiplier;
        public bool unlockedByDefault;

        public ChaiLocationData(string id, string displayName, string description, double unlockCost, double demandMultiplier, bool unlockedByDefault)
        {
            this.id = id;
            this.displayName = displayName;
            this.description = description;
            this.unlockCost = unlockCost;
            this.demandMultiplier = demandMultiplier;
            this.unlockedByDefault = unlockedByDefault;
        }

        public LocationDefinition ToDefinition()
        {
            return new LocationDefinition(id, displayName, description, unlockCost, demandMultiplier, unlockedByDefault);
        }
    }

    public static class ChaiContentValidator
    {
        public static IReadOnlyList<string> Validate(ChaiContentData data)
        {
            List<string> errors = new List<string>();
            if (data == null)
            {
                errors.Add("Content data is null.");
                return errors;
            }

            if (data.offlineEfficiency <= 0 || data.offlineEfficiency > 1)
            {
                errors.Add("Offline efficiency must be greater than 0 and no more than 1.");
            }

            if (data.offlineCapSeconds <= 0)
            {
                errors.Add("Offline cap seconds must be positive.");
            }

            if (string.IsNullOrWhiteSpace(data.prestigeUnlockRupees))
            {
                errors.Add("Prestige unlock rupees must be provided.");
            }
            else
            {
                try
                {
                    if (BigDouble.Parse(data.prestigeUnlockRupees) <= BigDouble.Zero)
                    {
                        errors.Add("Prestige unlock rupees must be positive.");
                    }
                }
                catch (Exception)
                {
                    errors.Add("Prestige unlock rupees must be a valid BigDouble value.");
                }
            }

            ValidateUpgrades(data.upgrades, errors);
            ValidateLocations(data.locations, errors);
            return errors;
        }

        private static void ValidateUpgrades(ChaiUpgradeData[] upgrades, List<string> errors)
        {
            if (upgrades == null || upgrades.Length == 0)
            {
                errors.Add("At least one upgrade is required.");
                return;
            }

            HashSet<string> ids = new HashSet<string>();
            bool hasAutomation = false;
            foreach (ChaiUpgradeData upgrade in upgrades)
            {
                if (upgrade == null)
                {
                    errors.Add("Upgrade entry is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(upgrade.id))
                {
                    errors.Add("Upgrade id is required.");
                }
                else if (!ids.Add(upgrade.id))
                {
                    errors.Add("Duplicate upgrade id: " + upgrade.id);
                }

                if (string.IsNullOrWhiteSpace(upgrade.displayName))
                {
                    errors.Add("Upgrade display name is required for " + upgrade.id);
                }

                if (string.IsNullOrWhiteSpace(upgrade.category))
                {
                    errors.Add("Upgrade category is required for " + upgrade.id);
                }

                if (!IsKnownUpgradeKind(upgrade.kind))
                {
                    errors.Add("Unknown upgrade kind for " + upgrade.id + ": " + upgrade.kind);
                }

                if (upgrade.baseCost <= 0)
                {
                    errors.Add("Upgrade base cost must be positive for " + upgrade.id);
                }

                if (upgrade.costMultiplier <= 1)
                {
                    errors.Add("Upgrade cost multiplier must be greater than 1 for " + upgrade.id);
                }

                if (upgrade.valuePerLevel <= 0)
                {
                    errors.Add("Upgrade value per level must be positive for " + upgrade.id);
                }

                if (upgrade.isAutomation)
                {
                    hasAutomation = true;
                    if (upgrade.kind != "PassiveFlat")
                    {
                        errors.Add("Automation upgrade must be PassiveFlat: " + upgrade.id);
                    }
                }
            }

            if (!hasAutomation)
            {
                errors.Add("At least one automation upgrade is required.");
            }
        }

        private static void ValidateLocations(ChaiLocationData[] locations, List<string> errors)
        {
            if (locations == null || locations.Length == 0)
            {
                errors.Add("At least one location is required.");
                return;
            }

            HashSet<string> ids = new HashSet<string>();
            int defaultCount = 0;
            double previousCost = -1;
            double previousDemand = 0;

            foreach (ChaiLocationData location in locations)
            {
                if (location == null)
                {
                    errors.Add("Location entry is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(location.id))
                {
                    errors.Add("Location id is required.");
                }
                else if (!ids.Add(location.id))
                {
                    errors.Add("Duplicate location id: " + location.id);
                }

                if (string.IsNullOrWhiteSpace(location.displayName))
                {
                    errors.Add("Location display name is required for " + location.id);
                }

                if (location.unlockCost < 0)
                {
                    errors.Add("Location unlock cost cannot be negative for " + location.id);
                }

                if (location.demandMultiplier < 1)
                {
                    errors.Add("Location demand multiplier must be at least 1 for " + location.id);
                }

                if (location.unlockCost < previousCost)
                {
                    errors.Add("Location unlock costs must be ordered: " + location.id);
                }

                if (location.demandMultiplier < previousDemand)
                {
                    errors.Add("Location demand multipliers must be ordered: " + location.id);
                }

                if (location.unlockedByDefault)
                {
                    defaultCount++;
                    if (location.unlockCost != 0)
                    {
                        errors.Add("Default location must have zero unlock cost: " + location.id);
                    }
                }

                previousCost = location.unlockCost;
                previousDemand = location.demandMultiplier;
            }

            if (defaultCount != 1)
            {
                errors.Add("Exactly one default location is required.");
            }
        }

        private static bool IsKnownUpgradeKind(string kind)
        {
            return kind == "TapFlat" ||
                kind == "TapMultiplier" ||
                kind == "PassiveFlat" ||
                kind == "GlobalMultiplier";
        }
    }
}
