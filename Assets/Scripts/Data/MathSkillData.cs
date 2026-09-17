using System;
using UnityEngine;

namespace StudyGame.Data
{
    public enum MathSkillType
    {
        LimitFixation,
        SyntaxOverride,
        FrequencySync,
        BlankOMR
    }

    [CreateAssetMenu(fileName = "MathSkill_", menuName = "StudyGame/Data/Math Skill Data")]
    public class MathSkillData : ScriptableObject
    {
        public string skillId;
        public string skillName;
        public MathSkillType skillType;
        public int baseApCost = 1;
        public float powerMultiplier = 1.0f;
        [TextArea(2, 4)]
        public string description;
        public Sprite skillIcon;
    }
}
