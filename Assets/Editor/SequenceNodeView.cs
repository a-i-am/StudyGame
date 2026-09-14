#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class SequenceNodeView : Node
{
    public SequenceNode NodeData { get; private set; }
    public Port InputPort { get; private set; }
    public List<Port> OutputPorts { get; private set; } = new List<Port>();

    private Action<SequenceNodeView> onNodeSelectedCallback;

    public SequenceNodeView(SequenceNode nodeData, Action<SequenceNodeView> onNodeSelected)
    {
        NodeData = nodeData;
        onNodeSelectedCallback = onNodeSelected;
        viewDataKey = string.IsNullOrEmpty(nodeData.guid) ? Guid.NewGuid().ToString() : nodeData.guid;
        if (string.IsNullOrEmpty(nodeData.guid))
        {
            nodeData.guid = viewDataKey;
            EditorUtility.SetDirty(nodeData);
        }

        title = string.IsNullOrEmpty(nodeData.name) ? "Sequence Node" : nodeData.name;

        SetPosition(new Rect(nodeData.graphPosition, new Vector2(240, 180)));

        CreateInputPort();
        CreateAddChoiceButton();
        RefreshChoices();
    }

    public override void OnSelected()
    {
        base.OnSelected();
        onNodeSelectedCallback?.Invoke(this);
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        if (NodeData != null)
        {
            NodeData.graphPosition = newPos.position;
            EditorUtility.SetDirty(NodeData);
        }
    }

    private void CreateInputPort()
    {
        InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        InputPort.portName = "In";
        inputContainer.Add(InputPort);
    }

    private void CreateAddChoiceButton()
    {
        Button addChoiceBtn = new Button(AddChoice) { text = "+ 질문 선택지 추가" };
        titleButtonContainer.Add(addChoiceBtn);
    }

    private void AddChoice()
    {
        Undo.RecordObject(NodeData, "Add Choice");
        DialogueChoice choice = new DialogueChoice
        {
            portGuid = Guid.NewGuid().ToString(),
            choiceText = "New Choice",
            targetNode = null
        };
        NodeData.choices.Add(choice);
        EditorUtility.SetDirty(NodeData);
        RefreshChoices();
    }

    public void RefreshChoices()
    {
        outputContainer.Clear();
        OutputPorts.Clear();

        if (NodeData == null || NodeData.choices == null) return;

        for (int i = 0; i < NodeData.choices.Count; i++)
        {
            DialogueChoice choice = NodeData.choices[i];
            if (string.IsNullOrEmpty(choice.portGuid))
            {
                choice.portGuid = Guid.NewGuid().ToString();
            }

            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;

            Button deleteBtn = new Button(() => RemoveChoice(choice)) { text = "X" };
            row.Add(deleteBtn);

            TextField textProperty = new TextField();
            textProperty.value = choice.choiceText;
            textProperty.style.flexGrow = 1;
            textProperty.style.minWidth = 100;
            textProperty.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(NodeData, "Edit Choice Text");
                choice.choiceText = evt.newValue;
                EditorUtility.SetDirty(NodeData);
            });
            row.Add(textProperty);

            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = "";
            outputPort.userData = choice;
            OutputPorts.Add(outputPort);
            row.Add(outputPort);

            outputContainer.Add(row);
        }

        RefreshExpandedState();
        RefreshPorts();
    }

    private void RemoveChoice(DialogueChoice choice)
    {
        Undo.RecordObject(NodeData, "Remove Choice");
        NodeData.choices.Remove(choice);
        EditorUtility.SetDirty(NodeData);
        RefreshChoices();
    }
}
#endif
