using System;
using UnityEngine;

namespace StudyGame.Data
{
    public enum SentenceCategory
    {
        Subject,
        Operator,
        TargetConcept,
        MathematicalConstraint
    }

    [CreateAssetMenu(fileName = "SentenceItem_", menuName = "StudyGame/Data/Sentence Item Data")]
    public class SentenceItemData : ScriptableObject
    {
        public string itemId;
        public string displayText;
        public SentenceCategory category;
        public ConceptData associatedConcept;
        public MathSkillData associatedSkill;
        public int apCost = 1;
        public Sprite itemIcon;
    }
}
