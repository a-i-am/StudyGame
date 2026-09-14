#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class SequenceGraphView : GraphView
{
    private SequenceGraphData currentGraph;
    public Action<SequenceNodeView> OnNodeSelected;

    public SequenceGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        GridBackground grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        graphViewChanged = OnGraphViewChanged;
        Undo.undoRedoPerformed += OnUndoRedoPerformed;

        RegisterCallback<DetachFromPanelEvent>(evt =>
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        });
    }

    private void OnUndoRedoPerformed()
    {
        if (currentGraph != null)
        {
            PopulateView(currentGraph);
        }
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new List<Port>();
        ports.ForEach(port =>
        {
            if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
            {
                compatiblePorts.Add(port);
            }
        });
        return compatiblePorts;
    }

    public void PopulateView(SequenceGraphData graph)
    {
        currentGraph = graph;

        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged = OnGraphViewChanged;

        if (currentGraph == null || currentGraph.allNodes == null) return;

        Dictionary<string, SequenceNodeView> nodeViewMap = new Dictionary<string, SequenceNodeView>();

        foreach (SequenceNode nodeData in currentGraph.allNodes)
        {
            if (nodeData == null) continue;
            SequenceNodeView nodeView = CreateNodeView(nodeData);
            if (!string.IsNullOrEmpty(nodeData.guid))
            {
                nodeViewMap[nodeData.guid] = nodeView;
            }
        }

        foreach (SequenceNode nodeData in currentGraph.allNodes)
        {
            if (nodeData == null || nodeData.choices == null) continue;
            if (!nodeViewMap.TryGetValue(nodeData.guid, out SequenceNodeView sourceView)) continue;

            for (int i = 0; i < nodeData.choices.Count; i++)
            {
                DialogueChoice choice = nodeData.choices[i];
                if (choice.targetNode == null) continue;

                if (nodeViewMap.TryGetValue(choice.targetNode.guid, out SequenceNodeView targetView))
                {
                    if (i < sourceView.OutputPorts.Count)
                    {
                        Port outputPort = sourceView.OutputPorts[i];
                        Port inputPort = targetView.InputPort;
                        Edge edge = outputPort.ConnectTo(inputPort);
                        AddElement(edge);
                    }
                }
            }
        }
    }

    public SequenceNodeView CreateNodeView(SequenceNode nodeData)
    {
        SequenceNodeView nodeView = new SequenceNodeView(nodeData, nodeView => OnNodeSelected?.Invoke(nodeView));
        AddElement(nodeView);
        return nodeView;
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if (graphViewChange.elementsToRemove != null)
        {
            foreach (GraphElement element in graphViewChange.elementsToRemove)
            {
                if (element is Edge edge)
                {
                    if (edge.output != null && edge.output.userData is DialogueChoice choice)
                    {
                        if (edge.output.node is SequenceNodeView sourceNodeView)
                        {
                            Undo.RecordObject(sourceNodeView.NodeData, "Disconnect Edge");
                            choice.targetNode = null;
                            EditorUtility.SetDirty(sourceNodeView.NodeData);
                        }
                    }
                }
                else if (element is SequenceNodeView nodeView)
                {
                    SequenceNode nodeToDelete = nodeView.NodeData;
                    if (nodeToDelete != null)
                    {
                        if (currentGraph != null && currentGraph.allNodes != null)
                        {
                            foreach (SequenceNode otherNode in currentGraph.allNodes)
                            {
                                if (otherNode == null || otherNode == nodeToDelete || otherNode.choices == null) continue;
                                bool modified = false;
                                foreach (DialogueChoice choice in otherNode.choices)
                                {
                                    if (choice.targetNode == nodeToDelete)
                                    {
                                        Undo.RecordObject(otherNode, "Disconnect Target Node");
                                        choice.targetNode = null;
                                        modified = true;
                                    }
                                }
                                if (modified)
                                {
                                    EditorUtility.SetDirty(otherNode);
                                }
                            }

                            if (currentGraph.allNodes.Contains(nodeToDelete))
                            {
                                Undo.RecordObject(currentGraph, "Delete Node From Graph");
                                currentGraph.allNodes.Remove(nodeToDelete);
                                EditorUtility.SetDirty(currentGraph);
                            }
                        }

                        Undo.DestroyObjectImmediate(nodeToDelete);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }

        if (graphViewChange.edgesToCreate != null)
        {
            foreach (Edge edge in graphViewChange.edgesToCreate)
            {
                if (edge.output != null && edge.output.userData is DialogueChoice choice && edge.input != null)
                {
                    if (edge.input.node is SequenceNodeView targetNodeView && edge.output.node is SequenceNodeView sourceNodeView)
                    {
                        Undo.RecordObject(sourceNodeView.NodeData, "Connect Edge");
                        choice.targetNode = targetNodeView.NodeData;
                        EditorUtility.SetDirty(sourceNodeView.NodeData);
                    }
                }
            }
        }

        return graphViewChange;
    }
}
#endif
