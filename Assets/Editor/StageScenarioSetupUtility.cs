#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using StudyGame.Data;
using StudyGame.Managers;
using StudyGame.UI;
using StudyGame.LLM;
using StudyGame.Combat;
using StudyGame.Partner;
using StudyGame.Environment;

public static class StageScenarioSetupUtility
{
    [MenuItem("StudyGame/Setup Exploration Runner Scene")]
    public static void SetupExplorationRunnerScene()
    {
        StageScenarioData scenario = GenerateVerticalSliceScenario();

        string saveFilePath = Path.Combine(Application.persistentDataPath, "unlocked_concepts.json");
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }

        PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI Toolkit/PanelSettings.asset");

        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        GameObject managersObj = new GameObject("@Managers");
        managersObj.AddComponent<ConceptArchiveManager>();
        managersObj.AddComponent<DeductionRuleEngine>();
        managersObj.AddComponent<LLMStreamSender>();
        managersObj.AddComponent<StageRunnerController>();

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
        dialogueObj.SetActive(false);

        UIDialogueController dialogueController = dialogueObj.AddComponent<UIDialogueController>();
        NPCData activeNPC = AssetDatabase.LoadAssetAtPath<NPCData>("Assets/ScriptableObjects/TestNPCs/NPC_INTP.asset");
        if (activeNPC != null && dialogueController != null)
        {
            dialogueController.SetActiveNPC(activeNPC);
        }

        GameObject launcherObj = new GameObject("StageRunnerLauncher");
        ExplorationStageRunnerLauncher launcher = launcherObj.AddComponent<ExplorationStageRunnerLauncher>();
        launcher.targetScenario = scenario;
        EditorUtility.SetDirty(launcher);

        VisualTreeAsset skillDeckAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/SkillDeckView.uxml");
        if (skillDeckAsset != null)
        {
            GameObject skillDeckObj = new GameObject("UI_SkillDeck");
            UIDocument skillDeckDocument = skillDeckObj.AddComponent<UIDocument>();
            BindUIDocument(skillDeckDocument, panelSettings, skillDeckAsset);
            skillDeckObj.AddComponent<StudyGame.Combat.UISkillDeckController>();
        }

        string scenePath = "Assets/Scenes/Scene_ExplorationRunner.unity";
        if (!Directory.Exists("Assets/Scenes"))
        {
            Directory.CreateDirectory("Assets/Scenes");
        }
        EditorSceneManager.MarkSceneDirty(newScene);
        EditorSceneManager.SaveScene(newScene, scenePath);
        AssetDatabase.Refresh();

        AddSceneToBuildSettings(scenePath);
        Debug.Log("[StageScenarioSetupUtility] Scene_ExplorationRunner setup completed successfully!");

        SetupWireframeChamberScene();
    }

    [MenuItem("StudyGame/Setup Wireframe Chamber Scene")]
    public static void SetupWireframeChamberScene()
    {
        try
        {
            StageScenarioData scenario = GenerateVerticalSliceScenario();

        string saveFilePath = Path.Combine(Application.persistentDataPath, "unlocked_concepts.json");
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }

        PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI Toolkit/PanelSettings.asset");

        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 1. Core Managers
        GameObject managersObj = new GameObject("@Managers");
        managersObj.AddComponent<ConceptArchiveManager>();
        managersObj.AddComponent<DeductionRuleEngine>();
        managersObj.AddComponent<LLMStreamSender>();
        managersObj.AddComponent<StageRunnerController>();

        // 2. UI Systems
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
        dialogueObj.SetActive(false);

        UIDialogueController dialogueController = dialogueObj.AddComponent<UIDialogueController>();
        NPCData activeNPC = AssetDatabase.LoadAssetAtPath<NPCData>("Assets/ScriptableObjects/TestNPCs/NPC_INTP.asset");
        if (activeNPC != null && dialogueController != null)
        {
            dialogueController.SetActiveNPC(activeNPC);
        }

        VisualTreeAsset skillDeckAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/SkillDeckView.uxml");
        if (skillDeckAsset != null)
        {
            GameObject skillDeckObj = new GameObject("UI_SkillDeck");
            UIDocument skillDeckDocument = skillDeckObj.AddComponent<UIDocument>();
            BindUIDocument(skillDeckDocument, panelSettings, skillDeckAsset);
            skillDeckObj.AddComponent<StudyGame.Combat.UISkillDeckController>();
        }

        // 3. Environment Chamber & JSON Anomaly Creatures
        GameObject envObj = new GameObject("WireframeEnvironment");
        WireframeEnvironmentBuilder envBuilder = envObj.AddComponent<WireframeEnvironmentBuilder>();
        envBuilder.BuildWireframeChamber();

        // 4. Launcher Setup
        if (scenario != null)
        {
            scenario.mapEnvironmentPrefab = null;
            scenario.anomalyMonsterPrefab = null;
        }

        GameObject launcherObj = new GameObject("StageRunnerLauncher");
        ExplorationStageRunnerLauncher launcher = launcherObj.AddComponent<ExplorationStageRunnerLauncher>();
        launcher.targetScenario = scenario;
        EditorUtility.SetDirty(launcher);

        string scenePath = "Assets/Scenes/Scene_WireframeChamber.unity";
        if (!Directory.Exists("Assets/Scenes"))
        {
            Directory.CreateDirectory("Assets/Scenes");
        }
        EditorSceneManager.MarkSceneDirty(newScene);
        EditorSceneManager.SaveScene(newScene, scenePath);
        AssetDatabase.Refresh();

        AddSceneToBuildSettings(scenePath);
        Debug.Log("[StageScenarioSetupUtility] Scene_WireframeChamber setup completed successfully!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SetupWireframeChamberScene Error]: {ex}");
        }
    }

    [MenuItem("StudyGame/Generate Vertical Slice Scenario")]
    public static StageScenarioData GenerateVerticalSliceScenario()
    {
        GenerateTestNPC();

        string basePath = "Assets/ScriptableObjects/Scenarios/GeomLimit";
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
        {
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        }
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Scenarios"))
        {
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Scenarios");
        }
        if (!AssetDatabase.IsValidFolder(basePath))
        {
            AssetDatabase.CreateFolder("Assets/ScriptableObjects/Scenarios", "GeomLimit");
        }

        // 1. Concepts
        ConceptData conceptRatio = CreateOrLoadAsset<ConceptData>($"{basePath}/Concept_Ratio.asset");
        conceptRatio.conceptId = "Math_Ratio";
        conceptRatio.title = "공비";
        conceptRatio.archiveSummary = "연속된 두 항 사이의 일정한 곱셈 비율";
        conceptRatio.subject = SubjectType.SequenceLimit;
        EditorUtility.SetDirty(conceptRatio);

        ConceptData conceptLimit = CreateOrLoadAsset<ConceptData>($"{basePath}/Concept_Limit.asset");
        conceptLimit.conceptId = "Math_Limit";
        conceptLimit.title = "수열의 극한";
        conceptLimit.archiveSummary = "무한히 진행할 때 일정한 값에 한없이 가까워지는 성질";
        conceptLimit.subject = SubjectType.SequenceLimit;
        EditorUtility.SetDirty(conceptLimit);

        ConceptData conceptGeomLimit = CreateOrLoadAsset<ConceptData>($"{basePath}/Concept_GeomLimit.asset");
        conceptGeomLimit.conceptId = "Math_GeomLimit";
        conceptGeomLimit.title = "등비수열의 극한";
        conceptGeomLimit.archiveSummary = "공비의 절댓값 조건에 따라 수렴과 발산이 결정되는 규칙";
        conceptGeomLimit.subject = SubjectType.SequenceLimit;
        conceptGeomLimit.prerequisites = new List<ConceptData> { conceptRatio, conceptLimit };
        EditorUtility.SetDirty(conceptGeomLimit);

        // 2. Sequence Nodes
        SequenceNode nodePrereqRatio = CreateOrLoadAsset<SequenceNode>($"{basePath}/Node_Prereq_Ratio.asset");
        nodePrereqRatio.guid = System.Guid.NewGuid().ToString();
        nodePrereqRatio.name = "Node_Prereq_Ratio";
        nodePrereqRatio.unlockConcept = conceptRatio;
        nodePrereqRatio.apCost = 1;
        nodePrereqRatio.discoveredClue = "연속된 두 항 사이에는 일정한 곱셈 규칙(공비)이 존재한다.";
        nodePrereqRatio.personaDialogues = new List<PersonaDialogueGroup>
        {
            new PersonaDialogueGroup
            {
                personality = MBTIType.INTP,
                lines = new List<DialogueLine>
                {
                    new DialogueLine { speakerName = "AI Tutor", text = "이전 항에서 다음 항으로 갈 때 항상 같은 값을 곱하고 있군요." },
                    new DialogueLine { speakerName = "AI Tutor", text = "이 일정한 비율을 '공비'라고 부릅니다." }
                }
            }
        };
        EditorUtility.SetDirty(nodePrereqRatio);

        SequenceNode nodeRoot = CreateOrLoadAsset<SequenceNode>($"{basePath}/Node_Root.asset");
        nodeRoot.guid = System.Guid.NewGuid().ToString();
        nodeRoot.name = "Node_Root";
        nodeRoot.unlockConcept = conceptGeomLimit;
        nodeRoot.apCost = 1;
        nodeRoot.discoveredClue = "물체의 크기가 매번 절반씩 줄어들고 있으나, 영원히 사라지지 않고 한계점에 수렴한다.";
        nodeRoot.personaDialogues = new List<PersonaDialogueGroup>
        {
            new PersonaDialogueGroup
            {
                personality = MBTIType.INTP,
                lines = new List<DialogueLine>
                {
                    new DialogueLine { speakerName = "AI Tutor", text = "현상의 범위를 분석한 결과, 특정한 패턴을 따르고 있습니다." },
                    new DialogueLine { speakerName = "AI Tutor", text = "무한히 작아지지만 결코 0에 도달하진 않네요." }
                }
            }
        };
        nodeRoot.choices = new List<DialogueChoice>
        {
            new DialogueChoice { choiceText = "[기초 질문] 공비가 대체 뭐야?", targetNode = nodePrereqRatio, isSubBranch = true }
        };
        EditorUtility.SetDirty(nodeRoot);

        // 3. Sequence Graph
        SequenceGraphData graphGeomLimit = CreateOrLoadAsset<SequenceGraphData>($"{basePath}/Graph_GeomLimit.asset");
        graphGeomLimit.allNodes = new List<SequenceNode> { nodeRoot, nodePrereqRatio };
        graphGeomLimit.entryNode = nodeRoot;
        EditorUtility.SetDirty(graphGeomLimit);

        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }

        Material monsterMat = new Material(Shader.Find("Standard"));
        monsterMat.color = new Color(0.85f, 0.2f, 0.35f);
        string matPath = "Assets/Materials/Material_AnomalyMonster.mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(matPath) != null)
        {
            AssetDatabase.DeleteAsset(matPath);
        }
        AssetDatabase.CreateAsset(monsterMat, matPath);

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        Material playerMat = CreateOrUpdateMaterialAsset("Assets/Materials/Material_Player.mat", new Color(0.1f, 0.5f, 0.95f));
        Material partnerMat = CreateOrUpdateMaterialAsset("Assets/Materials/Material_Partner.mat", new Color(0.8f, 0.3f, 0.9f));
        Material playerWeaponMat = CreateOrUpdateMaterialAsset("Assets/Materials/Material_PlayerWeapon.mat", new Color(0.2f, 0.9f, 1.0f));
        Material partnerWeaponMat = CreateOrUpdateMaterialAsset("Assets/Materials/Material_PartnerWeapon.mat", new Color(1.0f, 0.4f, 0.8f));

        // 1. Create Player Prefab
        GameObject tempPlayer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        tempPlayer.name = "Player_Test";
        tempPlayer.tag = "Player";
        MeshRenderer playerRenderer = tempPlayer.GetComponent<MeshRenderer>();
        if (playerRenderer != null) playerRenderer.sharedMaterial = playerMat;

        StudyGame.Player.PlayerController playerComp = tempPlayer.AddComponent<StudyGame.Player.PlayerController>();

        // Player Weapon Hand Pivot
        GameObject playerHand = new GameObject("Hand_Right");
        playerHand.transform.SetParent(tempPlayer.transform, false);
        playerHand.transform.localPosition = new Vector3(0.4f, 0.2f, 0.4f);

        GameObject playerWeaponObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        playerWeaponObj.name = "Weapon_EnergySword";
        playerWeaponObj.transform.SetParent(playerHand.transform, false);
        playerWeaponObj.transform.localScale = new Vector3(0.08f, 0.9f, 0.08f);
        playerWeaponObj.transform.localPosition = new Vector3(0f, 0.45f, 0.2f);
        playerWeaponObj.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
        Object.DestroyImmediate(playerWeaponObj.GetComponent<Collider>());
        MeshRenderer pWeaponRenderer = playerWeaponObj.GetComponent<MeshRenderer>();
        if (pWeaponRenderer != null) pWeaponRenderer.sharedMaterial = playerWeaponMat;

        WeaponController pWeaponCtrl = playerWeaponObj.AddComponent<WeaponController>();
        pWeaponCtrl.SetEnergyColor(new Color(0.2f, 0.9f, 1f));
        playerComp.weaponController = pWeaponCtrl;

        GameObject playerPrefab = PrefabUtility.SaveAsPrefabAsset(tempPlayer, "Assets/Prefabs/Player_Test.prefab");
        Object.DestroyImmediate(tempPlayer);

        // 2. Create Partner Prefab
        GameObject tempPartner = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        tempPartner.name = "Partner_Test";
        MeshRenderer partnerRenderer = tempPartner.GetComponent<MeshRenderer>();
        if (partnerRenderer != null) partnerRenderer.sharedMaterial = partnerMat;

        StudyGame.Partner.PartnerController partnerComp = tempPartner.AddComponent<StudyGame.Partner.PartnerController>();

        // Partner Weapon Hand Pivot
        GameObject partnerHand = new GameObject("Hand_Right");
        partnerHand.transform.SetParent(tempPartner.transform, false);
        partnerHand.transform.localPosition = new Vector3(0.4f, 0.2f, 0.4f);

        GameObject partnerWeaponObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        partnerWeaponObj.name = "Weapon_ArcaneStaff";
        partnerWeaponObj.transform.SetParent(partnerHand.transform, false);
        partnerWeaponObj.transform.localScale = new Vector3(0.06f, 1.1f, 0.06f);
        partnerWeaponObj.transform.localPosition = new Vector3(0f, 0.55f, 0.2f);
        partnerWeaponObj.transform.localRotation = Quaternion.Euler(30f, 0f, 0f);
        Object.DestroyImmediate(partnerWeaponObj.GetComponent<Collider>());
        MeshRenderer partWeaponRenderer = partnerWeaponObj.GetComponent<MeshRenderer>();
        if (partWeaponRenderer != null) partWeaponRenderer.sharedMaterial = partnerWeaponMat;

        WeaponController partWeaponCtrl = partnerWeaponObj.AddComponent<WeaponController>();
        partWeaponCtrl.SetEnergyColor(new Color(1f, 0.4f, 0.8f));

        GameObject partnerPrefab = PrefabUtility.SaveAsPrefabAsset(tempPartner, "Assets/Prefabs/Partner_Test.prefab");
        Object.DestroyImmediate(tempPartner);

        GameObject tempMonster = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tempMonster.name = "Anomaly_Test";
        tempMonster.transform.position = new Vector3(0f, -12f, 8f);
        MeshRenderer monsterRenderer = tempMonster.GetComponent<MeshRenderer>();
        if (monsterRenderer != null)
        {
            monsterRenderer.sharedMaterial = monsterMat;
        }

        BoxCollider boxCol = tempMonster.GetComponent<BoxCollider>();
        if (boxCol != null)
        {
            boxCol.isTrigger = false;
        }

        tempMonster.AddComponent<StudyGame.Combat.AnomalyTrigger>();
        
        MathGimmick gimmick = tempMonster.AddComponent<MathGimmick>();
        gimmick.minScaleLimit = 0.2f;

        GameObject anomalyPrefab = PrefabUtility.SaveAsPrefabAsset(tempMonster, "Assets/Prefabs/Anomaly_Test.prefab");
        Object.DestroyImmediate(tempMonster);

        // Generate Hollow Sphere Map
        GameObject tempMap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tempMap.name = "Map_HollowSphere";
        tempMap.transform.localScale = new Vector3(30f, 30f, 30f); // 30m diameter

        MeshFilter filter = tempMap.GetComponent<MeshFilter>();
        if (filter != null && filter.sharedMesh != null)
        {
            Mesh mesh = Object.Instantiate(filter.sharedMesh);
            int[] triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int temp = triangles[i + 0];
                triangles[i + 0] = triangles[i + 1];
                triangles[i + 1] = temp;
            }
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            string meshPath = "Assets/Prefabs/InvertedSphereMesh.asset";
            if (AssetDatabase.LoadAssetAtPath<Mesh>(meshPath) != null)
            {
                AssetDatabase.DeleteAsset(meshPath);
            }
            AssetDatabase.CreateAsset(mesh, meshPath);
            filter.sharedMesh = mesh;
        }

        MeshRenderer renderer = tempMap.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        Object.DestroyImmediate(tempMap.GetComponent<Collider>()); // Remove default SphereCollider
        MeshCollider mc = tempMap.AddComponent<MeshCollider>();
        mc.sharedMesh = filter.sharedMesh;
        
        GameObject mapPrefab = PrefabUtility.SaveAsPrefabAsset(tempMap, "Assets/Prefabs/Map_HollowSphere.prefab");
        Object.DestroyImmediate(tempMap);

        // 4. Stage Scenario
        StageScenarioData scenarioGeomLimit = CreateOrLoadAsset<StageScenarioData>($"{basePath}/Scenario_GeomLimit.asset");
        scenarioGeomLimit.scenarioId = "SCENARIO_MATH_GEOM_01";
        scenarioGeomLimit.title = "격리 구역 04호: 수렴의 경계";
        scenarioGeomLimit.subject = SubjectType.SequenceLimit;
        scenarioGeomLimit.targetConcept = conceptGeomLimit;
        scenarioGeomLimit.startingAP = 8;
        scenarioGeomLimit.candidatePool = new List<ConceptData> { conceptRatio, conceptLimit, conceptGeomLimit };
        scenarioGeomLimit.dialogueGraph = graphGeomLimit;
        scenarioGeomLimit.playerPrefab = playerPrefab;
        scenarioGeomLimit.partnerPrefab = partnerPrefab;
        scenarioGeomLimit.anomalyMonsterPrefab = anomalyPrefab;
        scenarioGeomLimit.mapEnvironmentPrefab = mapPrefab;
        EditorUtility.SetDirty(scenarioGeomLimit);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[StageScenarioSetupUtility] Vertical Slice Scenario Generated Successfully at " + basePath);
        return scenarioGeomLimit;
    }

    private static void GenerateTestNPC()
    {
        string npcPath = "Assets/ScriptableObjects/TestNPCs/NPC_INTP.asset";
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/TestNPCs"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            {
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            }
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "TestNPCs");
        }

        NPCData npc = AssetDatabase.LoadAssetAtPath<NPCData>(npcPath);
        if (npc == null)
        {
            npc = ScriptableObject.CreateInstance<NPCData>();
            AssetDatabase.CreateAsset(npc, npcPath);
        }

        npc.npcId = "NPC_TUTOR_01";
        npc.npcName = "AI Tutor (INTP)";
        npc.mbtiType = MBTIType.INTP;
        npc.defaultMaskId = "MASK_OWL_01";
        npc.personalityDescription = "매우 논리적이고 분석적이며, 감정적인 공감보다는 원리 규명과 사실 관계 확인에 집착합니다. 호기심이 많지만 무뚝뚝해 보일 수 있습니다.";
        npc.speechStyle = "딱딱하고 건조한 어조. '~합니다', '~군요' 같은 격식을 차리지만 감정이 배제된 말투. 핵심만 짚어서 팩트폭력을 하는 성향.";
        npc.promptInjection = "Always stay logical. Do not show excessive emotion. Treat mathematical discovery as a purely intellectual puzzle.";
        EditorUtility.SetDirty(npc);
        AssetDatabase.SaveAssets();
    }

    private static void BindUIDocument(UIDocument document, PanelSettings panelSettings, VisualTreeAsset visualTree)
    {
        if (document == null) return;
        if (panelSettings != null) document.panelSettings = panelSettings;
        if (visualTree != null) document.visualTreeAsset = visualTree;
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

    private static Material CreateOrUpdateMaterialAsset(string path, Color color)
    {
        if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
        {
            AssetDatabase.DeleteAsset(path);
        }
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    private static T CreateOrLoadAsset<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
        }
        return asset;
    }
}

#endif
