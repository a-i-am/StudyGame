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
        Exploration,
        Combat,
        Deduction,
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

        public void LoadAndRunScenario(StageScenarioData scenario)
        {
            if (scenario == null)
            {
                Debug.LogError("[StageRunnerController] Scenario is null!");
                return;
            }
            CurrentScenario = scenario;
            StartCoroutine(RunStageRoutine());
        }

        private IEnumerator RunStageRoutine()
        {
            // 1. Loading State
            ChangeState(StageState.Loading);
            yield return StartCoroutine(LoadingRoutine());

            // 2. Exploration State
            ChangeState(StageState.Exploration);
            CursorManager.Instance.SetGameCursorLocked(true);
            if (playerController != null)
            {
                playerController.SetMovementEnabled(true);
            }
            if (cameraController != null)
            {
                cameraController.SetInputEnabled(true);
            }
            
            hasEncounteredAnomaly = false;
            yield return new WaitUntil(() => hasEncounteredAnomaly);

            // 3. Combat State
            ChangeState(StageState.Combat);
            yield return StartCoroutine(CombatRoutine());

            // 4. Deduction State
            ChangeState(StageState.Deduction);
            CursorManager.Instance.SetGameCursorLocked(false);
            if (playerController != null)
            {
                playerController.SetMovementEnabled(false);
            }
            if (cameraController != null)
            {
                cameraController.SetInputEnabled(false);
            }
            yield return StartCoroutine(DeductionRoutine());
        }

        private IEnumerator LoadingRoutine()
        {
            if (currentMapInstance != null) Destroy(currentMapInstance);
            if (currentMonsterInstance != null) Destroy(currentMonsterInstance);

            if (CurrentScenario.mapEnvironmentPrefab != null)
            {
                currentMapInstance = Instantiate(CurrentScenario.mapEnvironmentPrefab);
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
                currentMonsterInstance = Instantiate(CurrentScenario.anomalyMonsterPrefab);
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

        private IEnumerator CombatRoutine()
        {
            Debug.Log("[전투 돌입] 스킬 덱 UI를 엽니다.");
            
            StudyGame.Combat.UISkillDeckController skillDeck = FindFirstObjectByType<StudyGame.Combat.UISkillDeckController>(FindObjectsInactive.Include);
            
            // 동적 스폰 팩백 (에디터 씬 셋업이 안된 경우)
            if (skillDeck == null)
            {
                Debug.Log("[StageRunnerController] UI_SkillDeck이 씬에 없어 동적으로 생성합니다.");
                GameObject skillDeckObj = new GameObject("UI_SkillDeck");
                UnityEngine.UIElements.UIDocument doc = skillDeckObj.AddComponent<UnityEngine.UIElements.UIDocument>();
                doc.visualTreeAsset = Resources.Load<UnityEngine.UIElements.VisualTreeAsset>("UI/SkillDeckView"); 
                // 위 Resources Load가 실패하더라도, 유니티 에디터 스크립트에서 프리팹으로 미리 만들어 두거나 Setup을 쓰게 안내.
                skillDeck = skillDeckObj.AddComponent<StudyGame.Combat.UISkillDeckController>();
            }

            // Find and trigger active Anomaly Visualizer to start attacking player
            AnomalyCreatureVisualizer activeVisualizer = FindFirstObjectByType<AnomalyCreatureVisualizer>();
            if (activeVisualizer != null && currentPlayerInstance != null)
            {
                activeVisualizer.StartAttacking(currentPlayerInstance.transform);
            }

            if (skillDeck != null)
            {
                skillDeck.Show();
                
                bool skillSelected = false;
                MathSkill chosenSkill = MathSkill.Limit; // default

                System.Action<MathSkill> onSkill = (skill) => 
                {
                    chosenSkill = skill;
                    skillSelected = true;
                };

                skillDeck.OnSkillSelected += onSkill;
                yield return new WaitUntil(() => skillSelected);
                skillDeck.OnSkillSelected -= onSkill;
                
                skillDeck.Hide();

                if (activeVisualizer != null)
                {
                    activeVisualizer.StopAttacking();
                }

                if (currentMonsterInstance != null)
                {
                    MathGimmick gimmick = currentMonsterInstance.GetComponent<MathGimmick>();
                    if (gimmick != null)
                    {
                        gimmick.ApplySkill(chosenSkill);
                        yield return new WaitForSeconds(2.0f);
                    }
                    else
                    {
                        currentMonsterInstance.SetActive(false);
                    }
                }
            }
            else
            {
                yield return new WaitForSeconds(2.0f);
            }

            if (currentMonsterInstance != null)
            {
                IAnomalyEntity anomaly = currentMonsterInstance.GetComponent<IAnomalyEntity>();
                if (anomaly != null)
                {
                    anomaly.TriggerStabilizeEffect();
                }
                else
                {
                    currentMonsterInstance.SetActive(false);
                }
            }

            StudyGame.Partner.PartnerController partner = FindFirstObjectByType<StudyGame.Partner.PartnerController>();
            if (partner == null && CurrentScenario != null && CurrentScenario.partnerPrefab != null)
            {
                Vector3 spawnPos = currentMonsterInstance != null ? currentMonsterInstance.transform.position : (currentPlayerInstance != null ? currentPlayerInstance.transform.position + Vector3.forward * 1.5f : new Vector3(0f, 0f, 10f));
                GameObject partnerObj = Instantiate(CurrentScenario.partnerPrefab, spawnPos, Quaternion.identity);
                partner = partnerObj.GetComponent<StudyGame.Partner.PartnerController>();
            }

            if (partner != null && playerController != null)
            {
                playerController.SetPartner(partner);
            }

            Debug.Log("[StageRunnerController] Combat Phase Completed. Anomaly isolated.");
        }

        // Removed RunArenaCombatModule mock

        private IEnumerator DeductionRoutine()
        {
            UIDialogueController dialogueController = FindFirstObjectByType<UIDialogueController>(FindObjectsInactive.Include);
            if (dialogueController != null && CurrentScenario.dialogueGraph != null)
            {
                dialogueController.gameObject.SetActive(true);
                dialogueController.StartDialogue(CurrentScenario.dialogueGraph);
            }
            else
            {
                Debug.LogWarning("[StageRunnerController] UIDialogueController or DialogueGraph is missing.");
            }
            yield return null;
        }

        private void HandleDeductionComplete(bool isCorrect, ConceptData guessedConcept)
        {
            if (CurrentState == StageState.Deduction && isCorrect)
            {
                ChangeState(StageState.Cleared);
                
                // Hide dialogue UI
                UIDialogueController dialogueController = FindFirstObjectByType<UIDialogueController>();
                if (dialogueController != null)
                {
                    dialogueController.gameObject.SetActive(false);
                }

                // Unlock concept (Toast will show automatically via event)
                if (ConceptArchiveManager.Instance != null && guessedConcept != null)
                {
                    ConceptArchiveManager.Instance.TryUnlockConcept(guessedConcept);
                }

                Debug.Log($"[StageRunnerController] Stage Cleared! Concept unlocked: {(guessedConcept != null ? guessedConcept.title : "Unknown")}");
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
