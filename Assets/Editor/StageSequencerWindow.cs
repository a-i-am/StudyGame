#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class StageSequencerWindow : EditorWindow
{
    private SequenceGraphView graphView;
    private SequenceGraphData currentGraphData;
    private VisualElement detailPanel;
    private Image standingPreviewImage;
    private Label previewInfoLabel;
    private SerializedObject selectedSerializedObject;
    private NPCType selectedPersonaTab = NPCType.Standard;

    [MenuItem("Tools/Stage Sequencer")]
    public static void ShowWindow()
    {
        StageSequencerWindow wnd = GetWindow<StageSequencerWindow>();
        wnd.titleContent = new GUIContent("Stage Sequencer");
        wnd.minSize = new Vector2(900, 500);
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        TwoPaneSplitView splitView = new TwoPaneSplitView(0, 600, TwoPaneSplitViewOrientation.Horizontal);
        root.Add(splitView);

        VisualElement leftPane = new VisualElement();
        leftPane.style.flexGrow = 1;

        Toolbar toolbar = new Toolbar();
        ObjectField graphField = new ObjectField("Graph Asset")
        {
            objectType = typeof(SequenceGraphData),
            allowSceneObjects = false,
            value = currentGraphData
        };
        graphField.RegisterValueChangedCallback(evt =>
        {
            currentGraphData = evt.newValue as SequenceGraphData;
            if (graphView != null)
            {
                graphView.PopulateView(currentGraphData);
            }
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

        VisualElement rightPane = new ScrollView();
        rightPane.style.width = 320;
        rightPane.style.paddingLeft = 10;
        rightPane.style.paddingRight = 10;
        rightPane.style.paddingTop = 10;

        detailPanel = new VisualElement();
        rightPane.Add(detailPanel);

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

        rightPane.Add(previewBox);
        splitView.Add(rightPane);

        if (currentGraphData != null)
        {
            graphView.PopulateView(currentGraphData);
        }
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
        detailPanel.Clear();
        if (nodeView == null || nodeView.NodeData == null) return;

        SequenceNode node = nodeView.NodeData;
        selectedSerializedObject = new SerializedObject(node);

        SerializedProperty prop = selectedSerializedObject.GetIterator();
        if (prop.NextVisible(true))
        {
            do
            {
                if (prop.name == "m_Script") continue;
                PropertyField field = new PropertyField(prop);
                field.Bind(selectedSerializedObject);
                field.RegisterValueChangeCallback(_ => UpdatePreview(node));
                detailPanel.Add(field);
            }
            while (prop.NextVisible(false));
        }

        RenderPersonaSection(node);
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
        foreach (NPCType type in Enum.GetValues(typeof(NPCType)))
        {
            Button tabBtn = new Button(() =>
            {
                selectedPersonaTab = type;
                if (graphView != null && graphView.selection != null)
                {
                    SequenceNodeView selected = graphView.selection.Find(e => e is SequenceNodeView) as SequenceNodeView;
                    if (selected != null) OnSequenceNodeSelected(selected);
                }
            })
            { text = type.ToString() };
            if (type == selectedPersonaTab)
            {
                tabBtn.style.backgroundColor = new Color(0.2f, 0.4f, 0.6f);
            }
            personaToolbar.Add(tabBtn);
        }
        section.Add(personaToolbar);

        SerializedProperty personaListProp = selectedSerializedObject.FindProperty("personaDialogues");
        int targetIndex = -1;
        if (personaListProp != null)
        {
            for (int i = 0; i < personaListProp.arraySize; i++)
            {
                SerializedProperty groupProp = personaListProp.GetArrayElementAtIndex(i);
                SerializedProperty personalityProp = groupProp.FindPropertyRelative("personality");
                if (personalityProp != null && (NPCType)personalityProp.enumValueIndex == selectedPersonaTab)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex >= 0)
            {
                SerializedProperty groupProp = personaListProp.GetArrayElementAtIndex(targetIndex);
                PropertyField linesField = new PropertyField(groupProp.FindPropertyRelative("lines"), $"{selectedPersonaTab} 대사 목록");
                linesField.Bind(selectedSerializedObject);
                section.Add(linesField);
            }
            else
            {
                Button addGroupBtn = new Button(() =>
                {
                    selectedSerializedObject.Update();
                    personaListProp.arraySize++;
                    SerializedProperty newElement = personaListProp.GetArrayElementAtIndex(personaListProp.arraySize - 1);
                    newElement.FindPropertyRelative("personality").enumValueIndex = (int)selectedPersonaTab;
                    newElement.FindPropertyRelative("lines").ClearArray();
                    selectedSerializedObject.ApplyModifiedProperties();

                    if (graphView != null && graphView.selection != null)
                    {
                        SequenceNodeView selected = graphView.selection.Find(e => e is SequenceNodeView) as SequenceNodeView;
                        if (selected != null) OnSequenceNodeSelected(selected);
                    }
                })
                { text = $"+ [{selectedPersonaTab}] 페르소나 대사 그룹 추가" };
                section.Add(addGroupBtn);
            }
        }

        detailPanel.Add(section);
    }

    private void UpdatePreview(SequenceNode node)
    {
        if (node == null) return;

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