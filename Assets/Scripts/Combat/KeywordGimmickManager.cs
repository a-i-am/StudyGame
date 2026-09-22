using UnityEngine;
using System.Collections.Generic;

namespace StudyGame.Runtime.Combat
{
    /// <summary>
    /// 베리드 스타즈 스타일의 '키워드 조립' 기믹 매니저.
    /// 보스전 시 화면에 SNS 텍스트 힌트가 팝업되며, 
    /// 플레이어가 정답 키워드를 조립(숫자키 선택 등)하여 제출하면 
    /// 보스에게 큰 데미지(Break)를 줍니다.
    /// </summary>
    public class KeywordGimmickManager : MonoBehaviour
    {
        [Header("Gimmick Settings")]
        public List<string> availableKeywords = new List<string>();
        private string correctKeywordCombo = "수학+정석+오류";
        
        private bool isGimmickActive = false;
        private string currentInput = "";

        private void Update()
        {
            // 디버그 테스트용 트리거
            if (Input.GetKeyDown(KeyCode.K) && !isGimmickActive)
            {
                TriggerKeywordPhase();
            }

            if (isGimmickActive)
            {
                HandleKeywordInput();
            }
        }

        public void TriggerKeywordPhase()
        {
            isGimmickActive = true;
            currentInput = "";
            Debug.Log("[KeywordGimmick] ⚠️ 보스 쉴드 전개! 키워드를 조립하여 약점을 찌르세요!");
            Debug.Log("[KeywordGimmick] 조립할 키워드 선택: 1:수학, 2:정석, 3:오류 (순서대로 입력 후 Enter)");
            
            // TODO: UI 매니저를 호출하여 화면에 텍스트 팝업
        }

        private void HandleKeywordInput()
        {
            // 단순 시뮬레이션: 숫자 키로 키워드를 더하고 Enter로 제출
            if (Input.GetKeyDown(KeyCode.Alpha1)) currentInput += (currentInput.Length > 0 ? "+" : "") + "수학";
            if (Input.GetKeyDown(KeyCode.Alpha2)) currentInput += (currentInput.Length > 0 ? "+" : "") + "정석";
            if (Input.GetKeyDown(KeyCode.Alpha3)) currentInput += (currentInput.Length > 0 ? "+" : "") + "오류";

            if (Input.GetKeyDown(KeyCode.Return))
            {
                SubmitKeyword();
            }
        }

        private void SubmitKeyword()
        {
            if (currentInput == correctKeywordCombo)
            {
                Debug.Log($"[KeywordGimmick] 정답! '{currentInput}' - 보스의 쉴드가 파괴(Break)되었습니다!");
            }
            else
            {
                Debug.Log($"[KeywordGimmick] 오답! '{currentInput}' - 아무 일도 일어나지 않았습니다.");
            }
            
            isGimmickActive = false;
            currentInput = "";
        }
    }
}
