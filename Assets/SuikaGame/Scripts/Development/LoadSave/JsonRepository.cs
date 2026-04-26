using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Development.LoadSave.Data;
using Development.Utils;
using UnityEngine;

namespace Development.LoadSave
{
    public static class JsonRepository
    {
        private const string SaveDirectoryName = "SaveData";
        private const string PlayerProfileFileName = "player_profile.json";
        private const string GameProgressFileName = "game_progress.json";

        private static readonly string SaveDirectoryPath = Path.Combine(Application.persistentDataPath, SaveDirectoryName);
        private static readonly string PlayerProfilePath = Path.Combine(SaveDirectoryPath, PlayerProfileFileName);
        private static readonly string GameProgressPath = Path.Combine(SaveDirectoryPath, GameProgressFileName);

        public static string ProfileFilePath => PlayerProfilePath;
        public static string ProgressFilePath => GameProgressPath;

        public static UniTask SavePlayerProfile(PlayerSaveData data)
        {
            return SaveData(PlayerProfilePath, data);
        }

        public static UniTask<PlayerSaveData> LoadPlayerProfile()
        {
            if (!File.Exists(PlayerProfilePath))
            {
                return UniTask.FromResult(new PlayerSaveData());
            }

            return LoadData<PlayerSaveData>(PlayerProfilePath);
        }

        public static UniTask SaveGameProgress(ProgressSaveData data)
        {
            return SaveData(GameProgressPath, data);
        }

        public static UniTask<ProgressSaveData> LoadGameProgress()
        {
            if (!File.Exists(GameProgressPath))
            {
                return UniTask.FromResult(new ProgressSaveData());
            }

            return LoadData<ProgressSaveData>(GameProgressPath);
        }

        public static bool HasGameProgress()
        {
            return File.Exists(GameProgressPath);
        }

        public static void DeleteGameProgress()
        {
            if (!File.Exists(GameProgressPath))
            {
                return;
            }

            File.Delete(GameProgressPath);
            MLog.Log($"Deleted game progress at {GameProgressPath}");
        }

        public static void DeleteAllSaves()
        {
            if (!Directory.Exists(SaveDirectoryPath))
            {
                return;
            }

            Directory.Delete(SaveDirectoryPath, true);
            MLog.Log($"Deleted all saves at {SaveDirectoryPath}");
        }

        private static UniTask SaveData<T>(string path, T data) where T : class
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            EnsureSaveDirectory();

            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(path, json);
                MLog.Log($"Saved {typeof(T).Name} to {path}");
            }
            catch (Exception exception)
            {
                MLog.LogError($"Failed to save {typeof(T).Name} at {path}: {exception.Message}");
                throw;
            }

            return UniTask.CompletedTask;
        }

        private static UniTask<T> LoadData<T>(string path) where T : class, new()
        {
            try
            {
                string json = File.ReadAllText(path);
                T data = JsonUtility.FromJson<T>(json);
                if (data == null)
                {
                    throw new InvalidDataException($"File '{path}' contains invalid {typeof(T).Name} payload.");
                }

                return UniTask.FromResult(data);
            }
            catch (Exception exception)
            {
                MLog.LogError($"Failed to load {typeof(T).Name} at {path}: {exception.Message}");
                throw;
            }
        }

        private static void EnsureSaveDirectory()
        {
            if (Directory.Exists(SaveDirectoryPath))
            {
                return;
            }

            Directory.CreateDirectory(SaveDirectoryPath);
        }
    }
}
