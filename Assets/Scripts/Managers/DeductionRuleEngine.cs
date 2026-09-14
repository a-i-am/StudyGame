using System;
using System.Collections.Generic;
using UnityEngine;

namespace StudyGame.Managers
{
    public class DeductionRuleEngine : MonoBehaviour
    {
        public static DeductionRuleEngine Instance { get; private set; }

        [SerializeField] private ConceptData targetConcept;
        [SerializeField] private int maxAP = 20;
        [SerializeField] private List<ConceptData> initialCandidatePool = new List<ConceptData>();

        public ConceptData TargetConcept
        {
            get => targetConcept;
            private set => targetConcept = value;
        }

        public int MaxAP
        {
            get => maxAP;
            private set => maxAP = value;
        }

        public int CurrentAP { get; private set; } = 20;

        public List<string> AccumulatedClues { get; private set; } = new List<string>();
        public List<ConceptData> CandidatePool { get; private set; } = new List<ConceptData>();

        public event Action<int> OnAPChanged;
        public event Action<string> OnClueDiscovered;
        public event Action<bool, ConceptData> OnDeductionComplete;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CurrentAP = maxAP;
            if (initialCandidatePool != null && initialCandidatePool.Count > 0)
            {
                CandidatePool = new List<ConceptData>(initialCandidatePool);
            }
        }

        private bool isSessionSolved = false;

        public void InitializeSession(ConceptData target, int apLimit, List<ConceptData> candidates)
        {
            targetConcept = target;
            maxAP = apLimit > 0 ? apLimit : 20;
            CurrentAP = maxAP;
            AccumulatedClues.Clear();
            initialCandidatePool = candidates != null ? new List<ConceptData>(candidates) : new List<ConceptData>();
            CandidatePool = new List<ConceptData>(initialCandidatePool);
            isSessionSolved = false;

            OnAPChanged?.Invoke(CurrentAP);
        }

        public bool TryConsumeAP(int cost)
        {
            if (CurrentAP < cost)
            {
                return false;
            }

            CurrentAP -= cost;
            OnAPChanged?.Invoke(CurrentAP);
            return true;
        }

        public void CollectClue(string clue)
        {
            if (string.IsNullOrEmpty(clue)) return;
            if (AccumulatedClues.Contains(clue)) return;

            AccumulatedClues.Add(clue);
            OnClueDiscovered?.Invoke(clue);
        }

        public bool SubmitGuess(ConceptData guessedConcept)
        {
            if (guessedConcept == null) return false;
            if (isSessionSolved) return false;

            bool isCorrect = TargetConcept != null && guessedConcept.conceptId == TargetConcept.conceptId;

            if (isCorrect)
            {
                isSessionSolved = true;
                if (ConceptArchiveManager.Instance != null)
                {
                    bool newlyUnlocked = ConceptArchiveManager.Instance.TryUnlockConcept(guessedConcept);
                    if (!newlyUnlocked)
                    {
                        ConceptArchiveManager.Instance.ForceNotifyConceptUnlocked(guessedConcept);
                    }
                }
            }

            OnDeductionComplete?.Invoke(isCorrect, guessedConcept);
            return isCorrect;
        }
    }
}
