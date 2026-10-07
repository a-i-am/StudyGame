using UnityEngine;
using Yarn.Unity;

namespace StudyGame.Interaction
{
    /// <summary>
    /// Yarn 스크립트의 커스텀 명령어들을 실제 C# 모듈(UI, 전투, 레벨)과 연결해주는 중계소
    /// </summary>
    public class EpisodeCommandHandler : MonoBehaviour
    {
        // [YarnCommand("명령어이름")] 어트리뷰트를 사용하면 .yarn 파일에서 해당 명령어를 직접 호출할 수 있습니다.

        [YarnCommand("play_video")]
        public void PlayVideo(string uiModuleName, string videoName)
        {
            // 예: [UI_VideoPlayer] 모듈 호출
            Debug.Log($"[EpisodeCommandHandler] 비디오 재생 요청: UI={uiModuleName}, Video={videoName}");
            // 실제 연결 로직은 기획이 확정될 때마다 하나씩 추가 (YAGNI)
        }

        [YarnCommand("load_level")]
        public void LoadLevel(string moduleName, string levelPresetName)
        {
            // 예: [Level_Environment] 모듈 호출
            Debug.Log($"[EpisodeCommandHandler] 레벨 로드 요청: 모듈={moduleName}, 프리셋={levelPresetName}");
        }

        [YarnCommand("play_haptic")]
        public void PlayHaptic(string uiModuleName, string hapticType)
        {
            // 예: [UI_Phone] 모듈 호출
            Debug.Log($"[EpisodeCommandHandler] 햅틱 재생 요청: UI={uiModuleName}, Type={hapticType}");
        }

        [YarnCommand("play_sound")]
        public void PlaySound(string uiModuleName, string soundName)
        {
            Debug.Log($"[EpisodeCommandHandler] 사운드 재생 요청: UI={uiModuleName}, Sound={soundName}");
        }

        [YarnCommand("open_ui")]
        public void OpenUI(string uiModuleName, string optionalData = "")
        {
            Debug.Log($"[EpisodeCommandHandler] UI 열기 요청: UI={uiModuleName}, Data={optionalData}");
            if (uiModuleName == "UI_SNS")
            {
                if (StudyGame.Managers.ModuleHub.Instance.PhoneUI != null)
                {
                    StudyGame.Managers.ModuleHub.Instance.PhoneUI.AddMessage("SNS UI Activated");
                }
            }
        }

        [YarnCommand("wait_ui_close")]
        public void WaitUIClose(string uiModuleName)
        {
            // TODO: 코루틴이나 비동기 처리가 필요하지만 일단 로그만 (나중에 구현 - Ponytail Debt)
            Debug.Log($"[EpisodeCommandHandler] UI 닫기 대기: UI={uiModuleName}");
        }

        [YarnCommand("enable_control")]
        public void EnableControl(string moduleName)
        {
            // 예: [Player_Controller] 조작 활성화
            Debug.Log($"[EpisodeCommandHandler] 플레이어 조작 활성화: 모듈={moduleName}");
        }

        [YarnCommand("start_combat")]
        public void StartCombat(string enemyModuleName)
        {
            // 예: [Enemy_Boss_AI] 모듈 호출
            Debug.Log($"[EpisodeCommandHandler] 전투 시작 요청: 대상={enemyModuleName}");
        }
    }
}
