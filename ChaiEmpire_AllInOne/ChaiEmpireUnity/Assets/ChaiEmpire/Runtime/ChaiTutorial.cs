using BreakInfinity;

namespace ChaiEmpire
{
    public static class ChaiTutorial
    {
        public const string FirstUpgradeId = "strong-tea";

        public static ChaiTutorialPrompt GetPrompt(ChaiGameState state, ChaiContent content)
        {
            if (state == null || content == null || !content.TryGetUpgrade(FirstUpgradeId, out UpgradeDefinition firstUpgrade))
            {
                return ChaiTutorialPrompt.Complete;
            }

            int firstUpgradeLevel = state.GetUpgradeLevel(FirstUpgradeId);
            if (firstUpgradeLevel > 0)
            {
                return ChaiTutorialPrompt.Complete;
            }

            BigDouble firstUpgradeCost = firstUpgrade.GetCost(firstUpgradeLevel);
            string progress = ChaiNumberFormatter.Rupees(state.Rupees) + " / " + ChaiNumberFormatter.Rupees(firstUpgradeCost);

            if (state.ChaiServed <= BigDouble.Zero)
            {
                return new ChaiTutorialPrompt(
                    ChaiTutorialStep.FirstTap,
                    "Brew your first chai",
                    "Fresh kettle is waiting. Tap once to serve the first cup.",
                    progress,
                    "Tap Kettle",
                    ChaiTutorialAction.TapKettle);
            }

            if (state.Rupees >= firstUpgradeCost)
            {
                return new ChaiTutorialPrompt(
                    ChaiTutorialStep.BuyFirstUpgrade,
                    "First upgrade ready",
                    "Strong Tea Leaves are ready. Buy them and each kettle tap gets stronger.",
                    "Ready at " + ChaiNumberFormatter.Rupees(firstUpgradeCost),
                    "Buy Strong Tea",
                    ChaiTutorialAction.BuyFirstUpgrade);
            }

            return new ChaiTutorialPrompt(
                ChaiTutorialStep.SaveForFirstUpgrade,
                "Save for Strong Tea Leaves",
                "Keep brewing until the first upgrade is ready.",
                progress,
                "Tap Kettle",
                ChaiTutorialAction.TapKettle);
        }
    }

    public enum ChaiTutorialStep
    {
        Complete,
        FirstTap,
        SaveForFirstUpgrade,
        BuyFirstUpgrade
    }

    public enum ChaiTutorialAction
    {
        None,
        TapKettle,
        BuyFirstUpgrade
    }

    public readonly struct ChaiTutorialPrompt
    {
        public static readonly ChaiTutorialPrompt Complete = new ChaiTutorialPrompt(
            ChaiTutorialStep.Complete,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            ChaiTutorialAction.None);

        public ChaiTutorialPrompt(
            ChaiTutorialStep step,
            string title,
            string body,
            string progress,
            string primaryButtonLabel,
            ChaiTutorialAction primaryAction)
        {
            Step = step;
            Title = title;
            Body = body;
            Progress = progress;
            PrimaryButtonLabel = primaryButtonLabel;
            PrimaryAction = primaryAction;
        }

        public ChaiTutorialStep Step { get; }
        public string Title { get; }
        public string Body { get; }
        public string Progress { get; }
        public string PrimaryButtonLabel { get; }
        public ChaiTutorialAction PrimaryAction { get; }
        public bool ShouldShow => Step != ChaiTutorialStep.Complete;
    }
}
