using System.Collections.Generic;

namespace ChaiEmpire
{
    public sealed class ChaiAchievementDefinition
    {
        public ChaiAchievementDefinition(string id, string displayName, string description)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
    }

    public static class ChaiProductionServices
    {
        public const string PrivacyPolicyUrl = "https://example.com/chai-empire/privacy";

        private static readonly ChaiAchievementDefinition[] achievements =
        {
            new ChaiAchievementDefinition("first-upgrade", "First Upgrade", "Buy any upgrade."),
            new ChaiAchievementDefinition("bus-stand-open", "Bus Stand Open", "Unlock the Bus Stand location."),
            new ChaiAchievementDefinition("first-event", "Event Starter", "Complete one optional live event."),
            new ChaiAchievementDefinition("secret-masala", "Secret Masala", "Preserve Secret Masala for the first time."),
            new ChaiAchievementDefinition("no-ads-owned", "Peaceful Stall", "Own the optional no-ads entitlement.")
        };

        public static IReadOnlyList<ChaiAchievementDefinition> Achievements => achievements;
    }
}
