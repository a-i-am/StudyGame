using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using StudyGame.Managers;

namespace StudyGame.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIDeductionModalController : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement deductionOverlay;
        private Label apLabel;
        private Button closeButton;
        private ScrollView cluesScrollView;
        private VisualElement candidatesGrid;
        private Label feedbackLabel;

        private bool isInitialized = false;

        private void OnEnable()
        {
            InitializeUI();
            StartCoroutine(SubscribeToEngine());
        }

        private void InitializeUI()
        {
            if (isInitialized) return;

            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null || uiDocument.rootVisualElement == null) return;
            uiDocument.sortingOrder = 10;

            VisualElement root = uiDocument.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;

            deductionOverlay = root.Q<VisualElement>("deduction-overlay");
            apLabel = root.Q<Label>("ap-label");
            closeButton = root.Q<Button>("close-button");
            cluesScrollView = root.Q<ScrollView>("clues-scroll-view");
            candidatesGrid = root.Q<VisualElement>("candidates-grid");
            feedbackLabel = root.Q<Label>("feedback-label");

            if (closeButton != null)
            {
                closeButton.clicked += CloseModal;
            }

            if (deductionOverlay != null)
            {
                deductionOverlay.style.display = DisplayStyle.None;
            }

            isInitialized = true;
        }



        private IEnumerator SubscribeToEngine()
        {
            while (DeductionRuleEngine.Instance == null)
            {
                yield return null;
            }
            DeductionRuleEngine.Instance.OnAPChanged += UpdateAPDisplay;
            DeductionRuleEngine.Instance.OnClueDiscovered += OnClueDiscovered;
            DeductionRuleEngine.Instance.OnDeductionComplete += OnDeductionComplete;
        }

        private void OnDisable()
        {
            if (DeductionRuleEngine.Instance != null)
            {
                DeductionRuleEngine.Instance.OnAPChanged -= UpdateAPDisplay;
                DeductionRuleEngine.Instance.OnClueDiscovered -= OnClueDiscovered;
                DeductionRuleEngine.Instance.OnDeductionComplete -= OnDeductionComplete;
            }
        }

        public void OpenModal()
        {
            StartCoroutine(OpenModalRoutine());
        }

        private IEnumerator OpenModalRoutine()
        {
            uiDocument = GetComponent<UIDocument>();
            while (uiDocument != null && uiDocument.rootVisualElement == null) yield return null;

            InitializeUI();
            if (deductionOverlay != null)
            {
                deductionOverlay.style.display = DisplayStyle.Flex;
            }

            RefreshUI();
        }

        public void CloseModal()
        {
            if (deductionOverlay != null)
            {
                deductionOverlay.style.display = DisplayStyle.None;
            }
        }

        private void RefreshUI()
        {
            if (DeductionRuleEngine.Instance == null) return;

            if (DeductionRuleEngine.Instance.CandidatePool == null || DeductionRuleEngine.Instance.CandidatePool.Count == 0)
            {
                UIDialogueController dialogue = FindFirstObjectByType<UIDialogueController>();
                if (dialogue != null)
                {
                    var field = typeof(UIDialogueController).GetField("autoStartGraph", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        SequenceGraphData graph = field.GetValue(dialogue) as SequenceGraphData;
                        if (graph != null)
                        {
                            var method = typeof(UIDialogueController).GetMethod("EnsureDeductionSession", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (method != null)
                            {
                                method.Invoke(dialogue, new object[] { graph });
                            }
                        }
                    }
                }
            }

            UpdateAPDisplay(DeductionRuleEngine.Instance.CurrentAP);
            RefreshClues();
            RefreshCandidates();
        }

        private void UpdateAPDisplay(int currentAP)
        {
            if (apLabel != null && DeductionRuleEngine.Instance != null)
            {
                apLabel.text = $"남은 AP: {currentAP} / {DeductionRuleEngine.Instance.MaxAP}";
            }
        }

        private void RefreshClues()
        {
            if (cluesScrollView == null || DeductionRuleEngine.Instance == null) return;
            cluesScrollView.Clear();

            for (int i = 0; i < DeductionRuleEngine.Instance.AccumulatedClues.Count; i++)
            {
                Label label = new Label($"{i + 1}. {DeductionRuleEngine.Instance.AccumulatedClues[i]}");
                label.AddToClassList("clue-item");
                cluesScrollView.Add(label);
            }
        }

        private void RefreshCandidates()
        {
            if (candidatesGrid == null || DeductionRuleEngine.Instance == null) return;
            candidatesGrid.Clear();

            foreach (ConceptData concept in DeductionRuleEngine.Instance.CandidatePool)
            {
                if (concept == null) continue;
                Button btn = new Button();
                btn.text = concept.title;
                btn.AddToClassList("candidate-button");
                ConceptData target = concept;
                btn.clicked += () =>
                {
                    DeductionRuleEngine.Instance.SubmitGuess(target);
                };
                candidatesGrid.Add(btn);
            }
        }

        private void OnClueDiscovered(string clue)
        {
            RefreshClues();
        }

        private void OnDeductionComplete(bool isCorrect, ConceptData guessedConcept)
        {
            if (feedbackLabel != null)
            {
                if (isCorrect)
                {
                    feedbackLabel.text = $"🎉 정답입니다! [{guessedConcept.title}] 개념을 완벽하게 추론해냈습니다!";
                    feedbackLabel.style.color = new StyleColor(new Color(0.2f, 0.8f, 0.4f));
                }
                else
                {
                    feedbackLabel.text = $"❌ [{guessedConcept.title}] 은(는) 정답이 아닙니다. 단서를 더 수집해 보세요.";
                    feedbackLabel.style.color = new StyleColor(new Color(0.9f, 0.3f, 0.3f));
                }
            }
        }
    }
}
