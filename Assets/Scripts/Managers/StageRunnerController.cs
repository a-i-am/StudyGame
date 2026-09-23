using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using StudyGame.Data;
using StudyGame.Core;
using StudyGame.UI;
using StudyGame.Player;
using StudyGame.Combat;

namespace StudyGame.Managers
{
    public enum StageState
    {
        None,
        Loading,
        Prologue,
        SNSNotification,
        Exploration,
        Combat,
        BossDialogue,
        Cleared
    }

    public class StageRunnerController : MonoBehaviour
    {
        public static StageRunnerController Instance { get; private set; }

        public StageState CurrentState { get; private set; } = StageState.None;
        public StageScenarioData CurrentScenario { get; private set; }

        public event Action<StageScenarioData> OnStageCleared;


        private GameObject currentMapInstance;
        private GameObject currentMonsterInstance;
        private GameObject currentPlayerInstance;
        private StudyGame.Player.PlayerController playerController;
        private StudyGame.Player.CameraController cameraController;
        private Volume postProcessVolume;
        public bool isVirtualTestMode = false;
        private bool hasEncounteredAnomaly = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            if (DeductionRuleEngine.Instance != null)
            {
                DeductionRuleEngine.Instance.OnDeductionComplete += HandleDeductionComplete;
            }
        }

        private void OnDisable()
        {
            if (DeductionRuleEngine.Instance != null)
            {
                DeductionRuleEngine.Instance.OnDeductionComplete -= HandleDeductionComplete;
            }
        }

        public void NotifyAnomalyEncounter(GameObject anomalyObj)
        {
            if (!hasEncounteredAnomaly)
            {
                currentMonsterInstance = anomalyObj;
                hasEncounteredAnomaly = true;
            }
        }

        public void LoadAndRunScenario(StageScenarioData scenario, bool virtualTest = false)
        {
            if (scenario == null)
            {
                Debug.LogError("[StageRunnerController] Scenario is null!");
                return;
            }
            CurrentScenario = scenario;
            isVirtualTestMode = virtualTest;
            InjectSubjectLock(CurrentScenario.dominantSubject);
            StartCoroutine(RunStageRoutine());
        }

        private void InjectSubjectLock(DominantSubject subject)
        {
            // 실제 게임 모드일 경우에만 UI 컨트롤러 접근
            if (!isVirtualTestMode)
            {
                StudyGame.Combat.UISkillDeckController skillDeck = FindFirstObjectByType<StudyGame.Combat.UISkillDeckController>(FindObjectsInactive.Include);
                if (skillDeck != null)
                {
                    // TBD: skillDeck.LoadDeckBySubject(subject); 
                    Debug.Log($"[과목 락 인젝션] 스킬 덱을 {subject} 속성으로 강제 스왑합니다.");
                }
            }
            else
            {
                Debug.Log($"[Virtual Test] 과목 락 인젝션: {subject} 속성 적용 완료.");
            }
        }

        private IEnumerator RunStageRoutine()
        {
            ChangeState(StageState.Loading);
            yield return StartCoroutine(LoadingRoutine());

            ChangeState(StageState.Prologue);
            yield return StartCoroutine(PrologueRoutine());

            ChangeState(StageState.SNSNotification);
            yield return StartCoroutine(SNSRoutine());

            ChangeState(StageState.Exploration);
            if (!isVirtualTestMode)
            {
                CursorManager.Instance.SetGameCursorLocked(true);
                if (playerController != null) playerController.SetMovementEnabled(true);
                if (cameraController != null) cameraController.SetInputEnabled(true);
            }

            hasEncounteredAnomaly = false;
            if (isVirtualTestMode)
            {
                Debug.Log("[Virtual Test] 탐험 진행 중... (2초 후 괴이 조우)");
                yield return new WaitForSeconds(2.0f);
                hasEncounteredAnomaly = true;
            }
            else
            {
                yield return new WaitUntil(() => hasEncounteredAnomaly);
            }

            ChangeState(StageState.Combat);
            yield return StartCoroutine(CombatRoutine());

            ChangeState(StageState.BossDialogue);
            if (!isVirtualTestMode)
            {
                CursorManager.Instance.SetGameCursorLocked(false);
                if (playerController != null) playerController.SetMovementEnabled(false);
                if (cameraController != null) cameraController.SetInputEnabled(false);
            }
            yield return StartCoroutine(BossDialogueRoutine());
        }

        private IEnumerator LoadingRoutine()
        {
            if (isVirtualTestMode)
            {
                Debug.Log($"[Virtual Test] {CurrentScenario.title} 데이터 로딩 완료.");
                yield break; // 가상 모드는 에셋 로딩 스킵
            }

            // 기존 로딩 로직 유지 (currentMapInstance, currentPlayerInstance, currentMonsterInstance 생성 등)
            if (currentMapInstance != null) Destroy(currentMapInstance);
            if (currentMonsterInstance != null) Destroy(currentMonsterInstance);

            Vector3 legacyOffset = new Vector3(0, 500, 0);

            if (CurrentScenario.mapEnvironmentPrefab != null)
            {
                currentMapInstance = Instantiate(CurrentScenario.mapEnvironmentPrefab, legacyOffset, Quaternion.identity);
            }

            if (CurrentScenario.playerPrefab != null)
            {
                currentPlayerInstance = Instantiate(CurrentScenario.playerPrefab, Vector3.zero, Quaternion.identity);
                playerController = currentPlayerInstance.GetComponent<StudyGame.Player.PlayerController>();

                if (Camera.main != null)
                {
                    Camera.main.clearFlags = CameraClearFlags.SolidColor;
                    Camera.main.backgroundColor = new Color(0.05f, 0.05f, 0.1f);

                    cameraController = Camera.main.GetComponent<StudyGame.Player.CameraController>();
                    if (cameraController == null)
                    {
                        cameraController = Camera.main.gameObject.AddComponent<StudyGame.Player.CameraController>();
                    }
                    cameraController.SetTarget(currentPlayerInstance.transform);
                }
            }

            if (CurrentScenario.anomalyMonsterPrefab != null)
            {
                currentMonsterInstance = Instantiate(CurrentScenario.anomalyMonsterPrefab, legacyOffset, Quaternion.identity);
                AnomalyTrigger trigger = currentMonsterInstance.GetComponent<AnomalyTrigger>();
                if (trigger != null)
                {
                    trigger.OnPlayerEncountered += () => { hasEncounteredAnomaly = true; };
                }
            }

            if (CurrentScenario.skyboxMaterial != null)
            {
                RenderSettings.skybox = CurrentScenario.skyboxMaterial;
            }

            if (CurrentScenario.postProcessProfile != null)
            {
                if (postProcessVolume == null)
                {
                    GameObject volumeObj = new GameObject("PostProcessVolume");
                    postProcessVolume = volumeObj.AddComponent<Volume>();
                    postProcessVolume.isGlobal = true;
                }
                postProcessVolume.profile = CurrentScenario.postProcessProfile;
            }

            // Initialize Deduction Engine
            if (DeductionRuleEngine.Instance != null)
            {
                DeductionRuleEngine.Instance.InitializeSession(
                    CurrentScenario.targetConcept,
                    CurrentScenario.startingAP,
                    CurrentScenario.candidatePool
                );
            }

            yield return null;
        }

        private IEnumerator PrologueRoutine()
        {
            if (isVirtualTestMode)
            {
                Debug.Log("[Virtual Test] 프롤로그 영상 재생 (스킵됨).");
                yield break;
            }

            // TODO: 실제 게임 모드 시 PrologueDirector를 호출하여 영상 재생 및 대기
            yield return new WaitForSeconds(1.0f);
        }

        private IEnumerator SNSRoutine()
        {
            if (isVirtualTestMode)
            {
                Debug.Log($"[Virtual Test] SNS 알림 도착: {(CurrentScenario.initialSNSNotification != null ? "새 메시지 수신" : "(데이터 없음)")}");
                yield return new WaitForSeconds(1.0f);
                yield break;
            }

            Debug.Log("[인게임] SNS UI 팝업");
            yield return new WaitForSeconds(2.0f);
        }

        private IEnumerator CombatRoutine()
        {
            if (isVirtualTestMode)
            {
                Debug.Log("[Virtual Test] 괴이 조우! 스킬덱 활성화 및 기믹 전투 수행.");
                yield return new WaitForSeconds(1.5f); // 가상 전투 딜레이
                Debug.Log("[Virtual Test] 괴이 방어막 파괴 (안정화 완료).");
                yield break;
            }

            // 인게임 기존 CombatRoutine 로직 유지
            Debug.Log("[전투 돌입] 스킬 덱 UI를 엽니다.");
            StudyGame.Combat.UISkillDeckController skillDeck = FindFirstObjectByType<StudyGame.Combat.UISkillDeckController>(FindObjectsInactive.Include);

            if (skillDeck == null)
            {
                Debug.LogWarning("[StageRunnerController] UI_SkillDeck이 씬에 없습니다.");
            }
            else
            {
                // (기존 스킬 선택 및 몬스터 타격 로직)
            }

            yield return new WaitForSeconds(2.0f);
        }

        private IEnumerator BossDialogueRoutine()
        {
            if (isVirtualTestMode)
            {
                Debug.Log("[Virtual Test] 미니보스 대화 시작.");
                Debug.Log("[Virtual Test] 문장 합성(추리) UI 호출 대기중...");
                // 가상 모드에서는 에디터 창에서 강제로 HandleDeductionComplete를 호출하여 클리어 처리
                yield break;
            }

            // 인게임 기존 DeductionRoutine 로직(대화 시작) 유지
            UIDialogueController dialogueController = FindFirstObjectByType<UIDialogueController>(FindObjectsInactive.Include);
            if (dialogueController != null && CurrentScenario.dialogueGraph != null)
            {
                dialogueController.gameObject.SetActive(true);
                dialogueController.StartDialogue(CurrentScenario.dialogueGraph);
            }
            else
            {
                Debug.LogWarning("[StageRunnerController] UIDialogueController or BossDialogueGraph is missing.");
            }
            yield return null;
        }
        private void HandleDeductionComplete(bool isCorrect, ConceptData guessedConcept)
        {
            if (CurrentState == StageState.BossDialogue && isCorrect) // 상태 검사 변경
            {
                ChangeState(StageState.Cleared);

                if (!isVirtualTestMode)
                {
                    UIDialogueController dialogueController = FindFirstObjectByType<UIDialogueController>();
                    if (dialogueController != null) dialogueController.gameObject.SetActive(false);
                }

                if (ConceptArchiveManager.Instance != null && guessedConcept != null)
                {
                    ConceptArchiveManager.Instance.TryUnlockConcept(guessedConcept);
                }

                Debug.Log($"[(Virtual: {isVirtualTestMode})] Stage Cleared! Concept unlocked: {(guessedConcept != null ? guessedConcept.title : "Unknown")}");
                OnStageCleared?.Invoke(CurrentScenario);
            }
        }

        private void ChangeState(StageState newState)
        {
            CurrentState = newState;
            Debug.Log($"[StageRunnerController] State changed to: {newState}");
        }
    }
}
