using System.Collections.Generic;

namespace ChaiEmpire
{
    public sealed class CosmeticDefinition
    {
        public CosmeticDefinition(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        public string Id { get; }
        public string DisplayName { get; }
    }

    public static class ChaiCosmetics
    {
        public static readonly CosmeticDefinition[] StallThemes =
        {
            new CosmeticDefinition("classic-tapri", "Classic Tapri"),
            new CosmeticDefinition("festival-lights", "Festival Lights"),
            new CosmeticDefinition("monsoon-blue", "Monsoon Blue")
        };

        public static readonly CosmeticDefinition[] CupPacks =
        {
            new CosmeticDefinition("kulhad-cups", "Kulhad Cups"),
            new CosmeticDefinition("steel-glasses", "Steel Glasses"),
            new CosmeticDefinition("paper-cups", "Paper Cups")
        };

        public static readonly CosmeticDefinition[] SignboardPacks =
        {
            new CosmeticDefinition("painted-board", "Painted Board"),
            new CosmeticDefinition("neon-board", "Neon Board"),
            new CosmeticDefinition("brass-board", "Brass Board")
        };

        public static bool Contains(IReadOnlyList<CosmeticDefinition> definitions, string id)
        {
            foreach (CosmeticDefinition definition in definitions)
            {
                if (definition.Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetNextId(IReadOnlyList<CosmeticDefinition> definitions, string currentId)
        {
            if (definitions.Count == 0)
            {
                return currentId;
            }

            for (int i = 0; i < definitions.Count; i++)
            {
                if (definitions[i].Id == currentId)
                {
                    return definitions[(i + 1) % definitions.Count].Id;
                }
            }

            return definitions[0].Id;
        }

        public static string GetDisplayName(IReadOnlyList<CosmeticDefinition> definitions, string id)
        {
            foreach (CosmeticDefinition definition in definitions)
            {
                if (definition.Id == id)
                {
                    return definition.DisplayName;
                }
            }

            return definitions.Count > 0 ? definitions[0].DisplayName : string.Empty;
        }
    }
}
