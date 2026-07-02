using System;
using BreakInfinity;

namespace ChaiEmpire
{
    public sealed class ChaiGame
    {
        private const double LegacyMultiplierPerMasala = 0.01;
        private const double RushDurationSeconds = 20;
        private const double RushCooldownSeconds = 90;
        private const double RushMultiplier = 2;
        private const double MinimumDiscountedCostMultiplier = 0.1;
        private const double RewardedProductionBoostDurationSeconds = 120;
        private const double RewardedProductionBoostCooldownSeconds = 600;
        private const double RewardedProductionBoostMultiplier = 2;

        private readonly ChaiContent content;

        private ChaiGame(ChaiContent content, ChaiGameState state)
        {
            this.content = content;
            State = state;
            EnsureStateDefaults();
        }

        public ChaiGameState State { get; private set; }

        public static ChaiGame NewGame(ChaiContent content)
        {
            return new ChaiGame(content, ChaiGameState.CreateNew());
        }

        public static ChaiGame FromState(ChaiContent content, ChaiGameState state)
        {
            return new ChaiGame(content, state ?? ChaiGameState.CreateNew());
        }

        public BigDouble GetTapValue()
        {
            BigDouble flat = new BigDouble(1);
            double tapMultiplier = 1;

            foreach (UpgradeDefinition upgrade in content.Upgrades)
            {
                int level = State.GetUpgradeLevel(upgrade.Id);
                if (level <= 0)
                {
                    continue;
                }

                if (upgrade.Kind == UpgradeKind.TapFlat)
                {
                    flat += upgrade.ValuePerLevel * level;
                }
                else if (upgrade.Kind == UpgradeKind.TapMultiplier)
                {
                    tapMultiplier += upgrade.ValuePerLevel * level;
                }
            }

            double skillTapMultiplier = 1 + GetSkillBonus(PrestigeSkillEffect.TapMultiplier);
            if (State.RushRemainingSeconds > 0)
            {
                skillTapMultiplier += GetSkillBonus(PrestigeSkillEffect.RushTapMultiplier);
            }

            return flat * tapMultiplier * skillTapMultiplier * GetEventTapMultiplier() * GetDemandMultiplier() * GetGlobalMultiplier() * GetLegacyMultiplier() * GetRushMultiplier() * GetProductionBoostMultiplier();
        }

        public BigDouble GetPassiveRupeesPerSecond()
        {
            return GetPassiveRupeesPerSecond(includeRush: true);
        }

        public BigDouble GetPassiveRupeesPerSecond(bool includeRush)
        {
            BigDouble passive = BigDouble.Zero;

            foreach (UpgradeDefinition upgrade in content.Upgrades)
            {
                if (upgrade.Kind != UpgradeKind.PassiveFlat)
                {
                    continue;
                }

                int level = State.GetUpgradeLevel(upgrade.Id);
                if (level > 0)
                {
                    passive += upgrade.ValuePerLevel * level;
                }
            }

            BigDouble multiplier = GetDemandMultiplier() * GetGlobalMultiplier() * GetLegacyMultiplier() * GetSkillMultiplier(PrestigeSkillEffect.PassiveMultiplier) * GetEventPassiveMultiplier() * GetProductionBoostMultiplier();
            if (includeRush)
            {
                multiplier *= GetRushMultiplier();
            }

            return passive * multiplier;
        }

        public void TapKettle(int taps = 1)
        {
            if (taps <= 0)
            {
                return;
            }

            Earn(GetTapValue() * taps);
            State.ChaiServed += taps;
        }

        public void TapCustomerQueue()
        {
            Earn(GetTapValue() * 3);
            State.ChaiServed += 3;
        }

        public void Tick(double deltaSeconds)
        {
            if (deltaSeconds <= 0)
            {
                return;
            }

            Earn(GetPassiveRupeesPerSecond() * deltaSeconds);

            State.RushRemainingSeconds = Math.Max(0, State.RushRemainingSeconds - deltaSeconds);
            State.RushCooldownSeconds = Math.Max(0, State.RushCooldownSeconds - deltaSeconds);
            UpdateEventTimers(deltaSeconds);
            UpdateMonetizationTimers(deltaSeconds);
        }

        public bool TryTriggerRushHour()
        {
            if (State.RushCooldownSeconds > 0)
            {
                return false;
            }

            State.RushRemainingSeconds = RushDurationSeconds;
            State.RushCooldownSeconds = GetRushCooldownSeconds();
            return true;
        }

        public bool TryBuyUpgrade(string upgradeId)
        {
            if (!content.TryGetUpgrade(upgradeId, out UpgradeDefinition upgrade))
            {
                return false;
            }

            int currentLevel = State.GetUpgradeLevel(upgradeId);
            if (upgrade.MaxLevel > 0 && currentLevel >= upgrade.MaxLevel)
            {
                return false;
            }

            BigDouble cost = GetUpgradeCost(upgrade, currentLevel);
            if (State.Rupees < cost)
            {
                return false;
            }

            State.Rupees -= cost;
            State.SetUpgradeLevel(upgradeId, currentLevel + 1);
            return true;
        }

        public bool TryUnlockLocation(string locationId)
        {
            if (!content.TryGetLocation(locationId, out LocationDefinition location) || State.IsLocationUnlocked(locationId))
            {
                return false;
            }

            BigDouble unlockCost = GetLocationUnlockCost(location);
            if (State.Rupees < unlockCost)
            {
                return false;
            }

            State.Rupees -= unlockCost;
            State.UnlockLocation(locationId);
            return true;
        }

        public OfflineReward ApplyOfflineProgress(TimeSpan elapsed)
        {
            double rawSeconds = Math.Max(0, elapsed.TotalSeconds);
            double cappedSeconds = Math.Min(rawSeconds, GetOfflineCapSeconds());
            BigDouble earned = GetPassiveRupeesPerSecond(includeRush: false) * cappedSeconds * GetOfflineEfficiency();
            Earn(earned);

            return new OfflineReward(earned, rawSeconds, cappedSeconds, rawSeconds > cappedSeconds);
        }

        public PrestigePreview GetPrestigePreview()
        {
            bool airportUnlocked = State.IsLocationUnlocked("airport-lounge");
            bool canPrestige = airportUnlocked && State.TotalLifetimeRupees >= content.PrestigeUnlockRupees;
            if (!canPrestige)
            {
                return new PrestigePreview(false, BigDouble.Zero, "Reach the Airport Lounge and 1B lifetime rupees to preserve Secret Masala.");
            }

            BigDouble projected = BigDouble.Floor(BigDouble.Sqrt(State.TotalLifetimeRupees / 10000000));
            return new PrestigePreview(true, projected, "Secret Masala is ready. Preserve it to reset this run and earn skill points.");
        }

        public bool TryPrestige(out PrestigeResult result)
        {
            PrestigePreview preview = GetPrestigePreview();
            if (!preview.CanPrestige || preview.ProjectedMasalaLegacy <= BigDouble.Zero)
            {
                result = new PrestigeResult(false, BigDouble.Zero, 0, preview.Message);
                return false;
            }

            PrestigeState preservedPrestige = GetPrestigeState();
            BigDouble gainedLegacy = preview.ProjectedMasalaLegacy;
            int gainedSkillPoints = ConvertLegacyToSkillPoints(gainedLegacy);
            preservedPrestige.MasalaLegacy += gainedLegacy;
            preservedPrestige.UnspentSkillPoints = Math.Max(0, preservedPrestige.UnspentSkillPoints) + gainedSkillPoints;

            ChaiGameState freshState = ChaiGameState.CreateNew();
            freshState.Prestige = preservedPrestige;
            freshState.Monetization = GetMonetizationState();
            freshState.Cosmetics = GetCosmeticState();
            State = freshState;

            result = new PrestigeResult(true, gainedLegacy, gainedSkillPoints, "Secret Masala preserved. The new stall opens stronger.");
            return true;
        }

        public bool TrySpendSkillPoint(string skillId)
        {
            if (!ChaiPrestigeSkills.TryGet(skillId, out PrestigeSkillDefinition definition))
            {
                return false;
            }

            PrestigeState prestige = GetPrestigeState();
            int currentLevel = prestige.GetSkillLevel(skillId);
            if (currentLevel >= definition.MaxLevel || prestige.UnspentSkillPoints <= 0)
            {
                return false;
            }

            prestige.UnspentSkillPoints--;
            prestige.SetSkillLevel(skillId, currentLevel + 1);
            return true;
        }

        public ChaiEventDefinition GetNextEventDefinition()
        {
            return ChaiEvents.GetByRotationIndex(GetEventState().CompletedCount);
        }

        public bool TryGetActiveEvent(out ChaiEventDefinition definition)
        {
            EventState eventState = GetEventState();
            if (eventState.IsActive && ChaiEvents.TryGet(eventState.ActiveEventId, out definition))
            {
                return true;
            }

            definition = null;
            return false;
        }

        public bool TryStartEvent(string eventId)
        {
            EventState eventState = GetEventState();
            if (eventState.IsActive || eventState.CooldownSeconds > 0)
            {
                return false;
            }

            if (!ChaiEvents.TryGet(eventId, out ChaiEventDefinition definition))
            {
                return false;
            }

            eventState.ActiveEventId = definition.Id;
            eventState.RemainingSeconds = definition.DurationSeconds;
            eventState.CooldownSeconds = 0;
            return true;
        }

        public bool TryClaimRewardedOfflineBonus(BigDouble baseReward)
        {
            if (baseReward <= BigDouble.Zero)
            {
                return false;
            }

            Earn(baseReward);
            GetMonetizationState().RewardedOfflineBonusClaims++;
            return true;
        }

        public bool TryStartRewardedProductionBoost()
        {
            MonetizationState monetization = GetMonetizationState();
            if (monetization.ProductionBoostRemainingSeconds > 0 || monetization.ProductionBoostCooldownSeconds > 0)
            {
                return false;
            }

            monetization.ProductionBoostRemainingSeconds = RewardedProductionBoostDurationSeconds;
            return true;
        }

        public bool TryPurchaseNoAds()
        {
            MonetizationState monetization = GetMonetizationState();
            if (monetization.NoAdsPurchased)
            {
                return false;
            }

            monetization.NoAdsPurchased = true;
            return true;
        }

        public bool TrySelectStallTheme(string themeId)
        {
            if (!ChaiCosmetics.Contains(ChaiCosmetics.StallThemes, themeId))
            {
                return false;
            }

            GetCosmeticState().StallThemeId = themeId;
            return true;
        }

        public bool TrySelectCupPack(string cupPackId)
        {
            if (!ChaiCosmetics.Contains(ChaiCosmetics.CupPacks, cupPackId))
            {
                return false;
            }

            GetCosmeticState().CupPackId = cupPackId;
            return true;
        }

        public bool TrySelectSignboardPack(string signboardPackId)
        {
            if (!ChaiCosmetics.Contains(ChaiCosmetics.SignboardPacks, signboardPackId))
            {
                return false;
            }

            GetCosmeticState().SignboardPackId = signboardPackId;
            return true;
        }

        public BigDouble GetUpgradeCost(string upgradeId)
        {
            if (!content.TryGetUpgrade(upgradeId, out UpgradeDefinition upgrade))
            {
                return BigDouble.Zero;
            }

            return GetUpgradeCost(upgrade, State.GetUpgradeLevel(upgradeId));
        }

        public BigDouble GetLocationUnlockCost(string locationId)
        {
            if (!content.TryGetLocation(locationId, out LocationDefinition location))
            {
                return BigDouble.Zero;
            }

            return GetLocationUnlockCost(location);
        }

        public double GetDemandMultiplier()
        {
            double multiplier = 1;
            foreach (LocationDefinition location in content.Locations)
            {
                if (State.IsLocationUnlocked(location.Id))
                {
                    multiplier = Math.Max(multiplier, location.DemandMultiplier);
                }
            }

            return multiplier;
        }

        public double GetGlobalMultiplier()
        {
            double multiplier = 1;
            foreach (UpgradeDefinition upgrade in content.Upgrades)
            {
                if (upgrade.Kind == UpgradeKind.GlobalMultiplier)
                {
                    multiplier += upgrade.ValuePerLevel * State.GetUpgradeLevel(upgrade.Id);
                }
            }

            return multiplier + GetSkillBonus(PrestigeSkillEffect.GlobalMultiplier) + GetEventGlobalMultiplierBonus();
        }

        public double GetLegacyMultiplier()
        {
            return 1 + GetPrestigeState().MasalaLegacy.ToDouble() * LegacyMultiplierPerMasala;
        }

        public double GetRushMultiplier()
        {
            return State.RushRemainingSeconds > 0 ? RushMultiplier : 1;
        }

        public double GetOfflineEfficiency()
        {
            return Math.Min(1, content.OfflineEfficiency + GetSkillBonus(PrestigeSkillEffect.OfflineEfficiencyBonus));
        }

        public double GetOfflineCapSeconds()
        {
            return content.OfflineCapSeconds + GetSkillBonus(PrestigeSkillEffect.OfflineCapSecondsBonus);
        }

        public double GetEventTapMultiplier()
        {
            return 1 + GetActiveEventBonus(definition => definition.TapMultiplierBonus);
        }

        public double GetEventPassiveMultiplier()
        {
            return 1 + GetActiveEventBonus(definition => definition.PassiveMultiplierBonus);
        }

        public double GetProductionBoostMultiplier()
        {
            return GetMonetizationState().ProductionBoostRemainingSeconds > 0 ? RewardedProductionBoostMultiplier : 1;
        }

        private BigDouble GetUpgradeCost(UpgradeDefinition upgrade, int currentLevel)
        {
            return ApplyCostReduction(upgrade.GetCost(currentLevel), PrestigeSkillEffect.UpgradeCostReduction);
        }

        private BigDouble GetLocationUnlockCost(LocationDefinition location)
        {
            return ApplyCostReduction(location.UnlockCost, PrestigeSkillEffect.LocationCostReduction);
        }

        private BigDouble ApplyCostReduction(BigDouble cost, PrestigeSkillEffect effect)
        {
            double multiplier = Math.Max(MinimumDiscountedCostMultiplier, 1 - GetSkillBonus(effect));
            return cost * multiplier;
        }

        private double GetRushCooldownSeconds()
        {
            return Math.Max(30, RushCooldownSeconds - GetSkillBonus(PrestigeSkillEffect.RushCooldownReductionSeconds));
        }

        private double GetSkillMultiplier(PrestigeSkillEffect effect)
        {
            return 1 + GetSkillBonus(effect);
        }

        private double GetSkillBonus(PrestigeSkillEffect effect)
        {
            PrestigeState prestige = GetPrestigeState();
            double bonus = 0;
            foreach (PrestigeSkillDefinition definition in ChaiPrestigeSkills.All)
            {
                if (definition.Effect == effect)
                {
                    bonus += definition.ValuePerLevel * prestige.GetSkillLevel(definition.Id);
                }
            }

            return bonus;
        }

        private PrestigeState GetPrestigeState()
        {
            if (State.Prestige == null)
            {
                State.Prestige = new PrestigeState();
            }

            return State.Prestige;
        }

        private EventState GetEventState()
        {
            if (State.Event == null)
            {
                State.Event = new EventState();
            }

            return State.Event;
        }

        private MonetizationState GetMonetizationState()
        {
            if (State.Monetization == null)
            {
                State.Monetization = new MonetizationState();
            }

            return State.Monetization;
        }

        private CosmeticState GetCosmeticState()
        {
            if (State.Cosmetics == null)
            {
                State.Cosmetics = CosmeticState.CreateDefault();
            }

            return State.Cosmetics;
        }

        private void EnsureStateDefaults()
        {
            if (State.Prestige == null)
            {
                State.Prestige = new PrestigeState();
            }

            if (State.Event == null)
            {
                State.Event = new EventState();
            }

            if (State.Monetization == null)
            {
                State.Monetization = new MonetizationState();
            }

            if (State.Cosmetics == null)
            {
                State.Cosmetics = CosmeticState.CreateDefault();
            }
        }

        private void UpdateEventTimers(double deltaSeconds)
        {
            EventState eventState = GetEventState();
            if (eventState.IsActive)
            {
                if (!ChaiEvents.TryGet(eventState.ActiveEventId, out ChaiEventDefinition activeEvent))
                {
                    eventState.ActiveEventId = null;
                    eventState.RemainingSeconds = 0;
                    return;
                }

                eventState.RemainingSeconds = Math.Max(0, eventState.RemainingSeconds - deltaSeconds);
                if (eventState.RemainingSeconds <= 0)
                {
                    eventState.ActiveEventId = null;
                    eventState.CooldownSeconds = activeEvent.CooldownSeconds;
                    eventState.CompletedCount++;
                }

                return;
            }

            eventState.CooldownSeconds = Math.Max(0, eventState.CooldownSeconds - deltaSeconds);
        }

        private double GetEventGlobalMultiplierBonus()
        {
            return GetActiveEventBonus(definition => definition.GlobalMultiplierBonus);
        }

        private double GetActiveEventBonus(Func<ChaiEventDefinition, double> selector)
        {
            if (TryGetActiveEvent(out ChaiEventDefinition definition))
            {
                return selector(definition);
            }

            return 0;
        }

        private void UpdateMonetizationTimers(double deltaSeconds)
        {
            MonetizationState monetization = GetMonetizationState();
            if (monetization.ProductionBoostRemainingSeconds > 0)
            {
                monetization.ProductionBoostRemainingSeconds = Math.Max(0, monetization.ProductionBoostRemainingSeconds - deltaSeconds);
                if (monetization.ProductionBoostRemainingSeconds <= 0)
                {
                    monetization.ProductionBoostCooldownSeconds = Math.Max(monetization.ProductionBoostCooldownSeconds, RewardedProductionBoostCooldownSeconds);
                }

                return;
            }

            monetization.ProductionBoostCooldownSeconds = Math.Max(0, monetization.ProductionBoostCooldownSeconds - deltaSeconds);
        }

        private static int ConvertLegacyToSkillPoints(BigDouble legacy)
        {
            double value = Math.Floor(legacy.ToDouble());
            if (double.IsNaN(value) || value <= 0)
            {
                return 0;
            }

            if (value >= int.MaxValue)
            {
                return int.MaxValue;
            }

            return (int)value;
        }

        private void Earn(BigDouble amount)
        {
            if (amount <= BigDouble.Zero)
            {
                return;
            }

            State.Rupees += amount;
            State.TotalLifetimeRupees += amount;
        }
    }

    public readonly struct OfflineReward
    {
        public OfflineReward(BigDouble rupeesEarned, double rawSeconds, double cappedSeconds, bool wasCapped)
        {
            RupeesEarned = rupeesEarned;
            RawSeconds = rawSeconds;
            CappedSeconds = cappedSeconds;
            WasCapped = wasCapped;
        }

        public BigDouble RupeesEarned { get; }
        public double RawSeconds { get; }
        public double CappedSeconds { get; }
        public bool WasCapped { get; }
    }

    public readonly struct PrestigePreview
    {
        public PrestigePreview(bool canPrestige, BigDouble projectedMasalaLegacy, string message)
        {
            CanPrestige = canPrestige;
            ProjectedMasalaLegacy = projectedMasalaLegacy;
            Message = message;
        }

        public bool CanPrestige { get; }
        public BigDouble ProjectedMasalaLegacy { get; }
        public string Message { get; }
    }

    public readonly struct PrestigeResult
    {
        public PrestigeResult(bool succeeded, BigDouble gainedMasalaLegacy, int gainedSkillPoints, string message)
        {
            Succeeded = succeeded;
            GainedMasalaLegacy = gainedMasalaLegacy;
            GainedSkillPoints = gainedSkillPoints;
            Message = message;
        }

        public bool Succeeded { get; }
        public BigDouble GainedMasalaLegacy { get; }
        public int GainedSkillPoints { get; }
        public string Message { get; }
    }
}
