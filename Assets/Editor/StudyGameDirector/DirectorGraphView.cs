using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace StudyGame.Editor.Director
{
    public class DirectorGraphView : GraphView
    {
        private GridBackground _gridBackground;

        public DirectorGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            _gridBackground = new GridBackground();
            Insert(0, _gridBackground);
            _gridBackground.StretchToParentSize();
            
            // Add Stylesheet if needed
            var styleSheet = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/StudyGameDirector/UI/DirectorGraphView.uss");
            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }

            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerform);
        }

        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            if (UnityEditor.DragAndDrop.objectReferences.Length > 0)
            {
                UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Copy;
            }
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            if (UnityEditor.DragAndDrop.objectReferences.Length > 0)
            {
                Vector2 localMousePos = contentViewContainer.WorldToLocal(evt.mousePosition);
                foreach (var obj in UnityEditor.DragAndDrop.objectReferences)
                {
                    if (obj is StudyGame.Data.WorkspaceNodeData nodeData)
                    {
                        var node = new ScenarioSequenceNode(nodeData);
                        node.SetPosition(new Rect(localMousePos, new Vector2(200, 150)));
                        AddElement(node);
                        localMousePos += new Vector2(20, 20); // offset if multiple
                    }
                }
            }
        }

        public void SetOverlayMode(bool isOverlay)
        {
            if (isOverlay)
            {
                this.style.backgroundColor = new StyleColor(UnityEngine.Color.clear);
                _gridBackground.visible = false;
            }
            else
            {
                this.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.12f, 0.12f, 0.12f, 1.0f));
                _gridBackground.visible = true;
            }
        }

        public void ToggleTransparency(bool isOverlay)
        {
            if (_gridBackground != null)
            {
                _gridBackground.style.opacity = isOverlay ? 0.3f : 1.0f;
            }
        }

        public override System.Collections.Generic.List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new System.Collections.Generic.List<Port>();
            
            ports.ForEach(port =>
            {
                // Can't connect to same port, same node, or same direction (input->input)
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });
            
            return compatiblePorts;
        }
    }
}
