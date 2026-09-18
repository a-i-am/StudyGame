using System.Collections.Generic;
using UnityEngine;

namespace StudyGame.Data
{
    [CreateAssetMenu(fileName = "ValidationRule_", menuName = "StudyGame/Data/Puzzle Validation Rule")]
    public class ValidationRuleSO : ScriptableObject
    {
        public string puzzleId;

        [Header("Required Sentence Categories (in order)")]
        public List<SentenceCategory> expectedSequence;

        [Header("Specific Required Item IDs (Optional)")]
        [Tooltip("Leave empty or null if any item matching the category is acceptable")]
        public List<string> requiredItemIds;

        public bool Validate(List<SentenceItemData> submittedItems)
        {
            if (submittedItems == null || submittedItems.Count != expectedSequence.Count)
            {
                return false;
            }

            for (int i = 0; i < submittedItems.Count; i++)
            {
                if (submittedItems[i] == null) return false;
                
                // Check Category
                if (submittedItems[i].category != expectedSequence[i])
                {
                    return false;
                }

                // Check Specific ID if required
                if (requiredItemIds != null && i < requiredItemIds.Count && !string.IsNullOrEmpty(requiredItemIds[i]))
                {
                    if (submittedItems[i].itemId != requiredItemIds[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
