using System.Collections.Generic;

namespace ChaiEmpire
{
    public sealed class ChaiEventDefinition
    {
        public ChaiEventDefinition(
            string id,
            string displayName,
            string description,
            double durationSeconds,
            double cooldownSeconds,
            double tapMultiplierBonus,
            double passiveMultiplierBonus,
            double globalMultiplierBonus)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            DurationSeconds = durationSeconds;
            CooldownSeconds = cooldownSeconds;
            TapMultiplierBonus = tapMultiplierBonus;
            PassiveMultiplierBonus = passiveMultiplierBonus;
            GlobalMultiplierBonus = globalMultiplierBonus;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public double DurationSeconds { get; }
        public double CooldownSeconds { get; }
        public double TapMultiplierBonus { get; }
        public double PassiveMultiplierBonus { get; }
        public double GlobalMultiplierBonus { get; }
    }

    public static class ChaiEvents
    {
        private static readonly ChaiEventDefinition[] definitions =
        {
            new ChaiEventDefinition("monsoon-chai-rush", "Monsoon Chai Rush", "Rain pulls everyone toward hot cutting chai.", 180, 600, 0.50, 0.25, 0),
            new ChaiEventDefinition("diwali-sweet-combo", "Diwali Sweet Combo", "Festive mithai bundles lift every order.", 300, 900, 0, 0.25, 0.20),
            new ChaiEventDefinition("cricket-match-night", "Cricket Match Night", "Match breaks bring waves of thirsty regulars.", 240, 720, 0.25, 0.50, 0)
        };

        private static readonly Dictionary<string, ChaiEventDefinition> definitionsById = BuildDictionary();

        public static IReadOnlyList<ChaiEventDefinition> All => definitions;

        public static bool TryGet(string id, out ChaiEventDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                definition = null;
                return false;
            }

            return definitionsById.TryGetValue(id, out definition);
        }

        public static ChaiEventDefinition GetByRotationIndex(int completedCount)
        {
            int index = completedCount % definitions.Length;
            if (index < 0)
            {
                index += definitions.Length;
            }

            return definitions[index];
        }

        private static Dictionary<string, ChaiEventDefinition> BuildDictionary()
        {
            Dictionary<string, ChaiEventDefinition> result = new Dictionary<string, ChaiEventDefinition>();
            foreach (ChaiEventDefinition definition in definitions)
            {
                result[definition.Id] = definition;
            }

            return result;
        }
    }
}
