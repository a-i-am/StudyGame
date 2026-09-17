using System;
using System.Collections.Generic;
using UnityEngine;

namespace StudyGame.Data
{
    public enum SynergyEffectType
    {
        ApCostReduction,
        BonusDamage,
        UnlockDialogueBranch,
        GrantSentenceItem
    }

    [CreateAssetMenu(fileName = "TutorSkill_", menuName = "StudyGame/Data/Tutor Skill Node")]
    public class TutorSkillNodeData : ScriptableObject
    {
        public string nodeId;
        public string skillName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;
        public ConceptData requiredConcept;
        public List<TutorSkillNodeData> prerequisites = new List<TutorSkillNodeData>();
        public SynergyEffectType effectType;
        public float effectValue;
        public SentenceItemData rewardSentenceItem;
    }
}
