using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudyGame.Managers;
using StudyGame.Data;
using StudyGame.Field;

namespace StudyGame.Tests
{
    public class VerticalSliceTestRunner : MonoBehaviour
    {
        [Header("= SentenceItem 에셋 (인스펙터에서 할당) =")]
        public SentenceItemData itemSubject;
        public SentenceItemData itemOperator;
        public SentenceItemData itemTarget;
        public SentenceItemData itemEmotional;

        [Header("= 스킬 카드 에셋 =")]
        public SkillCardSO skillSlash;
        public SkillCardSO skillHeal;

        [Header("= 개념 데이터 (다이어리) =")]
        public ConceptData testConcept;

        [Header("= 씬 오브젝트 참조 =")]
        public GameObject pickupItemObj;
        public PortalTrigger portalToHideout;
        public Transform portalDestination;

        private List<string> testLog = new List<string>();
        private bool testsDone = false;

        private void Start()
        {
            StartCoroutine(RunIntegrationTests());
        }

        private IEnumerator RunIntegrationTests()
        {
            testLog.Clear();
            testLog.Add("<b>[수직 슬라이스 통합 테스트]</b>");

            // --- 씬 오브젝트 자동 설정 ---
            if (pickupItemObj != null)
            {
                var pickup = pickupItemObj.GetComponent<SentenceItemPickup>();
                if (pickup != null && itemSubject != null)
                {
                    pickup.itemData = itemSubject;
                    testLog.Add("✅ 픽업 아이템: 에셋 연결 완료");
                }
            }

            if (portalToHideout != null && portalDestination != null)
            {
                portalToHideout.targetTeleportPoint = portalDestination;
                portalToHideout.requireManualInteraction = false;
                testLog.Add("✅ 포탈: 목적지 연결 완료 (걸어서 닿으면 Hideout으로 이동)");
            }

            yield return new WaitForSeconds(0.3f);

            // --- 1. 인벤토리 테스트 ---
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ClearInventory();
                if (itemSubject != null) InventoryManager.Instance.AddItem(itemSubject);
                if (itemOperator != null) InventoryManager.Instance.AddItem(itemOperator);
                if (itemTarget != null) InventoryManager.Instance.AddItem(itemTarget);
                if (itemEmotional != null) InventoryManager.Instance.AddItem(itemEmotional);

                var logicItems = InventoryManager.Instance.GetItemsByTag(ItemTag.Logic);
                var emotItems = InventoryManager.Instance.GetItemsByTag(ItemTag.Emotional);
                testLog.Add($"✅ 인벤토리: 총 {InventoryManager.Instance.GetAllItems().Count}개 | Logic:{logicItems.Count} | Emotional:{emotItems.Count}");
            }
            else
            {
                testLog.Add("❌ InventoryManager 없음!");
            }

            yield return new WaitForSeconds(0.3f);

            // --- 2. 아지트 호감도 테스트 ---
            if (RapportManager.Instance != null)
            {
                RapportManager.Instance.AddRapport("npc_doyoung", 20);
                RapportManager.Instance.AddRapport("npc_jimin", 10);
                RapportManager.Instance.AdvanceTutoringProgress("npc_doyoung", 1);
                int d = RapportManager.Instance.GetRapport("npc_doyoung");
                int j = RapportManager.Instance.GetRapport("npc_jimin");
                int prog = RapportManager.Instance.GetTutoringProgress("npc_doyoung");
                testLog.Add($"✅ 호감도: 도영={d}(목표:20) 지민={j}(목표:10) | 과외진척도={prog}");
            }
            else
            {
                testLog.Add("❌ RapportManager 없음!");
            }

            yield return new WaitForSeconds(0.3f);

            // --- 3. 로드아웃 테스트 ---
            if (LoadoutManager.Instance != null && skillSlash != null && skillHeal != null)
            {
                LoadoutManager.Instance.AddCardToDeck(skillSlash);
                LoadoutManager.Instance.AddCardToDeck(skillHeal);
                LoadoutManager.Instance.SavePreset("TestPreset_Boss");
                LoadoutManager.Instance.RemoveCardFromDeck(skillSlash);

                int beforeLoad = LoadoutManager.Instance.GetActiveDeck().Count;
                LoadoutManager.Instance.LoadPreset("TestPreset_Boss");
                int afterLoad = LoadoutManager.Instance.GetActiveDeck().Count;

                testLog.Add($"✅ 로드아웃: 제거 후={beforeLoad}(목표:1) | 프리셋 복구={afterLoad}(목표:2)");
            }
            else
            {
                testLog.Add("⚠️ LoadoutManager 또는 스킬 카드 에셋 미연결 (인스펙터 확인 필요)");
            }

            yield return new WaitForSeconds(0.3f);

            // --- 4. 다이어리 개념 데이터 확인 ---
            if (testConcept != null)
            {
                testLog.Add($"✅ 다이어리 개념: '{testConcept.title}' | 오디오={testConcept.cassetteAudio != null}");
            }
            else
            {
                testLog.Add("⚠️ testConcept 미연결 (다이어리 탭 키 테스트는 가능)");
            }

            testLog.Add("<b>테스트 완료! 포탈에 직접 걸어가 보세요.</b>");
            testsDone = true;
        }

        private void OnGUI()
        {
            float panelW = 420;
            float panelH = 30 + testLog.Count * 24;
            GUI.Box(new Rect(10, 10, panelW, panelH), "");
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            style.fontSize = 13;

            for (int i = 0; i < testLog.Count; i++)
            {
                GUI.Label(new Rect(16, 16 + i * 24, panelW - 12, 22), testLog[i], style);
            }
        }
    }
}
