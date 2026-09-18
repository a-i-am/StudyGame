using UnityEngine;
using UnityEditor;
using StudyGame.Data;
using System.Collections.Generic;
using System.IO;

namespace StudyGame.Tests.Editor
{
    public class PlayTestBootstrapWindow : EditorWindow
    {
        [MenuItem("StudyGame/Test/▶ Create PlayTest Assets &p")]
        public static void ShowWindow()
        {
            GetWindow<PlayTestBootstrapWindow>("PlayTest Bootstrap").Show();
            CreateAllAssets();
        }

        [MenuItem("StudyGame/Test/▶ Setup PlayTest UIs")]
        public static void SetupPlayTestUIs()
        {
            var panelSettings = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.PanelSettings>("Assets/UI Toolkit/PanelSettings.asset");
            if (panelSettings == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:PanelSettings");
                if (guids.Length > 0) panelSettings = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.PanelSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            GameObject uiObj = GameObject.Find("UI_Inventory");
            if (uiObj == null) uiObj = new GameObject("UI_Inventory");
            
            var uiDoc = uiObj.GetComponent<UnityEngine.UIElements.UIDocument>();
            if (uiDoc == null) uiDoc = uiObj.AddComponent<UnityEngine.UIElements.UIDocument>();
            
            uiDoc.panelSettings = panelSettings;
            uiDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>("Assets/UI/InventoryView.uxml");

            var controller = uiObj.GetComponent<StudyGame.UI.UIInventoryModalController>();
            if (controller == null) controller = uiObj.AddComponent<StudyGame.UI.UIInventoryModalController>();
            
            var so = new SerializedObject(controller);
            so.Update();
            so.FindProperty("document").objectReferenceValue = uiDoc;
            so.ApplyModifiedProperties();

            GameObject diaryObj = GameObject.Find("UI_Diary");
            if (diaryObj == null) diaryObj = new GameObject("UI_Diary");
            
            var diaryDoc = diaryObj.GetComponent<UnityEngine.UIElements.UIDocument>();
            if (diaryDoc == null) diaryDoc = diaryObj.AddComponent<UnityEngine.UIElements.UIDocument>();
            
            diaryDoc.panelSettings = panelSettings;
            diaryDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>("Assets/UI/DiaryModalView.uxml");

            var diaryController = diaryObj.GetComponent<StudyGame.UI.UIDiaryModalController>();
            if (diaryController == null) diaryController = diaryObj.AddComponent<StudyGame.UI.UIDiaryModalController>();

            Debug.Log("<color=lime>[PlayTest] ✅ UI_Inventory 및 UI_Diary 씬 셋업 완료!</color>");
        }

        public static void CreateAllAssets()
        {
            string folder = "Assets/Data/PlayTest";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/Data", "PlayTest");

            CreateSentenceItem(folder, "PlayTest_Subject_Doyoung", "도영의 가설",
                SentenceCategory.Subject, ItemTag.Logic | ItemTag.Quest,
                "SentenceItem_Subject_Doyoung.asset");
            CreateSentenceItem(folder, "PlayTest_Op_Converge", "수렴값 고정 연산",
                SentenceCategory.Operator, ItemTag.Logic,
                "SentenceItem_Operator_Converge.asset");
            CreateSentenceItem(folder, "PlayTest_Target_Limit", "극한 개념",
                SentenceCategory.TargetConcept, ItemTag.Logic | ItemTag.Deductive,
                "SentenceItem_Target_Limit.asset");
            CreateSentenceItem(folder, "PlayTest_Emotional_Jimin", "지민의 걱정",
                SentenceCategory.Premise, ItemTag.Emotional | ItemTag.Quest,
                "SentenceItem_Emotional_Jimin.asset");

            CreateValidationRule(folder);
            CreateSkillCard(folder, "skill_slash", "마이크 스매시", 2, "SkillCard_MicSmash.asset");
            CreateSkillCard(folder, "skill_beam", "주파수 빔", 3, "SkillCard_FreqBeam.asset");
            CreateSkillCard(folder, "skill_heal", "루틴 복원", 1, "SkillCard_Heal.asset");
            CreateConcept(folder);
            CreateEnemy(folder);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=lime>[PlayTest] ✅ 모든 플레이 테스트 에셋 생성 완료!</color>");
        }

        private static void CreateSentenceItem(string folder, string id, string text, SentenceCategory cat, ItemTag tags, string filename)
        {
            string path = $"{folder}/{filename}";
            if (AssetDatabase.LoadAssetAtPath<SentenceItemData>(path) != null) return;
            var item = ScriptableObject.CreateInstance<SentenceItemData>();
            item.itemId = id; item.displayText = text; item.category = cat; item.tags = tags; item.apCost = 1;
            AssetDatabase.CreateAsset(item, path);
        }

        private static void CreateValidationRule(string folder)
        {
            string path = $"{folder}/Rule_BasicSynthesis.asset";
            if (AssetDatabase.LoadAssetAtPath<ValidationRuleSO>(path) != null) return;
            var rule = ScriptableObject.CreateInstance<ValidationRuleSO>();
            rule.puzzleId = "boss_limit_01";
            rule.expectedSequence = new List<SentenceCategory>
                { SentenceCategory.Subject, SentenceCategory.Operator, SentenceCategory.TargetConcept };
            AssetDatabase.CreateAsset(rule, path);
        }

        private static void CreateSkillCard(string folder, string id, string displayName, int ap, string filename)
        {
            string path = $"{folder}/{filename}";
            if (AssetDatabase.LoadAssetAtPath<SkillCardSO>(path) != null) return;
            var card = ScriptableObject.CreateInstance<SkillCardSO>();
            card.skillId = id; card.displayName = displayName; card.apCost = ap;
            AssetDatabase.CreateAsset(card, path);
        }

        private static void CreateConcept(string folder)
        {
            string path = $"{folder}/Concept_LimitTest.asset";
            if (AssetDatabase.LoadAssetAtPath<ConceptData>(path) != null) return;
            var concept = ScriptableObject.CreateInstance<ConceptData>();
            concept.conceptId = "concept_limit_test";
            concept.title = "[테스트] 극한의 정의";
            concept.subject = SubjectType.Calculus;
            concept.archiveSummary = "lim f(x) = L 이란, x가 a에 무한히 가까워질 때 f(x)가 L에 가까워진다는 의미이다.";
            concept.loreFlavorText = "\"결국 모든 수렴은 방향을 정하는 일이야.\" - 도영";
            AssetDatabase.CreateAsset(concept, path);
        }

        private static void CreateEnemy(string folder)
        {
            string path = $"{folder}/Enemy_LimitGuardian.asset";
            if (AssetDatabase.LoadAssetAtPath<EnemyProfileSO>(path) != null) return;
            var enemy = ScriptableObject.CreateInstance<EnemyProfileSO>();
            enemy.enemyId = "enemy_limit_guardian";
            enemy.displayName = "극한의 수호자";
            enemy.maxHP = 100;
            enemy.attackPower = 12;
            enemy.weakness = EnemyWeaknessType.Convergence;
            AssetDatabase.CreateAsset(enemy, path);
        }
    }
}
