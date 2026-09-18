using System.Collections.Generic;
using UnityEngine;
using StudyGame.Data;

namespace StudyGame.Managers
{
    public class LoadoutManager : MonoBehaviour
    {
        public static LoadoutManager Instance { get; private set; }

        public const int MAX_DECK_SIZE = 10;

        private List<SkillCardSO> activeDeck = new List<SkillCardSO>();
        private Dictionary<string, List<SkillCardSO>> deckPresets = new Dictionary<string, List<SkillCardSO>>();

        public event System.Action OnDeckChanged;

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

        public bool AddCardToDeck(SkillCardSO card)
        {
            if (activeDeck.Count < MAX_DECK_SIZE && !activeDeck.Contains(card))
            {
                activeDeck.Add(card);
                OnDeckChanged?.Invoke();
                return true;
            }
            return false;
        }

        public void RemoveCardFromDeck(SkillCardSO card)
        {
            if (activeDeck.Remove(card))
            {
                OnDeckChanged?.Invoke();
            }
        }

        public List<SkillCardSO> GetActiveDeck()
        {
            return new List<SkillCardSO>(activeDeck);
        }

        public void SavePreset(string presetName)
        {
            deckPresets[presetName] = new List<SkillCardSO>(activeDeck);
        }

        public void LoadPreset(string presetName)
        {
            if (deckPresets.ContainsKey(presetName))
            {
                activeDeck = new List<SkillCardSO>(deckPresets[presetName]);
                OnDeckChanged?.Invoke();
            }
        }
    }
}
