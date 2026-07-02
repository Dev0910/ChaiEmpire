using System;
using System.Collections.Generic;
using System.IO;
using BreakInfinity;
using NUnit.Framework;
using UnityEngine;

namespace ChaiEmpire.Tests
{
    public sealed class ChaiGameEconomyTests
    {
        [Test]
        public void Manual_taps_scale_with_brew_upgrades_and_legacy_bonus()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            ChaiGame game = ChaiGame.NewGame(content);

            game.TapKettle(10);

            Assert.That(game.State.Rupees.ToDouble(), Is.EqualTo(10d).Within(0.001d));
            Assert.That(game.State.ChaiServed.ToDouble(), Is.EqualTo(10d).Within(0.001d));

            Assert.That(game.TryBuyUpgrade("strong-tea"), Is.True);
            game.State.Prestige.MasalaLegacy = new BigDouble(5);
            game.TapKettle();

            Assert.That(game.State.Rupees.ToDouble(), Is.EqualTo(2.1d).Within(0.001d));
            Assert.That(game.GetTapValue().ToDouble(), Is.EqualTo(2.1d).Within(0.001d));
        }

        [Test]
        public void Automation_and_location_unlocks_create_meaningful_passive_income()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            ChaiGame game = ChaiGame.NewGame(content);
            game.State.Rupees = new BigDouble(600);

            Assert.That(game.TryBuyUpgrade("helper-boy"), Is.True);
            Assert.That(game.TryBuyUpgrade("bulk-kettle"), Is.True);
            Assert.That(game.TryUnlockLocation("bus-stand"), Is.True);

            BigDouble before = game.State.Rupees;
            game.Tick(10d);

            Assert.That((game.State.Rupees - before).ToDouble(), Is.EqualTo(31.25d).Within(0.001d));
            Assert.That(game.GetPassiveRupeesPerSecond().ToDouble(), Is.EqualTo(3.125d).Within(0.001d));
        }

        [Test]
        public void Offline_progress_is_capped_and_uses_current_passive_rate()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            ChaiGame game = ChaiGame.NewGame(content);
            game.State.Rupees = new BigDouble(600);
            game.TryBuyUpgrade("helper-boy");
            game.TryBuyUpgrade("bulk-kettle");

            OfflineReward reward = game.ApplyOfflineProgress(TimeSpan.FromHours(12));

            Assert.That(reward.CappedSeconds, Is.EqualTo(28800d).Within(0.001d));
            Assert.That(reward.WasCapped, Is.True);
            Assert.That(reward.RupeesEarned.ToDouble(), Is.EqualTo(54000d).Within(0.001d));
            Assert.That(game.State.Rupees.ToDouble(), Is.EqualTo(54370d).Within(0.001d));
        }

        [Test]
        public void Events_apply_temporary_multipliers_and_rotate_after_cooldown()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            ChaiGame game = ChaiGame.NewGame(content);
            ChaiEventDefinition firstEvent = game.GetNextEventDefinition();

            Assert.That(firstEvent.Id, Is.EqualTo("monsoon-chai-rush"));
            Assert.That(game.TryStartEvent(firstEvent.Id), Is.True);
            Assert.That(game.GetTapValue().ToDouble(), Is.EqualTo(1.5d).Within(0.001d));

            game.State.SetUpgradeLevel("helper-boy", 1);
            Assert.That(game.GetPassiveRupeesPerSecond(includeRush: false).ToDouble(), Is.EqualTo(0.625d).Within(0.001d));

            game.Tick(firstEvent.DurationSeconds);

            Assert.That(game.State.Event.IsActive, Is.False);
            Assert.That(game.State.Event.CooldownSeconds, Is.EqualTo(firstEvent.CooldownSeconds).Within(0.001d));
            Assert.That(game.State.Event.CompletedCount, Is.EqualTo(1));
            Assert.That(game.GetNextEventDefinition().Id, Is.EqualTo("diwali-sweet-combo"));
            Assert.That(game.TryStartEvent(game.GetNextEventDefinition().Id), Is.False);

            game.Tick(firstEvent.CooldownSeconds);

            ChaiEventDefinition secondEvent = game.GetNextEventDefinition();
            Assert.That(game.TryStartEvent(secondEvent.Id), Is.True);
            Assert.That(game.GetTapValue().ToDouble(), Is.EqualTo(1.2d).Within(0.001d));
            Assert.That(game.GetPassiveRupeesPerSecond(includeRush: false).ToDouble(), Is.EqualTo(0.75d).Within(0.001d));
        }

        [Test]
        public void Optional_rewarded_production_boost_is_timed_and_cooldown_gated()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            ChaiGame game = ChaiGame.NewGame(content);
            game.State.SetUpgradeLevel("helper-boy", 1);

            Assert.That(game.GetProductionBoostMultiplier(), Is.EqualTo(1d).Within(0.001d));
            Assert.That(game.TryStartRewardedProductionBoost(), Is.True);
            Assert.That(game.GetProductionBoostMultiplier(), Is.EqualTo(2d).Within(0.001d));
            Assert.That(game.GetTapValue().ToDouble(), Is.EqualTo(2d).Within(0.001d));
            Assert.That(game.GetPassiveRupeesPerSecond(includeRush: false).ToDouble(), Is.EqualTo(1d).Within(0.001d));

            game.Tick(120d);

            Assert.That(game.GetProductionBoostMultiplier(), Is.EqualTo(1d).Within(0.001d));
            Assert.That(game.State.Monetization.ProductionBoostCooldownSeconds, Is.EqualTo(600d).Within(0.001d));
            Assert.That(game.TryStartRewardedProductionBoost(), Is.False);

            game.Tick(600d);

            Assert.That(game.State.Monetization.ProductionBoostCooldownSeconds, Is.EqualTo(0d).Within(0.001d));
            Assert.That(game.TryStartRewardedProductionBoost(), Is.True);
        }

        [Test]
        public void Optional_offline_sponsor_bonus_adds_extra_reward_without_blocking_claim()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            ChaiGame game = ChaiGame.NewGame(content);

            Assert.That(game.TryClaimRewardedOfflineBonus(new BigDouble(250)), Is.True);
            Assert.That(game.State.Rupees.ToDouble(), Is.EqualTo(250d).Within(0.001d));
            Assert.That(game.State.TotalLifetimeRupees.ToDouble(), Is.EqualTo(250d).Within(0.001d));
            Assert.That(game.State.Monetization.RewardedOfflineBonusClaims, Is.EqualTo(1));
            Assert.That(game.TryClaimRewardedOfflineBonus(BigDouble.Zero), Is.False);
        }

        [Test]
        public void Cosmetic_and_no_ads_choices_save_without_affecting_income()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            ChaiGame game = ChaiGame.NewGame(content);

            Assert.That(game.TryPurchaseNoAds(), Is.True);
            Assert.That(game.TryPurchaseNoAds(), Is.False);
            Assert.That(game.TrySelectStallTheme("festival-lights"), Is.True);
            Assert.That(game.TrySelectCupPack("steel-glasses"), Is.True);
            Assert.That(game.TrySelectSignboardPack("neon-board"), Is.True);
            Assert.That(game.GetTapValue().ToDouble(), Is.EqualTo(1d).Within(0.001d));

            ChaiGameState restored = ChaiSaveCodec.FromJson(ChaiSaveCodec.ToJson(game.State));
            ChaiGame restoredGame = ChaiGame.FromState(content, restored);

            Assert.That(restored.Monetization.NoAdsPurchased, Is.True);
            Assert.That(restored.Cosmetics.StallThemeId, Is.EqualTo("festival-lights"));
            Assert.That(restored.Cosmetics.CupPackId, Is.EqualTo("steel-glasses"));
            Assert.That(restored.Cosmetics.SignboardPackId, Is.EqualTo("neon-board"));
            Assert.That(restoredGame.GetTapValue().ToDouble(), Is.EqualTo(1d).Within(0.001d));
        }

        [Test]
        public void Non_paying_player_can_progress_without_optional_monetization()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            ChaiGame game = ChaiGame.NewGame(content);

            game.TapKettle(10);

            Assert.That(game.TryBuyUpgrade("strong-tea"), Is.True);
            Assert.That(game.State.Monetization.NoAdsPurchased, Is.False);
            Assert.That(game.State.Monetization.RewardedOfflineBonusClaims, Is.EqualTo(0));
            Assert.That(game.GetProductionBoostMultiplier(), Is.EqualTo(1d).Within(0.001d));
            Assert.That(game.GetTapValue().ToDouble(), Is.EqualTo(2d).Within(0.001d));
        }

        [Test]
        public void Early_balance_reaches_first_upgrade_and_first_automation_in_target_window()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            ChaiGame game = ChaiGame.NewGame(content);
            const double CasualSecondsPerTap = 6d;

            double elapsedSeconds = 0;
            double? firstUpgradeSeconds = null;
            double? firstAutomationSeconds = null;

            while (elapsedSeconds < 300d && !firstAutomationSeconds.HasValue)
            {
                game.TapKettle();
                elapsedSeconds += CasualSecondsPerTap;

                if (!firstUpgradeSeconds.HasValue && game.TryBuyUpgrade("strong-tea"))
                {
                    firstUpgradeSeconds = elapsedSeconds;
                    continue;
                }

                if (game.State.GetUpgradeLevel("strong-tea") > 0 && game.TryBuyUpgrade("helper-boy"))
                {
                    firstAutomationSeconds = elapsedSeconds;
                }
            }

            Assert.That(firstUpgradeSeconds.HasValue, Is.True);
            Assert.That(firstUpgradeSeconds.Value, Is.LessThanOrEqualTo(60d));
            Assert.That(firstAutomationSeconds.HasValue, Is.True);
            Assert.That(firstAutomationSeconds.Value, Is.InRange(180d, 300d));
            Assert.That(game.GetPassiveRupeesPerSecond().ToDouble(), Is.EqualTo(0.5d).Within(0.001d));
        }

        [Test]
        public void Number_formatter_outputs_readable_rupees_suffixes_rates_and_large_fallback()
        {
            Assert.That(ChaiNumberFormatter.Compact(new BigDouble(999.5)), Is.EqualTo("999.5"));
            Assert.That(ChaiNumberFormatter.Rupees(new BigDouble(1500)), Is.EqualTo("Rs 1.5K"));
            Assert.That(ChaiNumberFormatter.Compact(new BigDouble(-2500000)), Is.EqualTo("-2.5M"));
            Assert.That(ChaiNumberFormatter.PerSecond(new BigDouble(12.5)), Is.EqualTo("Rs 12.5/sec"));
            Assert.That(ChaiNumberFormatter.Compact(new BigDouble(1.234, 39)), Is.EqualTo("1.234e39"));
        }

        [Test]
        public void Default_upgrade_catalog_has_unique_valid_progression_values()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            HashSet<string> ids = new HashSet<string>();
            bool hasAutomation = false;

            foreach (UpgradeDefinition upgrade in content.Upgrades)
            {
                Assert.That(upgrade.Id, Is.Not.Empty);
                Assert.That(ids.Add(upgrade.Id), Is.True, "Duplicate upgrade id: " + upgrade.Id);
                Assert.That(upgrade.DisplayName, Is.Not.Empty);
                Assert.That(upgrade.Category, Is.Not.Empty);
                Assert.That(upgrade.BaseCost > BigDouble.Zero, Is.True, upgrade.Id);
                Assert.That(upgrade.CostMultiplier, Is.GreaterThan(1d), upgrade.Id);
                Assert.That(upgrade.ValuePerLevel, Is.GreaterThan(0d), upgrade.Id);

                if (upgrade.IsAutomation)
                {
                    hasAutomation = true;
                    Assert.That(upgrade.Kind, Is.EqualTo(UpgradeKind.PassiveFlat), upgrade.Id);
                }
            }

            Assert.That(hasAutomation, Is.True);
        }

        [Test]
        public void Default_location_catalog_has_one_start_and_ordered_unlock_progression()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            HashSet<string> ids = new HashSet<string>();
            BigDouble previousCost = new BigDouble(-1);
            double previousDemand = 0;
            int defaultCount = 0;
            string defaultLocationId = string.Empty;

            foreach (LocationDefinition location in content.Locations)
            {
                Assert.That(location.Id, Is.Not.Empty);
                Assert.That(ids.Add(location.Id), Is.True, "Duplicate location id: " + location.Id);
                Assert.That(location.DisplayName, Is.Not.Empty);
                Assert.That(location.UnlockCost >= BigDouble.Zero, Is.True, location.Id);
                Assert.That(location.DemandMultiplier, Is.GreaterThanOrEqualTo(1d), location.Id);
                Assert.That(location.UnlockCost >= previousCost, Is.True, location.Id);
                Assert.That(location.DemandMultiplier >= previousDemand, Is.True, location.Id);

                if (location.UnlockedByDefault)
                {
                    defaultCount++;
                    defaultLocationId = location.Id;
                    Assert.That(location.UnlockCost == BigDouble.Zero, Is.True);
                }

                previousCost = location.UnlockCost;
                previousDemand = location.DemandMultiplier;
            }

            Assert.That(defaultCount, Is.EqualTo(1));
            Assert.That(defaultLocationId, Is.EqualTo("gali-tapri"));
        }

        [Test]
        public void Default_content_json_parses_and_matches_built_in_catalog()
        {
            TextAsset contentAsset = Resources.Load<TextAsset>("ChaiEmpire/default-content");
            Assert.That(contentAsset, Is.Not.Null);

            Assert.That(ChaiContentData.TryFromJson(contentAsset.text, out ChaiContentData data, out string error), Is.True, error);

            ChaiContent jsonContent = data.ToContent();
            ChaiContent builtInContent = ChaiContent.CreateBuiltInDefault();

            AssertContentMatches(jsonContent, builtInContent);
            Assert.That(jsonContent.GetUpgrade("strong-tea").BaseCost.ToDouble(), Is.EqualTo(10d).Within(0.001d));
            Assert.That(jsonContent.GetLocation("gali-tapri").UnlockedByDefault, Is.True);
        }

        [Test]
        public void Content_validator_rejects_duplicate_ids_and_invalid_values()
        {
            ChaiContentData data = ChaiContentData.CreateBuiltInDefault();
            data.offlineEfficiency = 2d;
            data.prestigeUnlockRupees = "not-a-number";
            data.upgrades[1].id = data.upgrades[0].id;
            data.locations[1].id = data.locations[0].id;

            IReadOnlyList<string> errors = ChaiContentValidator.Validate(data);
            string joinedErrors = string.Join("; ", errors);

            Assert.That(joinedErrors, Does.Contain("Offline efficiency"));
            Assert.That(joinedErrors, Does.Contain("Prestige unlock rupees"));
            Assert.That(joinedErrors, Does.Contain("Duplicate upgrade id"));
            Assert.That(joinedErrors, Does.Contain("Duplicate location id"));
        }

        [Test]
        public void Save_round_trip_preserves_v1_and_future_prestige_fields()
        {
            ChaiGameState state = ChaiGameState.CreateNew();
            state.Rupees = new BigDouble(1234);
            state.TotalLifetimeRupees = new BigDouble(987654);
            state.ChaiServed = new BigDouble(321);
            state.LastSavedUtcTicks = new DateTime(2026, 7, 3, 10, 30, 0, DateTimeKind.Utc).Ticks;
            state.Prestige.MasalaLegacy = new BigDouble(7);
            state.Prestige.UnspentSkillPoints = 4;
            state.Prestige.SetSkillLevel("brew-stronger-start", 2);
            state.SetUpgradeLevel("helper-boy", 3);
            state.SetUpgradeLevel("strong-tea", 2);
            state.UnlockLocation("railway-platform");

            string json = ChaiSaveCodec.ToJson(state);
            ChaiGameState restored = ChaiSaveCodec.FromJson(json);

            Assert.That(restored.SaveVersion, Is.EqualTo(1));
            Assert.That(restored.Rupees.ToDouble(), Is.EqualTo(1234d).Within(0.001d));
            Assert.That(restored.TotalLifetimeRupees.ToDouble(), Is.EqualTo(987654d).Within(0.001d));
            Assert.That(restored.ChaiServed.ToDouble(), Is.EqualTo(321d).Within(0.001d));
            Assert.That(restored.Prestige.MasalaLegacy.ToDouble(), Is.EqualTo(7d).Within(0.001d));
            Assert.That(restored.Prestige.UnspentSkillPoints, Is.EqualTo(4));
            Assert.That(restored.Prestige.GetSkillLevel("brew-stronger-start"), Is.EqualTo(2));
            Assert.That(restored.GetUpgradeLevel("helper-boy"), Is.EqualTo(3));
            Assert.That(restored.GetUpgradeLevel("strong-tea"), Is.EqualTo(2));
            Assert.That(restored.IsLocationUnlocked("railway-platform"), Is.True);
        }

        [Test]
        public void Save_round_trip_preserves_high_exponent_big_numbers()
        {
            ChaiGameState state = ChaiGameState.CreateNew();
            state.Rupees = new BigDouble(1.23456789, 500);
            state.TotalLifetimeRupees = new BigDouble(9.87654321, 750);

            string json = ChaiSaveCodec.ToJson(state);
            ChaiGameState restored = ChaiSaveCodec.FromJson(json);

            Assert.That(restored.Rupees.Mantissa, Is.EqualTo(1.23456789d).Within(0.00000001d));
            Assert.That(restored.Rupees.Exponent, Is.EqualTo(500));
            Assert.That(restored.TotalLifetimeRupees.Mantissa, Is.EqualTo(9.87654321d).Within(0.00000001d));
            Assert.That(restored.TotalLifetimeRupees.Exponent, Is.EqualTo(750));
        }

        [Test]
        public void Event_save_fields_round_trip_active_and_cooldown_state()
        {
            ChaiGameState activeState = ChaiGameState.CreateNew();
            activeState.Event.ActiveEventId = "cricket-match-night";
            activeState.Event.RemainingSeconds = 123;
            activeState.Event.CompletedCount = 2;

            ChaiGameState restoredActive = ChaiSaveCodec.FromJson(ChaiSaveCodec.ToJson(activeState));

            Assert.That(restoredActive.Event.ActiveEventId, Is.EqualTo("cricket-match-night"));
            Assert.That(restoredActive.Event.RemainingSeconds, Is.EqualTo(123d).Within(0.001d));
            Assert.That(restoredActive.Event.CooldownSeconds, Is.EqualTo(0d).Within(0.001d));
            Assert.That(restoredActive.Event.CompletedCount, Is.EqualTo(2));

            ChaiGameState cooldownState = ChaiGameState.CreateNew();
            cooldownState.Event.CooldownSeconds = 456;
            cooldownState.Event.CompletedCount = 1;

            ChaiGameState restoredCooldown = ChaiSaveCodec.FromJson(ChaiSaveCodec.ToJson(cooldownState));

            Assert.That(restoredCooldown.Event.ActiveEventId, Is.Null);
            Assert.That(restoredCooldown.Event.RemainingSeconds, Is.EqualTo(0d).Within(0.001d));
            Assert.That(restoredCooldown.Event.CooldownSeconds, Is.EqualTo(456d).Within(0.001d));
            Assert.That(restoredCooldown.Event.CompletedCount, Is.EqualTo(1));
        }

        [Test]
        public void Repository_load_backs_up_malformed_save_and_starts_new_game()
        {
            string savePath = CreateTempSavePath();

            try
            {
                File.WriteAllText(savePath, "this is not json");

                LoadResult result = ChaiSaveRepository.LoadOrCreate(ChaiContent.CreateDefault(), savePath);

                Assert.That(result.RecoveredFromCorruptSave, Is.True);
                Assert.That(result.CorruptSaveBackupPath, Is.Not.Null);
                Assert.That(File.Exists(result.CorruptSaveBackupPath), Is.True);
                Assert.That(File.ReadAllText(result.CorruptSaveBackupPath), Is.EqualTo("this is not json"));
                Assert.That(File.Exists(savePath), Is.False);
                Assert.That(result.HasOfflineReward, Is.False);
                Assert.That(result.State.Rupees.ToDouble(), Is.EqualTo(0d).Within(0.001d));
                Assert.That(result.State.IsLocationUnlocked("gali-tapri"), Is.True);
            }
            finally
            {
                DeleteTempSaveDirectory(savePath);
            }
        }

        [Test]
        public void Repository_load_backs_up_invalid_number_save_and_starts_new_game()
        {
            string savePath = CreateTempSavePath();

            try
            {
                File.WriteAllText(savePath, "{\"saveVersion\":1,\"rupees\":\"spilled-chai\",\"unlockedLocations\":[{\"id\":\"airport-lounge\"}]}");

                LoadResult result = ChaiSaveRepository.LoadOrCreate(ChaiContent.CreateDefault(), savePath);

                Assert.That(result.RecoveredFromCorruptSave, Is.True);
                Assert.That(File.Exists(result.CorruptSaveBackupPath), Is.True);
                Assert.That(result.State.Rupees.ToDouble(), Is.EqualTo(0d).Within(0.001d));
                Assert.That(result.State.IsLocationUnlocked("airport-lounge"), Is.False);
                Assert.That(result.State.IsLocationUnlocked("gali-tapri"), Is.True);
            }
            finally
            {
                DeleteTempSaveDirectory(savePath);
            }
        }

        [Test]
        public void Repository_load_ignores_invalid_saved_ticks_without_resetting_valid_state()
        {
            string savePath = CreateTempSavePath();

            try
            {
                ChaiGameState state = ChaiGameState.CreateNew();
                state.Rupees = new BigDouble(250);
                state.LastSavedUtcTicks = long.MaxValue;
                File.WriteAllText(savePath, ChaiSaveCodec.ToJson(state));

                LoadResult result = ChaiSaveRepository.LoadOrCreate(ChaiContent.CreateDefault(), savePath);

                Assert.That(result.RecoveredFromCorruptSave, Is.False);
                Assert.That(result.HasOfflineReward, Is.False);
                Assert.That(result.State.Rupees.ToDouble(), Is.EqualTo(250d).Within(0.001d));
                Assert.That(result.State.LastSavedUtcTicks, Is.InRange(1, DateTime.MaxValue.Ticks));
            }
            finally
            {
                DeleteTempSaveDirectory(savePath);
            }
        }

        [Test]
        public void Repository_delete_save_removes_existing_file_and_tolerates_missing_file()
        {
            string savePath = CreateTempSavePath();

            try
            {
                File.WriteAllText(savePath, "{\"saveVersion\":1}");

                Assert.That(ChaiSaveRepository.DeleteSave(savePath), Is.True);
                Assert.That(File.Exists(savePath), Is.False);
                Assert.That(ChaiSaveRepository.DeleteSave(savePath), Is.True);
            }
            finally
            {
                DeleteTempSaveDirectory(savePath);
            }
        }

        [Test]
        public void Tutorial_prompts_first_tap_on_fresh_state()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            ChaiGame game = ChaiGame.NewGame(content);

            ChaiTutorialPrompt prompt = ChaiTutorial.GetPrompt(game.State, content);

            Assert.That(prompt.ShouldShow, Is.True);
            Assert.That(prompt.Step, Is.EqualTo(ChaiTutorialStep.FirstTap));
            Assert.That(prompt.PrimaryAction, Is.EqualTo(ChaiTutorialAction.TapKettle));
            Assert.That(prompt.PrimaryButtonLabel, Is.EqualTo("Tap Kettle"));
        }

        [Test]
        public void Tutorial_guides_player_toward_first_upgrade_after_first_tap()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            ChaiGame game = ChaiGame.NewGame(content);

            game.TapKettle();

            ChaiTutorialPrompt prompt = ChaiTutorial.GetPrompt(game.State, content);

            Assert.That(prompt.ShouldShow, Is.True);
            Assert.That(prompt.Step, Is.EqualTo(ChaiTutorialStep.SaveForFirstUpgrade));
            Assert.That(prompt.PrimaryAction, Is.EqualTo(ChaiTutorialAction.TapKettle));
            Assert.That(prompt.Progress, Is.EqualTo("Rs 1 / Rs 10"));
        }

        [Test]
        public void Tutorial_switches_to_buy_prompt_when_first_upgrade_is_affordable()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            ChaiGame game = ChaiGame.NewGame(content);
            game.TapKettle();
            game.State.Rupees = new BigDouble(10);

            ChaiTutorialPrompt prompt = ChaiTutorial.GetPrompt(game.State, content);

            Assert.That(prompt.ShouldShow, Is.True);
            Assert.That(prompt.Step, Is.EqualTo(ChaiTutorialStep.BuyFirstUpgrade));
            Assert.That(prompt.PrimaryAction, Is.EqualTo(ChaiTutorialAction.BuyFirstUpgrade));
            Assert.That(prompt.PrimaryButtonLabel, Is.EqualTo("Buy Strong Tea"));
        }

        [Test]
        public void Tutorial_completes_after_first_upgrade_purchase()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            ChaiGame game = ChaiGame.NewGame(content);
            game.State.Rupees = new BigDouble(10);

            Assert.That(game.TryBuyUpgrade(ChaiTutorial.FirstUpgradeId), Is.True);

            ChaiTutorialPrompt prompt = ChaiTutorial.GetPrompt(game.State, content);

            Assert.That(prompt.ShouldShow, Is.False);
            Assert.That(prompt.Step, Is.EqualTo(ChaiTutorialStep.Complete));
            Assert.That(prompt.PrimaryAction, Is.EqualTo(ChaiTutorialAction.None));
        }

        [Test]
        public void Prestige_preview_stays_locked_until_first_empire_arc_is_complete()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            ChaiGame game = ChaiGame.NewGame(content);

            PrestigePreview locked = game.GetPrestigePreview();

            Assert.That(locked.CanPrestige, Is.False);
            Assert.That(locked.ProjectedMasalaLegacy.ToDouble(), Is.EqualTo(0d).Within(0.001d));

            game.State.TotalLifetimeRupees = new BigDouble(1_000_000_000d);
            game.State.UnlockLocation("airport-lounge");

            PrestigePreview unlocked = game.GetPrestigePreview();

            Assert.That(unlocked.CanPrestige, Is.True);
            Assert.That(unlocked.ProjectedMasalaLegacy.ToDouble(), Is.GreaterThanOrEqualTo(10d));
        }

        [Test]
        public void Prestige_reset_adds_legacy_points_and_preserves_skill_tree()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            ChaiGameState state = ChaiGameState.CreateNew();
            state.Rupees = new BigDouble(12345);
            state.TotalLifetimeRupees = new BigDouble(1_000_000_000d);
            state.ChaiServed = new BigDouble(777);
            state.RushRemainingSeconds = 12;
            state.RushCooldownSeconds = 45;
            state.Prestige.MasalaLegacy = new BigDouble(5);
            state.Prestige.UnspentSkillPoints = 2;
            state.Prestige.SetSkillLevel("brew-stronger-start", 2);
            state.SetUpgradeLevel("helper-boy", 3);
            state.UnlockLocation("airport-lounge");
            ChaiGame game = ChaiGame.FromState(content, state);

            Assert.That(game.TryPrestige(out PrestigeResult result), Is.True);

            Assert.That(result.GainedMasalaLegacy.ToDouble(), Is.EqualTo(10d).Within(0.001d));
            Assert.That(result.GainedSkillPoints, Is.EqualTo(10));
            Assert.That(game.State.Rupees.ToDouble(), Is.EqualTo(0d).Within(0.001d));
            Assert.That(game.State.TotalLifetimeRupees.ToDouble(), Is.EqualTo(0d).Within(0.001d));
            Assert.That(game.State.ChaiServed.ToDouble(), Is.EqualTo(0d).Within(0.001d));
            Assert.That(game.State.GetUpgradeLevel("helper-boy"), Is.EqualTo(0));
            Assert.That(game.State.IsLocationUnlocked("gali-tapri"), Is.True);
            Assert.That(game.State.IsLocationUnlocked("airport-lounge"), Is.False);
            Assert.That(game.State.RushRemainingSeconds, Is.EqualTo(0d).Within(0.001d));
            Assert.That(game.State.RushCooldownSeconds, Is.EqualTo(0d).Within(0.001d));
            Assert.That(game.State.Prestige.MasalaLegacy.ToDouble(), Is.EqualTo(15d).Within(0.001d));
            Assert.That(game.State.Prestige.UnspentSkillPoints, Is.EqualTo(12));
            Assert.That(game.State.Prestige.GetSkillLevel("brew-stronger-start"), Is.EqualTo(2));
        }

        [Test]
        public void Prestige_skill_spending_applies_formula_and_cost_effects()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            ChaiGame game = ChaiGame.NewGame(content);
            game.State.Prestige.UnspentSkillPoints = 5;

            Assert.That(game.TrySpendSkillPoint("brew-stronger-start"), Is.True);
            Assert.That(game.TrySpendSkillPoint("ops-helper-training"), Is.True);
            Assert.That(game.TrySpendSkillPoint("supply-bulk-buying"), Is.True);
            Assert.That(game.TrySpendSkillPoint("brand-loyal-regulars"), Is.True);
            Assert.That(game.TrySpendSkillPoint("expand-cheaper-locations"), Is.True);

            Assert.That(game.State.Prestige.UnspentSkillPoints, Is.EqualTo(0));
            Assert.That(game.GetTapValue().ToDouble(), Is.EqualTo(1.26d).Within(0.001d));
            game.State.SetUpgradeLevel("helper-boy", 1);
            Assert.That(game.GetPassiveRupeesPerSecond(includeRush: false).ToDouble(), Is.EqualTo(0.5775d).Within(0.001d));
            Assert.That(game.GetUpgradeCost("strong-tea").ToDouble(), Is.EqualTo(9.8d).Within(0.001d));
            Assert.That(game.GetLocationUnlockCost("bus-stand").ToDouble(), Is.EqualTo(242.5d).Within(0.001d));
        }

        [Test]
        public void Prestige_offline_and_rush_skills_apply_to_timers_and_rewards()
        {
            ChaiContent content = ChaiContent.CreateDefault();
            ChaiGame game = ChaiGame.NewGame(content);
            game.State.Prestige.UnspentSkillPoints = 4;

            Assert.That(game.TrySpendSkillPoint("supply-offline-flask"), Is.True);
            Assert.That(game.TrySpendSkillPoint("supply-long-storage"), Is.True);
            Assert.That(game.TrySpendSkillPoint("ops-fast-rush"), Is.True);
            Assert.That(game.TrySpendSkillPoint("brew-rush-taps"), Is.True);

            game.State.SetUpgradeLevel("helper-boy", 1);
            OfflineReward reward = game.ApplyOfflineProgress(TimeSpan.FromHours(10));

            Assert.That(game.GetOfflineEfficiency(), Is.EqualTo(0.8d).Within(0.001d));
            Assert.That(game.GetOfflineCapSeconds(), Is.EqualTo(32400d).Within(0.001d));
            Assert.That(reward.CappedSeconds, Is.EqualTo(32400d).Within(0.001d));
            Assert.That(reward.RupeesEarned.ToDouble(), Is.EqualTo(12960d).Within(0.001d));

            game.State.RushCooldownSeconds = 0;
            Assert.That(game.TryTriggerRushHour(), Is.True);
            Assert.That(game.State.RushCooldownSeconds, Is.EqualTo(85d).Within(0.001d));
            Assert.That(game.GetTapValue().ToDouble(), Is.EqualTo(2.2d).Within(0.001d));
        }

        private static string CreateTempSavePath()
        {
            string directory = Path.Combine(Path.GetTempPath(), "ChaiEmpireTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, "chai-empire-save.json");
        }

        private static void DeleteTempSaveDirectory(string savePath)
        {
            string directory = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        private static void AssertContentMatches(ChaiContent actual, ChaiContent expected)
        {
            Assert.That(actual.OfflineEfficiency, Is.EqualTo(expected.OfflineEfficiency).Within(0.001d));
            Assert.That(actual.OfflineCapSeconds, Is.EqualTo(expected.OfflineCapSeconds).Within(0.001d));
            Assert.That(actual.PrestigeUnlockRupees.ToDouble(), Is.EqualTo(expected.PrestigeUnlockRupees.ToDouble()).Within(0.001d));
            Assert.That(actual.Upgrades.Count, Is.EqualTo(expected.Upgrades.Count));
            Assert.That(actual.Locations.Count, Is.EqualTo(expected.Locations.Count));

            for (int i = 0; i < expected.Upgrades.Count; i++)
            {
                UpgradeDefinition actualUpgrade = actual.Upgrades[i];
                UpgradeDefinition expectedUpgrade = expected.Upgrades[i];
                Assert.That(actualUpgrade.Id, Is.EqualTo(expectedUpgrade.Id));
                Assert.That(actualUpgrade.DisplayName, Is.EqualTo(expectedUpgrade.DisplayName));
                Assert.That(actualUpgrade.Kind, Is.EqualTo(expectedUpgrade.Kind));
                Assert.That(actualUpgrade.BaseCost.ToDouble(), Is.EqualTo(expectedUpgrade.BaseCost.ToDouble()).Within(0.001d));
                Assert.That(actualUpgrade.CostMultiplier, Is.EqualTo(expectedUpgrade.CostMultiplier).Within(0.001d));
                Assert.That(actualUpgrade.ValuePerLevel, Is.EqualTo(expectedUpgrade.ValuePerLevel).Within(0.001d));
                Assert.That(actualUpgrade.IsAutomation, Is.EqualTo(expectedUpgrade.IsAutomation));
            }

            for (int i = 0; i < expected.Locations.Count; i++)
            {
                LocationDefinition actualLocation = actual.Locations[i];
                LocationDefinition expectedLocation = expected.Locations[i];
                Assert.That(actualLocation.Id, Is.EqualTo(expectedLocation.Id));
                Assert.That(actualLocation.DisplayName, Is.EqualTo(expectedLocation.DisplayName));
                Assert.That(actualLocation.UnlockCost.ToDouble(), Is.EqualTo(expectedLocation.UnlockCost.ToDouble()).Within(0.001d));
                Assert.That(actualLocation.DemandMultiplier, Is.EqualTo(expectedLocation.DemandMultiplier).Within(0.001d));
                Assert.That(actualLocation.UnlockedByDefault, Is.EqualTo(expectedLocation.UnlockedByDefault));
            }
        }
    }
}
