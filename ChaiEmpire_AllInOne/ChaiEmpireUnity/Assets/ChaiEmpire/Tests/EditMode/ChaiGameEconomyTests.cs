using System;
using System.IO;
using BreakInfinity;
using NUnit.Framework;

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
    }
}
