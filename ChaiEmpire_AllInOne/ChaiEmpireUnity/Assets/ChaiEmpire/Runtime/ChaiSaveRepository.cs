using System;
using System.IO;
using UnityEngine;

namespace ChaiEmpire
{
    public static class ChaiSaveRepository
    {
        private const string SaveFileName = "chai-empire-save.json";

        public static LoadResult LoadOrCreate(ChaiContent content)
        {
            return LoadOrCreate(content, GetSavePath());
        }

        public static LoadResult LoadOrCreate(ChaiContent content, string path)
        {
            ChaiGameState state = LoadState(path, out bool recoveredFromCorruptSave, out string corruptSaveBackupPath);

            OfflineReward reward = default;
            bool hasReward = false;
            if (IsValidUtcTicks(state.LastSavedUtcTicks))
            {
                DateTime lastSaved = new DateTime(state.LastSavedUtcTicks, DateTimeKind.Utc);
                TimeSpan elapsed = DateTime.UtcNow - lastSaved;
                if (elapsed.TotalSeconds > 30)
                {
                    ChaiGame game = ChaiGame.FromState(content, state);
                    reward = game.ApplyOfflineProgress(elapsed);
                    hasReward = reward.RupeesEarned > 0;
                }
            }

            state.LastSavedUtcTicks = DateTime.UtcNow.Ticks;
            return new LoadResult(state, reward, hasReward, path, recoveredFromCorruptSave, corruptSaveBackupPath);
        }

        public static void Save(ChaiGameState state)
        {
            Save(state, GetSavePath());
        }

        public static void Save(ChaiGameState state, string path)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            state.LastSavedUtcTicks = DateTime.UtcNow.Ticks;
            File.WriteAllText(path, ChaiSaveCodec.ToJson(state));
        }

        public static bool DeleteSave()
        {
            return DeleteSave(GetSavePath());
        }

        public static bool DeleteSave(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return false;
                }

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                return true;
            }
            catch (Exception exception) when (IsRecoverableFileException(exception))
            {
                return false;
            }
        }

        public static string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, SaveFileName);
        }

        private static ChaiGameState LoadState(string path, out bool recoveredFromCorruptSave, out string corruptSaveBackupPath)
        {
            recoveredFromCorruptSave = false;
            corruptSaveBackupPath = null;

            if (!File.Exists(path))
            {
                return ChaiGameState.CreateNew();
            }

            try
            {
                string json = File.ReadAllText(path);
                if (ChaiSaveCodec.TryFromJson(json, out ChaiGameState state))
                {
                    return state;
                }
            }
            catch (Exception exception) when (IsRecoverableFileException(exception))
            {
            }

            recoveredFromCorruptSave = true;
            corruptSaveBackupPath = TryBackupCorruptSave(path);
            return ChaiGameState.CreateNew();
        }

        private static string TryBackupCorruptSave(string path)
        {
            try
            {
                string backupPath = CreateUniqueCorruptBackupPath(path);
                File.Move(path, backupPath);
                return backupPath;
            }
            catch (Exception exception) when (IsRecoverableFileException(exception))
            {
                return null;
            }
        }

        private static string CreateUniqueCorruptBackupPath(string path)
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string candidate = path + ".corrupt-" + timestamp + ".bak";
            int suffix = 1;

            while (File.Exists(candidate))
            {
                candidate = path + ".corrupt-" + timestamp + "-" + suffix + ".bak";
                suffix++;
            }

            return candidate;
        }

        private static bool IsValidUtcTicks(long ticks)
        {
            return ticks > 0 && ticks <= DateTime.MaxValue.Ticks;
        }

        private static bool IsRecoverableFileException(Exception exception)
        {
            return exception is IOException ||
                exception is UnauthorizedAccessException ||
                exception is ArgumentException ||
                exception is NotSupportedException ||
                exception is System.Security.SecurityException;
        }
    }

    public readonly struct LoadResult
    {
        public LoadResult(
            ChaiGameState state,
            OfflineReward offlineReward,
            bool hasOfflineReward,
            string savePath,
            bool recoveredFromCorruptSave,
            string corruptSaveBackupPath)
        {
            State = state;
            OfflineReward = offlineReward;
            HasOfflineReward = hasOfflineReward;
            SavePath = savePath;
            RecoveredFromCorruptSave = recoveredFromCorruptSave;
            CorruptSaveBackupPath = corruptSaveBackupPath;
        }

        public ChaiGameState State { get; }
        public OfflineReward OfflineReward { get; }
        public bool HasOfflineReward { get; }
        public string SavePath { get; }
        public bool RecoveredFromCorruptSave { get; }
        public string CorruptSaveBackupPath { get; }
    }
}
