using System;
using System.IO;
using UnityEngine;

namespace StudyGame.Core
{
    public class LocalJsonPersistenceProvider : IPersistenceProvider
    {
        private readonly string saveFilePath;

        public LocalJsonPersistenceProvider(string fileName = "saveData.json")
        {
            saveFilePath = Path.Combine(Application.persistentDataPath, fileName);
        }

        public void Save(GameSaveData data)
        {
            if (data == null) return;
            data.lastSavedTimestamp = DateTime.UtcNow.ToString("o");
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFilePath, json);
        }

        public GameSaveData Load()
        {
            if (!File.Exists(saveFilePath))
            {
                return new GameSaveData();
            }

            try
            {
                string json = File.ReadAllText(saveFilePath);
                GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
                return data ?? new GameSaveData();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Persistence] Load failed: {ex.Message}");
                return new GameSaveData();
            }
        }

        public void Reset()
        {
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }
        }
    }
}
