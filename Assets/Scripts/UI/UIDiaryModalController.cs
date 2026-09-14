using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace StudyGame.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIDiaryModalController : MonoBehaviour
    {
        [SerializeField] private List<ConceptData> allConcepts = new List<ConceptData>();
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

        private UIDocument uiDocument;
        private VisualElement modalOverlay;
        private Button closeButton;
        private Button tabCalculus;
        private Button tabSequence;
        private Button tabStats;

        private ListView conceptListView;
        private VisualElement detailIcon;
        private Label detailTitle;
        private Label detailSummary;
        private Label detailLore;

        private SubjectType currentSubject = SubjectType.Calculus;
        private List<ConceptData> filteredConcepts = new List<ConceptData>();

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            VisualElement root = uiDocument.rootVisualElement;

            modalOverlay = root.Q<VisualElement>("modal-overlay");
            closeButton = root.Q<Button>("close-button");
            tabCalculus = root.Q<Button>("tab-calculus");
            tabSequence = root.Q<Button>("tab-sequence");
            tabStats = root.Q<Button>("tab-stats");

            conceptListView = root.Q<ListView>("concept-list");
            detailIcon = root.Q<VisualElement>("detail-icon");
            detailTitle = root.Q<Label>("detail-title");
            detailSummary = root.Q<Label>("detail-summary");
            detailLore = root.Q<Label>("detail-lore");

            if (closeButton != null) closeButton.clicked += CloseModal;

            if (tabCalculus != null) tabCalculus.clicked += () => SwitchTab(SubjectType.Calculus);
            if (tabSequence != null) tabSequence.clicked += () => SwitchTab(SubjectType.SequenceLimit);
            if (tabStats != null) tabStats.clicked += () => SwitchTab(SubjectType.ProbabilityStatistics);

            SetupListView();

            if (modalOverlay != null)
            {
                modalOverlay.style.display = DisplayStyle.None;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleModal();
            }
        }

        public void ToggleModal()
        {
            if (modalOverlay == null) return;

            if (modalOverlay.style.display == DisplayStyle.Flex)
            {
                CloseModal();
            }
            else
            {
                OpenModal();
            }
        }

        public void OpenModal()
        {
            if (modalOverlay != null)
            {
                modalOverlay.style.display = DisplayStyle.Flex;
            }
            SwitchTab(currentSubject);
        }

        public void CloseModal()
        {
            if (modalOverlay != null)
            {
                modalOverlay.style.display = DisplayStyle.None;
            }
        }

        private void SwitchTab(SubjectType subject)
        {
            currentSubject = subject;

            UpdateTabButtonState(tabCalculus, subject == SubjectType.Calculus);
            UpdateTabButtonState(tabSequence, subject == SubjectType.SequenceLimit);
            UpdateTabButtonState(tabStats, subject == SubjectType.ProbabilityStatistics);

            filteredConcepts = allConcepts.Where(c => c != null && c.subject == subject).ToList();
            if (conceptListView != null)
            {
                conceptListView.itemsSource = filteredConcepts;
                conceptListView.Rebuild();
                conceptListView.ClearSelection();
            }

            ResetDetailPanel();
        }

        private void UpdateTabButtonState(Button btn, bool isActive)
        {
            if (btn == null) return;
            if (isActive) btn.AddToClassList("tab-active");
            else btn.RemoveFromClassList("tab-active");
        }

        private void SetupListView()
        {
            if (conceptListView == null) return;

            conceptListView.makeItem = () =>
            {
                Label label = new Label();
                label.AddToClassList("concept-list-item");
                return label;
            };

            conceptListView.bindItem = (elem, index) =>
            {
                Label label = elem as Label;
                if (label == null || index < 0 || index >= filteredConcepts.Count) return;

                ConceptData concept = filteredConcepts[index];
                bool isUnlocked = ConceptArchiveManager.Instance != null && ConceptArchiveManager.Instance.IsUnlocked(concept);

                if (isUnlocked)
                {
                    label.text = concept.title;
                    label.RemoveFromClassList("locked");
                }
                else
                {
                    label.text = "??? (미해금 개념)";
                    label.AddToClassList("locked");
                }
            };

            conceptListView.selectionChanged += OnConceptSelected;
        }

        private void OnConceptSelected(IEnumerable<object> selectedItems)
        {
            ConceptData concept = selectedItems.FirstOrDefault() as ConceptData;
            if (concept == null)
            {
                ResetDetailPanel();
                return;
            }

            bool isUnlocked = ConceptArchiveManager.Instance != null && ConceptArchiveManager.Instance.IsUnlocked(concept);

            if (isUnlocked)
            {
                if (detailTitle != null) detailTitle.text = concept.title;
                if (detailSummary != null) detailSummary.text = concept.archiveSummary;
                if (detailLore != null) detailLore.text = concept.loreFlavorText;

                if (detailIcon != null)
                {
                    if (concept.diaryIcon != null)
                    {
                        detailIcon.style.backgroundImage = new StyleBackground(concept.diaryIcon);
                        detailIcon.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        detailIcon.style.backgroundImage = null;
                        detailIcon.style.display = DisplayStyle.None;
                    }
                }
            }
            else
            {
                if (detailTitle != null) detailTitle.text = "??? (잠긴 개념)";
                if (detailSummary != null) detailSummary.text = "해당 대사 시퀀스를 진행하면 도감에 등록됩니다.";
                if (detailLore != null) detailLore.text = "선행 조건을 만족하여 개념을 해금하세요.";
                if (detailIcon != null)
                {
                    detailIcon.style.backgroundImage = null;
                    detailIcon.style.display = DisplayStyle.None;
                }
            }
        }

        private void ResetDetailPanel()
        {
            if (detailTitle != null) detailTitle.text = "개념을 선택하세요";
            if (detailSummary != null) detailSummary.text = "-";
            if (detailLore != null) detailLore.text = "-";
            if (detailIcon != null)
            {
                detailIcon.style.backgroundImage = null;
                detailIcon.style.display = DisplayStyle.None;
            }
        }
    }
}
