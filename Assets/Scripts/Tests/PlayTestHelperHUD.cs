using UnityEngine;
using StudyGame.Managers;
using StudyGame.Data;
using StudyGame.Field;
using System.Collections.Generic;

namespace StudyGame.Tests
{
    public class PlayTestHelperHUD : MonoBehaviour
    {
        [System.Serializable]
        public class FloatingLabel
        {
            public Transform target;
            public string text;
            public Vector3 offset = new Vector3(0, 1.5f, 0);
        }

        [Header("Floating Labels")]
        public List<FloatingLabel> labels = new List<FloatingLabel>();

        private GUIStyle hudStyle;
        private GUIStyle labelStyle;
        private GUIStyle titleStyle;

        private void Start()
        {
            // Auto-detect targets if they exist in the scene
            var portal = FindObjectOfType<PortalTrigger>();
            if (portal != null)
            {
                labels.Add(new FloatingLabel { target = portal.transform, text = "🚪 [포탈]\n닿으면 아지트로 이동" });
            }

            var pickup = FindObjectOfType<SentenceItemPickup>();
            if (pickup != null)
            {
                labels.Add(new FloatingLabel { target = pickup.transform, text = "✨ [아이템]\n가까이 가면 획득" });
            }

            GameObject hideoutZone = GameObject.Find("Zone_Hideout");
            if (hideoutZone != null)
            {
                labels.Add(new FloatingLabel { target = hideoutZone.transform, text = "📻 [아지트]\nNPC 대화 및 정비 구역" });
            }
        }

        private void Update()
        {
            // Hotkeys for testing backend features
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestInventoryPickup();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestRapportIncrease();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestDeckBuilding();
            }
        }

        private void TestInventoryPickup()
        {
            if (InventoryManager.Instance == null) return;
            var dummyItem = ScriptableObject.CreateInstance<SentenceItemData>();
            dummyItem.itemId = "dummy_" + Random.Range(100, 999);
            dummyItem.displayText = "임시 단서 " + Random.Range(1, 100);
            dummyItem.tags = ItemTag.Logic;
            InventoryManager.Instance.AddItem(dummyItem);
            Debug.Log($"[HUD] 임시 아이템 '{dummyItem.displayText}' 획득!");
        }

        private void TestRapportIncrease()
        {
            if (RapportManager.Instance == null) return;
            RapportManager.Instance.AddRapport("npc_doyoung", 5);
            Debug.Log($"[HUD] 도영과 대화! 호감도 +5");
        }

        private void TestDeckBuilding()
        {
            if (LoadoutManager.Instance == null) return;
            var dummySkill = ScriptableObject.CreateInstance<SkillCardSO>();
            dummySkill.skillId = "skill_" + Random.Range(100, 999);
            dummySkill.displayName = "임시 스킬 " + Random.Range(1, 100);
            LoadoutManager.Instance.AddCardToDeck(dummySkill);
            Debug.Log($"[HUD] 덱에 '{dummySkill.displayName}' 추가 완료!");
        }

        private void OnGUI()
        {
            InitStyles();

            DrawHUD();
            DrawFloatingLabels();
        }

        private void InitStyles()
        {
            if (hudStyle == null)
            {
                hudStyle = new GUIStyle(GUI.skin.box);
                hudStyle.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.15f, 0.85f));
                hudStyle.richText = true;
                hudStyle.alignment = TextAnchor.UpperLeft;
                hudStyle.padding = new RectOffset(15, 15, 15, 15);
            }

            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label);
                titleStyle.richText = true;
                titleStyle.fontSize = 16;
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.normal.textColor = new Color(0.4f, 0.8f, 1f);
            }

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.richText = true;
                labelStyle.fontSize = 14;
                labelStyle.alignment = TextAnchor.MiddleCenter;
                labelStyle.normal.textColor = Color.white;
                labelStyle.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.6f));
            }
        }

        private void DrawHUD()
        {
            float width = 450;
            float height = 220;
            float x = 20;
            float y = Screen.height - height - 20;

            GUI.Box(new Rect(x, y, width, height), "", hudStyle);

            GUILayout.BeginArea(new Rect(x + 15, y + 15, width - 30, height - 30));
            
            GUILayout.Label("🎮 수직 슬라이스 테스트 컨트롤 HUD", titleStyle);
            GUILayout.Space(10);

            // Status
            string invCount = InventoryManager.Instance != null ? InventoryManager.Instance.GetAllItems().Count.ToString() : "0";
            string rapport = RapportManager.Instance != null ? RapportManager.Instance.GetRapport("npc_doyoung").ToString() : "0";
            string deckCount = LoadoutManager.Instance != null ? LoadoutManager.Instance.GetActiveDeck().Count.ToString() : "0";

            GUILayout.Label($"<b>🎒 인벤토리:</b> 아이템 {invCount}개 소지중");
            GUILayout.Label($"<b>📻 아지트(도영):</b> 호감도 {rapport}");
            GUILayout.Label($"<b>🃏 로드아웃:</b> 덱에 스킬 {deckCount}장 편성됨");
            
            GUILayout.Space(15);
            GUILayout.Label("<color=#aaaaaa>아래 숫자 키를 눌러 백엔드 시스템을 테스트하세요:</color>");
            GUILayout.Space(5);
            
            GUILayout.Label("<b>[ 1 ]</b> 가상의 단서 아이템 루팅 (Inventory)");
            GUILayout.Label("<b>[ 2 ]</b> 도영과 대화하여 호감도 상승 (Rapport)");
            GUILayout.Label("<b>[ 3 ]</b> 랜덤 스킬 카드 덱에 추가 (Loadout)");
            GUILayout.Label("<b>[ Tab ]</b> 다이어리 열기 (Diary UI)");

            GUILayout.EndArea();
        }

        private void DrawFloatingLabels()
        {
            if (Camera.main == null) return;
            Camera cam = Camera.main;

            foreach (var label in labels)
            {
                if (label.target == null) continue;

                Vector3 worldPos = label.target.position + label.offset;
                
                // Only draw if in front of camera
                Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);
                if (viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1)
                {
                    Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
                    
                    GUIContent content = new GUIContent(label.text);
                    Vector2 size = labelStyle.CalcSize(content);
                    
                    // Invert Y because GUI Y is top-down
                    Rect rect = new Rect(screenPos.x - (size.x / 2) - 5, Screen.height - screenPos.y - size.y - 5, size.x + 10, size.y + 10);
                    
                    GUI.Label(rect, content, labelStyle);
                }
            }
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
