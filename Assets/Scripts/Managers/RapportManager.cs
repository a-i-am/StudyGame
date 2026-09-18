using System.Collections.Generic;
using UnityEngine;

namespace StudyGame.Managers
{
    public class RapportManager : MonoBehaviour
    {
        public static RapportManager Instance { get; private set; }

        // Stores current rapport level per NPC ID
        private Dictionary<string, int> npcRapportLevels = new Dictionary<string, int>();
        
        // Stores tutoring progress (e.g., stage index or quest phase) per NPC ID
        private Dictionary<string, int> tutoringProgress = new Dictionary<string, int>();

        public event System.Action<string, int> OnRapportChanged;
        public event System.Action<string, int> OnTutoringProgressChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void AddRapport(string npcId, int amount)
        {
            if (!npcRapportLevels.ContainsKey(npcId))
            {
                npcRapportLevels[npcId] = 0;
            }
            npcRapportLevels[npcId] += amount;
            OnRapportChanged?.Invoke(npcId, npcRapportLevels[npcId]);
        }

        public int GetRapport(string npcId)
        {
            return npcRapportLevels.ContainsKey(npcId) ? npcRapportLevels[npcId] : 0;
        }

        public void AdvanceTutoringProgress(string npcId, int step = 1)
        {
            if (!tutoringProgress.ContainsKey(npcId))
            {
                tutoringProgress[npcId] = 0;
            }
            tutoringProgress[npcId] += step;
            OnTutoringProgressChanged?.Invoke(npcId, tutoringProgress[npcId]);
        }

        public int GetTutoringProgress(string npcId)
        {
            return tutoringProgress.ContainsKey(npcId) ? tutoringProgress[npcId] : 0;
        }
    }
}
