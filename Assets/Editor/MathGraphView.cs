#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class MathGraphView : GraphView
{
    public MathGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach(port =>
        {
            if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
            {
                compatiblePorts.Add(port);
            }
        });
        return compatiblePorts;
    }

    public void CreateInputNode(Vector2 position)
    {
        var node = new MathInputNode();
        node.SetPosition(new Rect(position, new Vector2(220, 160)));
        AddElement(node);
    }

    public void CreateOperationNode(Vector2 position)
    {
        var node = new MathOperationNode();
        node.SetPosition(new Rect(position, new Vector2(220, 160)));
        AddElement(node);
    }

    public void CreateOutputNode(Vector2 position)
    {
        var node = new MathOutputNode();
        node.SetPosition(new Rect(position, new Vector2(220, 160)));
        AddElement(node);
    }
}
#endif

