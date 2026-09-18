using System;
using UnityEngine;

namespace StudyGame.Data
{
    public enum SentenceCategory
    {
        Subject,
        Operator,
        TargetConcept,
        MathematicalConstraint,
        Premise,
        Contradiction,
        Conclusion
    }

    [Flags]
    public enum ItemTag
    {
        None = 0,
        Logic = 1 << 0,
        Emotional = 1 << 1,
        Deductive = 1 << 2,
        Quest = 1 << 3,
        Rare = 1 << 4
    }

    [CreateAssetMenu(fileName = "SentenceItem_", menuName = "StudyGame/Data/Sentence Item Data")]
    public class SentenceItemData : ScriptableObject
    {
        public string itemId;
        public string displayText;
        public SentenceCategory category;
        public ItemTag tags = ItemTag.None;
        
        public ConceptData associatedConcept;
        public MathSkillData associatedSkill;
        public int apCost = 1;
        public Sprite itemIcon;
    }
}
