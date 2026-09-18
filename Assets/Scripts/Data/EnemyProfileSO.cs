using UnityEngine;

namespace StudyGame.Data
{
    public enum EnemyWeaknessType
    {
        None,
        Convergence,
        Translation,
        ContextReading,
        Empathy,
        Entropy,
        History
    }

    [CreateAssetMenu(fileName = "EnemyProfile_", menuName = "StudyGame/Data/Enemy Profile")]
    public class EnemyProfileSO : ScriptableObject
    {
        [Header("Identity")]
        public string enemyId;
        public string displayName;

        [Header("Stats")]
        public int maxHP = 100;
        public int attackPower = 10;
        public int defense = 5;

        [Header("Weakness & Patterns")]
        public EnemyWeaknessType weakness = EnemyWeaknessType.None;
        public float weaknessDamageMultiplier = 2.0f;

        [Header("Turn Behavior")]
        public int actionsPerTurn = 1;
        [Range(0f, 1f)]
        public float specialAttackChance = 0.3f;

        [Header("Field Behavior")]
        public float fieldMoveSpeed = 3f;
        public float fieldDetectionRange = 8f;
        public float stunDuration = 2.5f;

        [Header("Visuals")]
        public GameObject fieldPrefab;
        public Color emissiveColor = Color.cyan;
    }
}
