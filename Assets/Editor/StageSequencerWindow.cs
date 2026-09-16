#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using StudyGame.Data;

public class StageSequencerWindow : EditorWindow
{
    private SequenceGraphView graphView;
    private StageScenarioData currentScenarioData;
    private SequenceGraphData currentGraphData;
    private VisualElement rightPaneContent;
    private Image standingPreviewImage;
    private Label previewInfoLabel;
    
    private SerializedObject scenarioSerializedObject;
    private SerializedObject selectedNodeSerializedObject;
    private MBTIType selectedPersonaTab = MBTIType.Unknown;

    private int activeTabIndex = 0; // 0 = Scenario, 1 = Node

    [MenuItem("StudyGame/Stage Sequencer (All-in-One)")]
    public static void ShowWindow()
    {
        StageSequencerWindow wnd = GetWindow<StageSequencerWindow>();
        wnd.titleContent = new GUIContent("Stage Sequencer");
        wnd.minSize = new Vector2(1000, 600);
    }

    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj is StageScenarioData scenario)
        {
            ShowWindow();
            GetWindow<StageSequencerWindow>().LoadScenario(scenario);
            return true;
        }
        else if (obj is SequenceGraphData graph)
        {
            ShowWindow();
            GetWindow<StageSequencerWindow>().LoadGraph(graph);
            return true;
        }
        return false;
    }

    public void LoadScenario(StageScenarioData scenario)
    {
        currentScenarioData = scenario;
        if (scenario != null)
        {
            scenarioSerializedObject = new SerializedObject(scenario);
            LoadGraph(scenario.dialogueGraph);
        }
        else
        {
            scenarioSerializedObject = null;
            LoadGraph(null);
        }
        activeTabIndex = 0;
        RefreshRightPane();
    }

    public void LoadGraph(SequenceGraphData graph)
    {
        currentGraphData = graph;
        if (graphView != null)
        {
            graphView.PopulateView(currentGraphData);
        }
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        TwoPaneSplitView splitView = new TwoPaneSplitView(0, 600, TwoPaneSplitViewOrientation.Horizontal);
        root.Add(splitView);

        VisualElement leftPane = new VisualElement();
        leftPane.style.flexGrow = 1;

        Toolbar toolbar = new Toolbar();
        ObjectField scenarioField = new ObjectField("Scenario Asset")
        {
            objectType = typeof(StageScenarioData),
            allowSceneObjects = false,
            value = currentScenarioData
        };
        scenarioField.RegisterValueChangedCallback(evt =>
        {
            LoadScenario(evt.newValue as StageScenarioData);
        });
        toolbar.Add(scenarioField);

        ObjectField graphField = new ObjectField("Graph Asset")
        {
            objectType = typeof(SequenceGraphData),
            allowSceneObjects = false,
            value = currentGraphData
        };
        graphField.RegisterValueChangedCallback(evt =>
        {
            LoadGraph(evt.newValue as SequenceGraphData);
        });
        toolbar.Add(graphField);

        Button createNodeButton = new Button(CreateNewNode) { text = "새 노드 생성" };
        toolbar.Add(createNodeButton);
        leftPane.Add(toolbar);

        graphView = new SequenceGraphView();
        graphView.style.flexGrow = 1;
        graphView.OnNodeSelected = OnSequenceNodeSelected;
        leftPane.Add(graphView);

        splitView.Add(leftPane);

        VisualElement rightPane = new VisualElement();
        rightPane.style.width = 350;
        rightPane.style.flexDirection = FlexDirection.Column;

        Toolbar rightTabs = new Toolbar();
        Button tabScenario = new Button(() => { activeTabIndex = 0; RefreshRightPane(); }) { text = "시나리오 설정" };
        Button tabNode = new Button(() => { activeTabIndex = 1; RefreshRightPane(); }) { text = "노드 설정" };
        rightTabs.Add(tabScenario);
        rightTabs.Add(tabNode);
        rightPane.Add(rightTabs);

        ScrollView scroll = new ScrollView();
        scroll.style.flexGrow = 1;
        scroll.style.paddingLeft = 10;
        scroll.style.paddingRight = 10;
        scroll.style.paddingTop = 10;

        rightPaneContent = new VisualElement();
        scroll.Add(rightPaneContent);

        rightPane.Add(scroll);
        splitView.Add(rightPane);

        if (currentGraphData != null)
        {
            graphView.PopulateView(currentGraphData);
        }
        
        RefreshRightPane();
    }

    private void CreateNewNode()
    {
        if (currentGraphData == null)
        {
            EditorUtility.DisplayDialog("Error", "Graph Asset을 먼저 선택해주세요.", "OK");
            return;
        }

        SequenceNode node = ScriptableObject.CreateInstance<SequenceNode>();
        node.guid = Guid.NewGuid().ToString();
        node.name = $"SeqNode_{currentGraphData.allNodes.Count + 1}";
        node.graphPosition = new Vector2(100, 100);

        AssetDatabase.AddObjectToAsset(node, currentGraphData);
        Undo.RegisterCreatedObjectUndo(node, "Create Node");

        Undo.RecordObject(currentGraphData, "Add Node To Graph");
        currentGraphData.allNodes.Add(node);
        EditorUtility.SetDirty(currentGraphData);
        AssetDatabase.SaveAssets();

        graphView.PopulateView(currentGraphData);
    }

    private void OnSequenceNodeSelected(SequenceNodeView nodeView)
    {
        if (nodeView == null || nodeView.NodeData == null)
        {
            selectedNodeSerializedObject = null;
        }
        else
        {
            selectedNodeSerializedObject = new SerializedObject(nodeView.NodeData);
            activeTabIndex = 1; // Auto switch to node tab
        }
        RefreshRightPane();
    }

    private void RefreshRightPane()
    {
        if (rightPaneContent == null) return;
        rightPaneContent.Clear();

        if (activeTabIndex == 0)
        {
            RenderScenarioTab();
        }
        else
        {
            RenderNodeTab();
        }
    }

    private void RenderScenarioTab()
    {
        if (scenarioSerializedObject == null)
        {
            rightPaneContent.Add(new Label("선택된 시나리오가 없습니다."));
            return;
        }

        scenarioSerializedObject.Update();

        SerializedProperty prop = scenarioSerializedObject.GetIterator();
        if (prop.NextVisible(true))
        {
            do
            {
                if (prop.name == "m_Script") continue;
                PropertyField field = new PropertyField(prop);
                field.Bind(scenarioSerializedObject);
                rightPaneContent.Add(field);
            }
            while (prop.NextVisible(false));
        }
    }

    private void RenderNodeTab()
    {
        if (selectedNodeSerializedObject == null)
        {
            rightPaneContent.Add(new Label("선택된 노드가 없습니다."));
            return;
        }

        selectedNodeSerializedObject.Update();
        SequenceNode node = selectedNodeSerializedObject.targetObject as SequenceNode;

        SerializedProperty prop = selectedNodeSerializedObject.GetIterator();
        if (prop.NextVisible(true))
        {
            do
            {
                if (prop.name == "m_Script") continue;
                PropertyField field = new PropertyField(prop);
                field.Bind(selectedNodeSerializedObject);
                field.RegisterValueChangeCallback(_ => UpdatePreview(node));
                rightPaneContent.Add(field);
            }
            while (prop.NextVisible(false));
        }

        RenderPersonaSection(node);

        // Preview Box
        VisualElement previewBox = new VisualElement();
        previewBox.style.marginTop = 15;
        previewBox.style.paddingTop = 10;
        previewBox.style.borderTopWidth = 1;
        previewBox.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);

        previewInfoLabel = new Label("아트 미리보기 (캐릭터 스탠딩 / 맵)");
        previewInfoLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        previewBox.Add(previewInfoLabel);

        standingPreviewImage = new Image();
        standingPreviewImage.style.width = 180;
        standingPreviewImage.style.height = 240;
        standingPreviewImage.style.marginTop = 10;
        standingPreviewImage.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.4f);
        previewBox.Add(standingPreviewImage);

        rightPaneContent.Add(previewBox);

        UpdatePreview(node);
    }

    private void RenderPersonaSection(SequenceNode node)
    {
        VisualElement section = new VisualElement();
        section.style.marginTop = 15;
        section.style.paddingTop = 10;
        section.style.borderTopWidth = 1;
        section.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);

        Label header = new Label("NPC 성향별 페르소나 대사");
        header.style.unityFontStyleAndWeight = FontStyle.Bold;
        section.Add(header);

        Toolbar personaToolbar = new Toolbar();
        foreach (MBTIType type in Enum.GetValues(typeof(MBTIType)))
        {
            Button tabBtn = new Button(() =>
            {
                selectedPersonaTab = type;
                RefreshRightPane();
            })
            { text = type.ToString() };
            if (type == selectedPersonaTab)
            {
                tabBtn.style.backgroundColor = new Color(0.2f, 0.4f, 0.6f);
            }
            personaToolbar.Add(tabBtn);
        }
        section.Add(personaToolbar);

        SerializedProperty personaListProp = selectedNodeSerializedObject.FindProperty("personaDialogues");
        int targetIndex = -1;
        if (personaListProp != null)
        {
            for (int i = 0; i < personaListProp.arraySize; i++)
            {
                SerializedProperty groupProp = personaListProp.GetArrayElementAtIndex(i);
                SerializedProperty personalityProp = groupProp.FindPropertyRelative("personality");
                if (personalityProp != null && (MBTIType)personalityProp.enumValueIndex == selectedPersonaTab)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex >= 0)
            {
                SerializedProperty groupProp = personaListProp.GetArrayElementAtIndex(targetIndex);
                PropertyField linesField = new PropertyField(groupProp.FindPropertyRelative("lines"), $"{selectedPersonaTab} 대사 목록");
                linesField.Bind(selectedNodeSerializedObject);
                section.Add(linesField);
            }
            else
            {
                Button addGroupBtn = new Button(() =>
                {
                    selectedNodeSerializedObject.Update();
                    personaListProp.arraySize++;
                    SerializedProperty newElement = personaListProp.GetArrayElementAtIndex(personaListProp.arraySize - 1);
                    newElement.FindPropertyRelative("personality").enumValueIndex = (int)selectedPersonaTab;
                    newElement.FindPropertyRelative("lines").ClearArray();
                    selectedNodeSerializedObject.ApplyModifiedProperties();
                    RefreshRightPane();
                })
                { text = $"+ [{selectedPersonaTab}] 페르소나 대사 그룹 추가" };
                section.Add(addGroupBtn);
            }
        }

        rightPaneContent.Add(section);
    }

    private void UpdatePreview(SequenceNode node)
    {
        if (node == null || standingPreviewImage == null || previewInfoLabel == null) return;

        if (node.characterStanding != null)
        {
            standingPreviewImage.sprite = node.characterStanding;
            standingPreviewImage.style.display = DisplayStyle.Flex;
        }
        else
        {
            standingPreviewImage.style.display = DisplayStyle.None;
        }

        string mapInfo = node.mapPrefab != null ? node.mapPrefab.name : "할당된 맵 없음";
        previewInfoLabel.text = $"[미리보기] 맵 프리팹: {mapInfo}";
    }
}
#endif