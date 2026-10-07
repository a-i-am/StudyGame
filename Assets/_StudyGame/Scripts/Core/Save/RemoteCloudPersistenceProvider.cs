using System;
using UnityEngine;

namespace StudyGame.Core
{
    public class RemoteCloudPersistenceProvider : IPersistenceProvider
    {
        private string serverUrl;
        private string userToken;
        private LocalJsonPersistenceProvider fallbackLocalProvider;

        public RemoteCloudPersistenceProvider(string serverUrl, string userToken)
        {
            this.serverUrl = serverUrl;
            this.userToken = userToken;
            this.fallbackLocalProvider = new LocalJsonPersistenceProvider("cloudFallbackData.json");
        }

        public void Save(GameSaveData data)
        {
            if (data == null) return;
            fallbackLocalProvider.Save(data);
            Debug.Log($"[CloudPersistence] Simulated cloud sync to {serverUrl} for token {userToken}");
        }

        public GameSaveData Load()
        {
            Debug.Log($"[CloudPersistence] Simulated cloud fetch from {serverUrl}");
            return fallbackLocalProvider.Load();
        }

        public void Reset()
        {
            fallbackLocalProvider.Reset();
        }
    }
}
