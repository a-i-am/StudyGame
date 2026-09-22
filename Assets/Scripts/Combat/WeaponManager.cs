using UnityEngine;

namespace StudyGame.Runtime.Combat
{
    /// <summary>
    /// 플레이어의 기본 무기(외형 의존성 없음)의 공격 모션 및 판정을 관리합니다.
    /// 마우스 좌클릭으로 기본 콤보 공격을 트리거합니다.
    /// </summary>
    public class WeaponManager : MonoBehaviour
    {
        [Header("Weapon Settings")]
        public float attackCooldown = 0.5f;
        public float attackRange = 2.5f;
        
        private float lastAttackTime = 0f;
        private CombatManager combatManager;

        private void Awake()
        {
            combatManager = GetComponent<CombatManager>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                PerformAttack();
            }
        }

        private void PerformAttack()
        {
            if (Time.time - lastAttackTime < attackCooldown) return;
            
            lastAttackTime = Time.time;
            StudySubject currentStance = combatManager != null ? combatManager.currentSubject : StudySubject.Default;

            Debug.Log($"[WeaponManager] {currentStance} 속성으로 기본 공격 휘두름!");
            
            // TODO: OverlapSphere 또는 Raycast를 통해 attackRange 내의 적 타격 판정
            // TODO: 애니메이터 트리거 (콤보 카운트 연동)
        }
    }
}
