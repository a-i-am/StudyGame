using System.Collections.Generic;
using UnityEngine;
using StudyGame.Data;
using StudyGame.UI;
using StudyGame.Managers;

public class TestSynthesizer : MonoBehaviour
{
    public UIQuestionSynthesizerController synthesizerUI;
    public List<SentenceItemData> testItems;

    void Start()
    {
#if UNITY_EDITOR
        if (testItems == null || testItems.Count == 0)
        {
            testItems = new List<SentenceItemData>();
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SentenceItemData");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                SentenceItemData data = UnityEditor.AssetDatabase.LoadAssetAtPath<SentenceItemData>(path);
                if (data != null)
                {
                    testItems.Add(data);
                }
            }
        }
#endif

        if (testItems == null || testItems.Count == 0)
        {
            testItems = new List<SentenceItemData>();

            var item1 = ScriptableObject.CreateInstance<SentenceItemData>();
            item1.itemId = "subj_doyoung";
            item1.displayText = "도영의 문제";
            item1.category = SentenceCategory.Subject;

            var item2 = ScriptableObject.CreateInstance<SentenceItemData>();
            item2.itemId = "op_multiply";
            item2.displayText = "곱하기";
            item2.category = SentenceCategory.Operator;

            var item3 = ScriptableObject.CreateInstance<SentenceItemData>();
            item3.itemId = "target_concept";
            item3.displayText = "이차함수";
            item3.category = SentenceCategory.TargetConcept;

            testItems.Add(item1);
            testItems.Add(item2);
            testItems.Add(item3);
        }

        if (synthesizerUI == null)
        {
            synthesizerUI = FindObjectOfType<UIQuestionSynthesizerController>();
        }

        if (synthesizerUI != null)
        {
            synthesizerUI.PopulateInventory(testItems);
            synthesizerUI.ShowModal();
        }

        if (TutorManager.Instance != null)
        {
            TutorManager.Instance.SetStudentRescued("doyoung", true);
        }

        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        
        StudyGame.Player.PlayerController playerController = FindObjectOfType<StudyGame.Player.PlayerController>();
        if (playerController != null)
        {
            playerController.SetMovementEnabled(false);
        }

        StudyGame.Player.CameraController cameraController = FindObjectOfType<StudyGame.Player.CameraController>();
        if (cameraController != null)
        {
            cameraController.SetInputEnabled(false);
        }
    }
}
