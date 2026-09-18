using UnityEngine;

namespace StudyGame.Data
{
    public enum SkillTargetType
    {
        SingleEnemy,
        AllEnemies,
        Self,
        SingleAlly
    }

    [CreateAssetMenu(fileName = "SkillCard_", menuName = "StudyGame/Data/Skill Card")]
    public class SkillCardSO : ScriptableObject
    {
        [Header("Identity")]
        public string skillId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;

        [Header("Cost & Targeting")]
        public int apCost = 1;
        public SkillTargetType targetType = SkillTargetType.SingleEnemy;

        [Header("Damage")]
        public int baseDamage = 15;
        public EnemyWeaknessType exploitsWeakness = EnemyWeaknessType.None;

        [Header("Effects")]
        public int healAmount = 0;
        public int shieldAmount = 0;
        public int debuffTurns = 0;
        [Range(0f, 1f)]
        public float stunChance = 0f;

        [Header("Visuals")]
        public Sprite cardIcon;
        public Color cardTintColor = Color.white;
    }
}
