using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using StudyGame.UI;
using StudyGame.Data;
using StudyGame.Managers;

namespace StudyGame.EditorScripts
{
    public class TestSetupMenu
    {
        [MenuItem("StudyGame/Setup Test Scene")]
        public static void SetupTestScene()
        {
            // 1. Create SynthesizerTest GameObject
            GameObject testGo = GameObject.Find("SynthesizerTest");
            if (testGo == null)
            {
                testGo = new GameObject("SynthesizerTest");
            }

            // 2. Add UIDocument
            UIDocument uiDoc = testGo.GetComponent<UIDocument>();
            if (uiDoc == null)
            {
                uiDoc = testGo.AddComponent<UIDocument>();
            }

            // Load UXML and PanelSettings
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/QuestionSynthesizerView.uxml");
            if (uxml != null)
            {
                uiDoc.visualTreeAsset = uxml;
            }
            else
            {
                Debug.LogError("Could not find QuestionSynthesizerView.uxml at Assets/UI/");
            }

            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI Toolkit/PanelSettings.asset");
            if (panelSettings != null)
            {
                uiDoc.panelSettings = panelSettings;
            }

            // 3. Add Controller
            UIQuestionSynthesizerController controller = testGo.GetComponent<UIQuestionSynthesizerController>();
            if (controller == null)
            {
                controller = testGo.AddComponent<UIQuestionSynthesizerController>();
            }

            // 4. Add Test Script
            TestSynthesizer testScript = testGo.GetComponent<TestSynthesizer>();
            if (testScript == null)
            {
                testScript = testGo.AddComponent<TestSynthesizer>();
            }
            testScript.synthesizerUI = controller;

            testScript.testItems = new System.Collections.Generic.List<SentenceItemData>();
            string[] guids = AssetDatabase.FindAssets("t:SentenceItemData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SentenceItemData data = AssetDatabase.LoadAssetAtPath<SentenceItemData>(path);
                if (data != null)
                {
                    testScript.testItems.Add(data);
                }
            }

            GameObject tutorGo = GameObject.Find("UI_TutorModal");
            if (tutorGo == null)
            {
                tutorGo = new GameObject("UI_TutorModal");
            }
            UIDocument tutorUiDoc = tutorGo.GetComponent<UIDocument>();
            if (tutorUiDoc == null)
            {
                tutorUiDoc = tutorGo.AddComponent<UIDocument>();
            }
            VisualTreeAsset tutorUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/TutorModalView.uxml");
            if (tutorUxml != null)
            {
                tutorUiDoc.visualTreeAsset = tutorUxml;
            }
            if (panelSettings != null)
            {
                tutorUiDoc.panelSettings = panelSettings;
            }
            UITutorModalController tutorController = tutorGo.GetComponent<UITutorModalController>();
            if (tutorController == null)
            {
                tutorController = tutorGo.AddComponent<UITutorModalController>();
            }

            GameObject tutorMgrGo = GameObject.Find("TutorManager");
            if (tutorMgrGo == null)
            {
                tutorMgrGo = new GameObject("TutorManager");
                tutorMgrGo.AddComponent<TutorManager>();
            }

            Selection.activeGameObject = testGo;
            Debug.Log("Test scene setup complete! You can now press Play.");
        }
    }
}
