#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using StudyGame.Managers;
using StudyGame.LLM;
using StudyGame.UI;
using StudyGame.Test;

public static class StudyGameTestSetup
{
    [MenuItem("StudyGame/Reset Save Data")]
    public static void ResetSaveDataMenu()
    {
        string saveFilePath = Path.Combine(Application.persistentDataPath, "unlocked_concepts.json");
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }
        if (ConceptArchiveManager.Instance != null)
        {
            ConceptArchiveManager.Instance.ResetSaveData();
        }
        Debug.Log("[StudyGameTestSetup] Concept Save Data has been cleared.");
    }

    [MenuItem("StudyGame/Setup Verification Scene")]
    public static void SetupVerificationScene()
    {
        EnsureDirectories();
        ResetSaveDataMenu();

        ConceptData conceptRatio = CreateOrLoadConcept("Assets/ScriptableObjects/TestConcepts/Concept_Ratio.asset", "Math_Ratio", "공비", "연속된 두 항의 일정한 비율", SubjectType.SequenceLimit, null);
        ConceptData conceptLimit = CreateOrLoadConcept("Assets/ScriptableObjects/TestConcepts/Concept_Limit.asset", "Math_Limit", "수열의 극한", "무한히 진행할 때 가까워지는 값", SubjectType.SequenceLimit, null);
        ConceptData conceptGeomLimit = CreateOrLoadConcept("Assets/ScriptableObjects/TestConcepts/Concept_GeomLimit.asset", "Math_GeomLimit", "등비수열의 극한", "공비의 절댓값에 따라 수렴/발산이 결정되는 규칙", SubjectType.SequenceLimit, new List<ConceptData> { conceptRatio, conceptLimit });

        NPCData npcCynical = CreateOrLoadNPC("Assets/ScriptableObjects/TestNPCs/NPC_Cynical.asset", "NPC_01", "아카데미 냉소파 조교", NPCType.Cynical);

        SequenceNode nodeRoot = CreateOrLoadSequenceNode("Assets/ScriptableObjects/TestGraph/Node_Root.asset", "Node_Root", conceptGeomLimit, 0, "크기가 절반씩 계속 줄어들고 있으나 완전히 0이 되지는 않는다.", new List<DialogueLine>
        {
            new DialogueLine { speakerName = "아카데미 냉소파 조교", text = "흥, 또 이상한 현상을 들고 왔군. 어디서부터 손을 대야 할지 감도 안 잡히나?" },
            new DialogueLine { speakerName = "아카데미 냉소파 조교", text = "단서를 잘 봐. 계속 줄어들고 있지만 단순한 뺄셈이 아니야." }
        }, NPCType.Cynical);

        SequenceNode nodePrereqRatio = CreateOrLoadSequenceNode("Assets/ScriptableObjects/TestGraph/Node_Prereq_Ratio.asset", "Node_Prereq_Ratio", conceptRatio, 0, "항과 항 사이에 일정한 곱셈 비율(공비)이 관찰된다.", new List<DialogueLine>
        {
            new DialogueLine { speakerName = "아카데미 냉소파 조교", text = "공비도 모르면서 극한을 다루겠다고? 쯧, 연속된 두 항을 나눠봐라. 그게 공비다." }
        }, NPCType.Cynical);

        nodeRoot.choices = new List<DialogueChoice>
        {
            new DialogueChoice { choiceText = "💡 현상을 유심히 살펴본다", targetNode = nodeRoot, isSubBranch = false },
            new DialogueChoice { choiceText = "💬 조교에게 조언을 구한다", targetNode = nodeRoot, isSubBranch = false }
        };
        EditorUtility.SetDirty(nodeRoot);

        SequenceGraphData graphData = CreateOrLoadGraphData("Assets/ScriptableObjects/TestGraph/TestGraphData.asset", nodeRoot, new List<SequenceNode> { nodeRoot, nodePrereqRatio });

        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI Toolkit/PanelSettings.asset");

        GameObject managersObj = new GameObject("@Managers");
        managersObj.AddComponent<ConceptArchiveManager>();

        DeductionRuleEngine deductionEngine = managersObj.AddComponent<DeductionRuleEngine>();
        deductionEngine.InitializeSession(conceptGeomLimit, 8, new List<ConceptData> { conceptRatio, conceptLimit, conceptGeomLimit });
        EditorUtility.SetDirty(deductionEngine);

        LLMStreamSender streamSender = managersObj.AddComponent<LLMStreamSender>();

        VisualTreeAsset toastAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/ConceptToastView.uxml");
        GameObject toastObj = new GameObject("UI_Toast");
        UIDocument toastDocument = toastObj.AddComponent<UIDocument>();
        BindUIDocument(toastDocument, panelSettings, toastAsset);
        toastObj.AddComponent<UIConceptToastController>();

        VisualTreeAsset deductionModalAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/DeductionModalView.uxml");
        GameObject deductionObj = new GameObject("UI_DeductionModal");
        UIDocument deductionDocument = deductionObj.AddComponent<UIDocument>();
        BindUIDocument(deductionDocument, panelSettings, deductionModalAsset);
        deductionObj.AddComponent<UIDeductionModalController>();

        VisualTreeAsset dialogueAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/DialogueView.uxml");
        GameObject dialogueObj = new GameObject("UI_Dialogue");
        UIDocument dialogueDocument = dialogueObj.AddComponent<UIDocument>();
        BindUIDocument(dialogueDocument, panelSettings, dialogueAsset);

        UIDialogueController dialogueController = dialogueObj.AddComponent<UIDialogueController>();
        SerializedObject serializedDialogue = new SerializedObject(dialogueController);
        SerializedProperty npcProp = serializedDialogue.FindProperty("activeNPC");
        if (npcProp != null) npcProp.objectReferenceValue = npcCynical;

        SerializedProperty autoStartGraphProp = serializedDialogue.FindProperty("autoStartGraph");
        if (autoStartGraphProp != null) autoStartGraphProp.objectReferenceValue = graphData;

        serializedDialogue.ApplyModifiedProperties();

        GameObject runnerObj = new GameObject("TestSequenceRunner");
        TestSequenceRunner runner = runnerObj.AddComponent<TestSequenceRunner>();
        SerializedObject serializedRunner = new SerializedObject(runner);
        SerializedProperty graphProp = serializedRunner.FindProperty("graphData");
        if (graphProp != null)
        {
            graphProp.objectReferenceValue = graphData;
            serializedRunner.ApplyModifiedProperties();
        }

        string scenePath = "Assets/Scenes/VerificationTestScene.unity";
        EditorSceneManager.MarkSceneDirty(newScene);
        EditorSceneManager.SaveScene(newScene, scenePath);
        AssetDatabase.Refresh();

        AddSceneToBuildSettings(scenePath);
        Debug.Log("[StudyGameTestSetup] VerificationTestScene setup completed!");
    }

    private static void BindUIDocument(UIDocument document, PanelSettings panelSettings, VisualTreeAsset visualTree)
    {
        if (document == null) return;
        document.panelSettings = panelSettings;
        document.visualTreeAsset = visualTree;
        EditorUtility.SetDirty(document);
    }

    private static void EnsureDirectories()
    {
        CreateDir("Assets/ScriptableObjects");
        CreateDir("Assets/ScriptableObjects/TestConcepts");
        CreateDir("Assets/ScriptableObjects/TestNPCs");
        CreateDir("Assets/ScriptableObjects/TestGraph");
        CreateDir("Assets/Scenes");
    }

    private static void CreateDir(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private static ConceptData CreateOrLoadConcept(string path, string id, string title, string summary, SubjectType subject, List<ConceptData> prereqs)
    {
        ConceptData asset = AssetDatabase.LoadAssetAtPath<ConceptData>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<ConceptData>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.conceptId = id;
        asset.title = title;
        asset.archiveSummary = summary;
        asset.subject = subject;
        asset.prerequisites = prereqs != null ? prereqs : new List<ConceptData>();

        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static NPCData CreateOrLoadNPC(string path, string id, string name, NPCType personality)
    {
        NPCData asset = AssetDatabase.LoadAssetAtPath<NPCData>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<NPCData>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.npcId = id;
        asset.npcName = name;
        asset.personalityType = personality;

        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static SequenceNode CreateOrLoadSequenceNode(string path, string name, ConceptData concept, int ap, string clue, List<DialogueLine> lines, NPCType personality)
    {
        SequenceNode asset = AssetDatabase.LoadAssetAtPath<SequenceNode>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<SequenceNode>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.name = name;
        asset.unlockConcept = concept;
        asset.apCost = ap;
        asset.discoveredClue = clue;

        asset.personaDialogues = new List<PersonaDialogueGroup>
        {
            new PersonaDialogueGroup
            {
                personality = personality,
                lines = lines
            }
        };

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        return asset;
    }

    private static SequenceGraphData CreateOrLoadGraphData(string path, SequenceNode entry, List<SequenceNode> nodes)
    {
        SequenceGraphData asset = AssetDatabase.LoadAssetAtPath<SequenceGraphData>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<SequenceGraphData>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.entryNode = entry;
        asset.allNodes = nodes;

        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        EditorBuildSettingsScene[] originalScenes = EditorBuildSettings.scenes;
        foreach (var scene in originalScenes)
        {
            if (scene.path == scenePath) return;
        }

        EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[originalScenes.Length + 1];
        System.Array.Copy(originalScenes, newScenes, originalScenes.Length);
        newScenes[newScenes.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
    }
}
#endif
