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
