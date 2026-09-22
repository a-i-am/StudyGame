using UnityEngine;

namespace StudyGame.Runtime.Combat
{
    public enum StudySubject { Default, Korean, Math, English }

    /// <summary>
    /// 플레이어의 현재 '과목 속성(덱)'을 관리하는 스크립트.
    /// 무기 자체의 외형은 그대로 두되, Q/E 키로 속성을 스왑하여 
    /// 공격 이펙트나 데미지 판정에 영향을 주게 됩니다.
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        public StudySubject currentSubject = StudySubject.Default;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q)) SwapSubject(StudySubject.Korean);
            if (Input.GetKeyDown(KeyCode.E)) SwapSubject(StudySubject.Math);
            if (Input.GetKeyDown(KeyCode.F)) SwapSubject(StudySubject.English); // R 대신 F 사용 (일반적)
        }

        private void SwapSubject(StudySubject newSubject)
        {
            if (currentSubject == newSubject) return;
            
            currentSubject = newSubject;
            Debug.Log($"[CombatManager] 덱 스왑 완료: 현재 과목 속성 = {currentSubject}");
            
            // TODO: 무기 이펙트(Aura) 색상 변경이나 UI 갱신 이벤트 호출
        }
    }
}
