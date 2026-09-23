using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace StudyGame.Editor.Director
{
    public enum DirectorNodeType
    {
        Database,
        Desk,
        Bed,
        NPC,
        Event,
        Effect,
        FloorTemplate
    }

    public class WindowNode : Node
    {
        public string NodeId { get; private set; }
        public DirectorNodeType NodeType { get; private set; }
        public Color NodeColor { get; set; } = Color.cyan;
        public Vector3 WorldPosition { get; set; } = Vector3.zero;
        public string WindowTitle { get; private set; }
        public VisualElement ContentContainer { get; private set; }
        
        private Camera _cctvCamera;
        private IMGUIContainer _cctvViewport;

        public WindowNode(string title, DirectorNodeType type = DirectorNodeType.Event)
        {
            NodeId = System.Guid.NewGuid().ToString();
            NodeType = type;
            WindowTitle = title;
            this.title = title;

            // Set default colors based on type
            switch (type)
            {
                case DirectorNodeType.Database: NodeColor = new Color(0.2f, 0.6f, 1f); break; // Blue
                case DirectorNodeType.Desk: NodeColor = new Color(0.8f, 0.5f, 0.2f); break; // Orange/Brown
                case DirectorNodeType.Bed: NodeColor = new Color(0.9f, 0.2f, 0.2f); break; // Red
                case DirectorNodeType.NPC: NodeColor = new Color(0.2f, 0.9f, 0.2f); break; // Green
                case DirectorNodeType.Event: NodeColor = new Color(0.9f, 0.9f, 0.2f); break; // Yellow
                case DirectorNodeType.Effect: NodeColor = new Color(0.8f, 0.2f, 0.8f); break; // Purple
                case DirectorNodeType.FloorTemplate: NodeColor = new Color(0.6f, 0.6f, 0.6f); break; // Gray
            }

            // Customize the node to look more like a window
            mainContainer.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f, 0.95f));
            mainContainer.style.borderTopLeftRadius = 8;
            mainContainer.style.borderTopRightRadius = 8;
            mainContainer.style.borderBottomLeftRadius = 8;
            mainContainer.style.borderBottomRightRadius = 8;
            mainContainer.style.overflow = Overflow.Hidden;

            // Add ports for flowchart connection
            var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            // Container for custom editor panels
            ContentContainer = new VisualElement();
            ContentContainer.style.paddingLeft = 8;
            ContentContainer.style.paddingRight = 8;
            ContentContainer.style.paddingTop = 8;
            ContentContainer.style.paddingBottom = 8;
            ContentContainer.style.minWidth = 300;
            ContentContainer.style.minHeight = 200;
            
            // --- CCTV Viewport ---
            _cctvViewport = new IMGUIContainer(DrawCCTV);
            _cctvViewport.style.height = 150;
            _cctvViewport.style.marginBottom = 5;
            _cctvViewport.style.backgroundColor = Color.black;
            
            var cctvTitleContainer = new VisualElement();
            cctvTitleContainer.style.flexDirection = FlexDirection.Row;
            cctvTitleContainer.style.alignItems = Align.Center;
            cctvTitleContainer.style.paddingLeft = 8;
            cctvTitleContainer.style.paddingTop = 4;
            cctvTitleContainer.style.paddingBottom = 4;
            cctvTitleContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f));

            var cctvIndicator = new VisualElement();
            cctvIndicator.style.width = 8;
            cctvIndicator.style.height = 8;
            cctvIndicator.style.borderTopLeftRadius = 4;
            cctvIndicator.style.borderTopRightRadius = 4;
            cctvIndicator.style.borderBottomLeftRadius = 4;
            cctvIndicator.style.borderBottomRightRadius = 4;
            cctvIndicator.style.backgroundColor = Color.red;
            cctvIndicator.style.marginRight = 5;

            var cctvTitle = new Label("LIVE CCTV FEED");
            cctvTitle.style.color = Color.white;
            cctvTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            cctvTitleContainer.Add(cctvIndicator);
            cctvTitleContainer.Add(cctvTitle);

            extensionContainer.Add(cctvTitleContainer);
            
            // Add Color Picker
            var colorField = new UnityEditor.UIElements.ColorField("Node Color")
            {
                value = NodeColor
            };
            colorField.RegisterValueChangedCallback(evt =>
            {
                NodeColor = evt.newValue;
                if (this.panel != null)
                {
                    NodeProxyManager.UpdateProxy(this.NodeId, WorldPosition, NodeColor, this.NodeType);
                }
            });
            extensionContainer.Add(colorField);
            
            extensionContainer.Add(_cctvViewport);
            extensionContainer.Add(ContentContainer);

            // Sync 2D position with 3D Hologram Renderer and Camera
            this.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (this.panel == null) return;
                // No longer moves the 3D proxy! Just updates CCTV layout rendering internally if needed.
            });
            this.RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                NodeProxyManager.RemoveProxy(this.NodeId);
                CleanupCamera();
            });

            RefreshExpandedState();
            RefreshPorts();
            
            EditorApplication.update += RepaintNode;
        }

        public void SetWorldPosition(Vector3 newWorldPos)
        {
            WorldPosition = newWorldPos;
            NodeProxyManager.UpdateProxy(NodeId, WorldPosition, NodeColor, this.NodeType);
            UpdateCameraPosition();
        }

        private double _lastRepaintTime = 0;
        private void RepaintNode()
        {
            if (EditorApplication.timeSinceStartup - _lastRepaintTime < 0.033) return;
            _lastRepaintTime = EditorApplication.timeSinceStartup;

            if (this.panel != null && _cctvViewport != null)
            {
                _cctvViewport.MarkDirtyRepaint();
            }
        }

        private void UpdateCameraPosition()
        {
            if (_cctvCamera == null)
            {
                var go = new GameObject($"CCTV_{WindowTitle}");
                go.hideFlags = HideFlags.HideAndDontSave;
                _cctvCamera = go.AddComponent<Camera>();
                go.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                _cctvCamera.clearFlags = CameraClearFlags.Skybox;
                _cctvCamera.fieldOfView = 60f;
            }

            // Position camera slightly above and looking down at an angle
            _cctvCamera.transform.position = WorldPosition + new Vector3(0, 8f, -4f);
            _cctvCamera.transform.rotation = Quaternion.Euler(60f, 0, 0);
        }

        private void DrawCCTV()
        {
            if (_cctvCamera == null) return;
            if (Event.current.type != EventType.Repaint) return;

            Rect rect = _cctvViewport.layout; // Use layout for exact pixel dimensions
            if (rect.width <= 1 || rect.height <= 1) return;

            // Remove pixelRect assignment! When using targetTexture, camera uses the whole texture.
            _cctvCamera.aspect = rect.width / rect.height; // Fix clipping / letterboxing
            
            var rt = RenderTexture.GetTemporary((int)rect.width, (int)rect.height, 24);
            _cctvCamera.targetTexture = rt;
            _cctvCamera.Render();
            _cctvCamera.targetTexture = null;

            Rect drawRect = new Rect(0, 0, rect.width, rect.height);
            GUI.DrawTexture(drawRect, rt, ScaleMode.StretchToFill);
            RenderTexture.ReleaseTemporary(rt);
        }

        private void CleanupCamera()
        {
            EditorApplication.update -= RepaintNode;
            if (_cctvCamera != null)
            {
                Object.DestroyImmediate(_cctvCamera.gameObject);
                _cctvCamera = null;
            }
        }
    }
}
