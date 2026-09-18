using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using StudyGame.Data;
using StudyGame.Managers;

namespace StudyGame.UI
{
    public class QuestionSynthesizerUI : MonoBehaviour
    {
        public UIDocument uiDocument;
        public ValidationRuleSO currentRule;

        private VisualElement root;
        private List<VisualElement> dropSlots = new List<VisualElement>();
        private List<SentenceItemData> submittedItems = new List<SentenceItemData>();
        
        private Button submitButton;
        private Label feedbackLabel;

        private QuestionSynthesizerValidator validator = new QuestionSynthesizerValidator();

        private void OnEnable()
        {
            if (uiDocument == null) return;

            root = uiDocument.rootVisualElement;
            if (root == null) return;

            // Assuming we have slots named "Slot0", "Slot1", "Slot2" in the UXML
            for (int i = 0; i < 3; i++)
            {
                var slot = root.Q<VisualElement>($"Slot{i}");
                if (slot != null)
                {
                    dropSlots.Add(slot);
                    // Registration for drag and drop would happen here (using GenericDragAndDropHandler logic)
                }
            }

            submitButton = root.Q<Button>("SubmitButton");
            if (submitButton != null)
            {
                submitButton.clicked += OnSubmitClicked;
            }

            feedbackLabel = root.Q<Label>("FeedbackLabel");
        }

        private void OnDisable()
        {
            if (submitButton != null)
            {
                submitButton.clicked -= OnSubmitClicked;
            }
        }

        // Called by drag and drop handler when an item is dropped into a slot
        public void OnItemDropped(int slotIndex, SentenceItemData itemData)
        {
            while (submittedItems.Count <= slotIndex)
            {
                submittedItems.Add(null);
            }
            submittedItems[slotIndex] = itemData;

            // Visual update
            if (dropSlots.Count > slotIndex && dropSlots[slotIndex] != null)
            {
                // In a real implementation, you'd update a child VisualElement's background image or text
                dropSlots[slotIndex].style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.8f, 0.8f));
            }
        }

        private void OnSubmitClicked()
        {
            if (currentRule == null)
            {
                if (feedbackLabel != null) feedbackLabel.text = "현재 활성화된 퍼즐 규칙이 없습니다.";
                return;
            }

            var result = validator.ValidateSynthesis(currentRule, submittedItems);

            if (feedbackLabel != null)
            {
                feedbackLabel.text = result.feedbackMessage;
                if (result.isValid)
                {
                    feedbackLabel.style.color = new StyleColor(Color.green);
                    // Trigger climax attack sequence
                }
                else
                {
                    feedbackLabel.style.color = new StyleColor(Color.red);
                    // Trigger NaN error failure state
                }
            }
        }
    }
}
