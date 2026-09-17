using System.Collections.Generic;
using UnityEngine;
using StudyGame.Data;
using StudyGame.UI;
using StudyGame.Managers;

public class TestSynthesizer : MonoBehaviour
{
    public UIQuestionSynthesizerController synthesizerUI;
    public List<SentenceItemData> testItems;

    private UITutorModalController tutorModal;
    private bool synthesizerVisible = true;

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
            synthesizerUI = FindFirstObjectByType<UIQuestionSynthesizerController>();
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

        tutorModal = FindFirstObjectByType<UITutorModalController>();
        if (tutorModal != null)
        {
            List<TutorSkillNodeData> skillNodes = LoadTutorSkillNodes();
            if (skillNodes.Count > 0)
            {
                tutorModal.DisplayTutorSession("doyoung", "도영", skillNodes);
            }
            tutorModal.HideModal();
        }

        CursorManager.Instance.SetGameCursorLocked(false);
    }

    private List<TutorSkillNodeData> LoadTutorSkillNodes()
    {
        var nodes = new List<TutorSkillNodeData>();

#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:TutorSkillNodeData");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            TutorSkillNodeData node = UnityEditor.AssetDatabase.LoadAssetAtPath<TutorSkillNodeData>(path);
            if (node != null)
            {
                nodes.Add(node);
            }
        }
#endif

        return nodes;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            synthesizerVisible = !synthesizerVisible;
            if (synthesizerVisible)
            {
                if (tutorModal != null) tutorModal.HideModal();
                if (synthesizerUI != null) synthesizerUI.ShowModal();
            }
            else
            {
                if (synthesizerUI != null) synthesizerUI.HideModal();
                if (tutorModal != null) tutorModal.ShowModal();
            }
        }
    }
}

