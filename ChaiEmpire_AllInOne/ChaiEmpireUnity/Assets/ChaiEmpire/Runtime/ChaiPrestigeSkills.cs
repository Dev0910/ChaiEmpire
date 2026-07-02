using System.Collections.Generic;

namespace ChaiEmpire
{
    public enum PrestigeSkillEffect
    {
        TapMultiplier,
        RushTapMultiplier,
        PassiveMultiplier,
        RushCooldownReductionSeconds,
        UpgradeCostReduction,
        OfflineEfficiencyBonus,
        OfflineCapSecondsBonus,
        GlobalMultiplier,
        LocationCostReduction
    }

    public sealed class PrestigeSkillDefinition
    {
        public PrestigeSkillDefinition(
            string id,
            string displayName,
            string branch,
            string description,
            int maxLevel,
            PrestigeSkillEffect effect,
            double valuePerLevel,
            string effectLabel)
        {
            Id = id;
            DisplayName = displayName;
            Branch = branch;
            Description = description;
            MaxLevel = maxLevel;
            Effect = effect;
            ValuePerLevel = valuePerLevel;
            EffectLabel = effectLabel;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Branch { get; }
        public string Description { get; }
        public int MaxLevel { get; }
        public PrestigeSkillEffect Effect { get; }
        public double ValuePerLevel { get; }
        public string EffectLabel { get; }
    }

    public static class ChaiPrestigeSkills
    {
        private static readonly PrestigeSkillDefinition[] definitions =
        {
            new PrestigeSkillDefinition("brew-stronger-start", "Stronger First Pour", "Brew Craft", "Each new run starts with a stronger kettle tap.", 5, PrestigeSkillEffect.TapMultiplier, 0.20, "+20% tap per level"),
            new PrestigeSkillDefinition("brew-rush-taps", "Rush Brewing", "Brew Craft", "Rush Hour makes manual brewing even sharper.", 5, PrestigeSkillEffect.RushTapMultiplier, 0.10, "+10% rush tap per level"),
            new PrestigeSkillDefinition("ops-helper-training", "Helper Training", "Operations", "Trained helpers raise all passive production.", 5, PrestigeSkillEffect.PassiveMultiplier, 0.10, "+10% passive per level"),
            new PrestigeSkillDefinition("ops-fast-rush", "Faster Rush Prep", "Operations", "The stall prepares the next Rush Hour sooner.", 4, PrestigeSkillEffect.RushCooldownReductionSeconds, 5, "-5s rush cooldown per level"),
            new PrestigeSkillDefinition("supply-bulk-buying", "Bulk Buying", "Supply", "Better buying reduces upgrade costs.", 5, PrestigeSkillEffect.UpgradeCostReduction, 0.02, "-2% upgrade cost per level"),
            new PrestigeSkillDefinition("supply-offline-flask", "Offline Flask", "Supply", "Thermos discipline improves offline earnings.", 5, PrestigeSkillEffect.OfflineEfficiencyBonus, 0.05, "+5% offline efficiency per level"),
            new PrestigeSkillDefinition("supply-long-storage", "Long Storage", "Supply", "Better storage keeps chai viable for longer breaks.", 4, PrestigeSkillEffect.OfflineCapSecondsBonus, 3600, "+1h offline cap per level"),
            new PrestigeSkillDefinition("brand-loyal-regulars", "Loyal Regulars", "Brand", "Regular customers lift every source of income.", 5, PrestigeSkillEffect.GlobalMultiplier, 0.05, "+5% all income per level"),
            new PrestigeSkillDefinition("expand-cheaper-locations", "Better Rent Deals", "Expansion", "Sharper rollout deals reduce location unlock costs.", 5, PrestigeSkillEffect.LocationCostReduction, 0.03, "-3% location cost per level")
        };

        private static readonly Dictionary<string, PrestigeSkillDefinition> definitionsById = BuildDictionary();

        public static IReadOnlyList<PrestigeSkillDefinition> All => definitions;

        public static bool TryGet(string id, out PrestigeSkillDefinition definition)
        {
            return definitionsById.TryGetValue(id, out definition);
        }

        private static Dictionary<string, PrestigeSkillDefinition> BuildDictionary()
        {
            Dictionary<string, PrestigeSkillDefinition> result = new Dictionary<string, PrestigeSkillDefinition>();
            foreach (PrestigeSkillDefinition definition in definitions)
            {
                result[definition.Id] = definition;
            }

            return result;
        }
    }
}
