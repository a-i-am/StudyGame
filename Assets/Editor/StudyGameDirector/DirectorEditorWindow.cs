using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System.Collections.Generic;

namespace StudyGame.Editor.Director
{
    public class DirectorEditorWindow : EditorWindow
    {
        private DirectorGraphView _graphView;

        private IMGUIContainer _masterMonitor;
        private IMGUIContainer _floorplanMonitor;
        private Camera _proxyCamera;
        private Camera _floorplanCamera;
        
        // Interactive Camera State
        private Vector2 _masterCameraRotation = new Vector2(45, 0); // Pitch, Yaw
        private Vector3 _masterCameraPivot = Vector3.zero;
        private float _masterCameraDistance = 15f;
        
        private Vector3 _floorplanCameraPivot = Vector3.zero;
        private float _floorplanCameraSize = 15f;

        private string _draggingNodeId = null;
        private string _hoveredNodeId = null;
        private Vector3 _dragPlanePoint;

        private string _currentSavePath = null;
        
        private bool _isBrushMode = false;
        private LevelGeometryData _levelGeometry = new LevelGeometryData();

        [MenuItem("StudyGame/Director Hub")]
        public static void ShowWindow()
        {
            DirectorEditorWindow wnd = GetWindow<DirectorEditorWindow>();
            wnd.titleContent = new GUIContent("Director Hub");
            wnd.minSize = new Vector2(800, 600);
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.focusable = true;
            root.RegisterCallback<KeyDownEvent>(OnKeyDown);

            // Main horizontal split
            var mainSplitView = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(mainSplitView);

            // Left Pane (Palette)
            var palettePane = new ScrollView(ScrollViewMode.Vertical);
            palettePane.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
            palettePane.style.paddingLeft = 10;
            palettePane.style.paddingRight = 10;
            palettePane.style.paddingTop = 15;
            
            var paletteHeader = new VisualElement();
            paletteHeader.style.flexDirection = FlexDirection.Column;
            paletteHeader.style.marginBottom = 15;
            
            var titleRow = new VisualElement();
            titleRow.style.flexDirection = FlexDirection.Row;
            titleRow.style.justifyContent = Justify.SpaceBetween;

            var paletteTitle = new Label("🎨 Floor Planner");
            paletteTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            paletteTitle.style.fontSize = 14;
            titleRow.Add(paletteTitle);
            
            var actionContainer = new VisualElement();
            actionContainer.style.flexDirection = FlexDirection.Row;
            actionContainer.Add(new Button(QuickSaveData) { text = "💾 Save" });
            actionContainer.Add(new Button(SaveDataAs) { text = "Save As" });
            actionContainer.Add(new Button(LoadData) { text = "📂 Load" });
            titleRow.Add(actionContainer);
            paletteHeader.Add(titleRow);

            var brushContainer = new VisualElement();
            brushContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.3f, 0.1f, 0.8f));
            brushContainer.style.paddingLeft = 5; brushContainer.style.paddingRight = 5;
            brushContainer.style.paddingTop = 5; brushContainer.style.paddingBottom = 5;
            brushContainer.style.marginTop = 10;
            brushContainer.style.marginBottom = 10;
            
            var brushToggle = new Toggle("🖌️ Brush Mode (Shift+Click to Erase)");
            brushToggle.value = _isBrushMode;
            brushToggle.RegisterValueChangedCallback(evt => _isBrushMode = evt.newValue);
            brushContainer.Add(brushToggle);
            
            var gridField = new FloatField("Grid Size") { value = _levelGeometry.GridSize };
            gridField.RegisterValueChangedCallback(evt => _levelGeometry.GridSize = evt.newValue);
            brushContainer.Add(gridField);
            
            var wallToggle = new Toggle("Auto Walls") { value = _levelGeometry.GenerateWalls };
            wallToggle.RegisterValueChangedCallback(evt => { _levelGeometry.GenerateWalls = evt.newValue; LevelGeometryManager.Rebuild(_levelGeometry); });
            brushContainer.Add(wallToggle);

            var ceilToggle = new Toggle("Auto Ceiling") { value = _levelGeometry.GenerateCeiling };
            ceilToggle.RegisterValueChangedCallback(evt => { _levelGeometry.GenerateCeiling = evt.newValue; LevelGeometryManager.Rebuild(_levelGeometry); });
            brushContainer.Add(ceilToggle);
            
            var wallHeightField = new FloatField("Wall Height") { value = _levelGeometry.WallHeight };
            wallHeightField.RegisterValueChangedCallback(evt => { _levelGeometry.WallHeight = evt.newValue; LevelGeometryManager.Rebuild(_levelGeometry); });
            brushContainer.Add(wallHeightField);

            paletteHeader.Add(brushContainer);

            palettePane.Add(paletteHeader);
            
            void AddPaletteItem(string label, string title, DirectorNodeType type)
            {
                var container = new VisualElement();
                container.style.marginBottom = 10;
                container.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f, 0.5f));
                container.style.paddingLeft = 5; container.style.paddingRight = 5; 
                container.style.paddingTop = 5; container.style.paddingBottom = 5;
                
                var pathField = new TextField("Prefab Path");
                pathField.value = EditorPrefs.GetString($"StudyGameDirector_PrefabPath_{type}", "Assets/Prefabs");
                pathField.RegisterValueChangedCallback(evt => {
                    EditorPrefs.SetString($"StudyGameDirector_PrefabPath_{type}", evt.newValue);
                });
                container.Add(pathField);

                var btn = new Button(() => CreateNode(title, type)) { text = label };
                container.Add(btn);

                palettePane.Add(container);
            }

            AddPaletteItem("📂 Database Hub", "Database Hub", DirectorNodeType.Database);
            AddPaletteItem("🪑 Desk (120cm)", "Desk", DirectorNodeType.Desk);
            AddPaletteItem("🛏️ Bed (Single)", "Bed", DirectorNodeType.Bed);
            AddPaletteItem("🧍 NPC Spawn Point", "NPC Point", DirectorNodeType.NPC);
            AddPaletteItem("⚡ Event Trigger", "Event Trigger", DirectorNodeType.Event);
            AddPaletteItem("✨ VFX Effect", "VFX Effect", DirectorNodeType.Effect);
            AddPaletteItem("⬛ Floor Template", "Floor Template", DirectorNodeType.FloorTemplate);
            
            mainSplitView.Add(palettePane);

            // Right Pane Container
            var rightContainer = new VisualElement();
            rightContainer.style.flexGrow = 1;

            // Vertical Split (Top Monitors / Bottom Graph)
            var verticalSplit = new TwoPaneSplitView(0, 400, TwoPaneSplitViewOrientation.Vertical);
            rightContainer.Add(verticalSplit);

            // Top Monitors (Horizontal Split)
            var topMonitorsSplit = new TwoPaneSplitView(0, 400, TwoPaneSplitViewOrientation.Horizontal);
            verticalSplit.Add(topMonitorsSplit);

            // Top-Left: 2D Floorplan
            var floorplanContainer = new VisualElement();
            floorplanContainer.style.flexGrow = 1;
            floorplanContainer.style.backgroundColor = Color.black;
            
            _floorplanMonitor = new IMGUIContainer(DrawFloorplanMonitor);
            _floorplanMonitor.style.flexGrow = 1;
            floorplanContainer.Add(_floorplanMonitor);
            topMonitorsSplit.Add(floorplanContainer);

            // Top-Right: 3D Master View
            var masterContainer = new VisualElement();
            masterContainer.style.flexGrow = 1;
            masterContainer.style.backgroundColor = Color.black;

            _masterMonitor = new IMGUIContainer(DrawMasterMonitor);
            _masterMonitor.style.flexGrow = 1;
            masterContainer.Add(_masterMonitor);
            topMonitorsSplit.Add(masterContainer);

            // Bottom: Logic Graph
            _graphView = new DirectorGraphView();
            _graphView.style.flexGrow = 1;
            verticalSplit.Add(_graphView);

            mainSplitView.Add(rightContainer);

            EditorApplication.update += RepaintMonitors;
        }

        private void CreateNode(string title, DirectorNodeType type = DirectorNodeType.Event)
        {
            var node = new WindowNode(title, type);
            int count = _graphView.nodes.ToList().Count;
            
            // Offset UI Position
            float offsetUI = (count % 10) * 20f;
            node.SetPosition(new Rect(150 + offsetUI, 150 + offsetUI, 0, 0));
            
            // Offset World Position
            Vector3 offsetWorld = new Vector3((count % 5) * 1.5f, 0, (count % 5) * 1.5f);
            node.SetWorldPosition(_masterCameraPivot + offsetWorld);
            
            _graphView.AddElement(node);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.ctrlKey && evt.keyCode == KeyCode.S)
            {
                QuickSaveData();
                evt.StopPropagation();
            }
        }

        private void QuickSaveData()
        {
            if (string.IsNullOrEmpty(_currentSavePath))
            {
                SaveDataAs();
            }
            else
            {
                SaveData(_currentSavePath);
            }
        }

        private void SaveDataAs()
        {
            string path = EditorUtility.SaveFilePanel("Save Floor Planner", "Assets", "DirectorData", "json");
            if (!string.IsNullOrEmpty(path))
            {
                _currentSavePath = path;
                SaveData(path);
            }
        }

        private void SaveData(string path)
        {
            var data = new DirectorSaveData();
            data.LevelGeometry = _levelGeometry;
            
            foreach (var node in _graphView.nodes.ToList())
            {
                if (node is WindowNode wNode)
                {
                    data.Nodes.Add(new DirectorNodeData
                    {
                        NodeId = wNode.NodeId,
                        NodeType = wNode.NodeType,
                        Title = wNode.WindowTitle,
                        Position = wNode.GetPosition(),
                        NodeColor = wNode.NodeColor,
                        WorldPosition = wNode.WorldPosition
                    });
                }
            }
            string json = JsonUtility.ToJson(data, true);
            System.IO.File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            Debug.Log($"Floor Planner saved to {path}!");
        }

        private void LoadData()
        {
            string path = EditorUtility.OpenFilePanel("Load Floor Planner", "Assets", "json");
            if (string.IsNullOrEmpty(path)) return;

            string json = System.IO.File.ReadAllText(path);
            var data = JsonUtility.FromJson<DirectorSaveData>(json);
            if (data == null) return;
            
            // Clear current graph
            _graphView.DeleteElements(_graphView.nodes.ToList());
            NodeProxyManager.ClearAll();

            _currentSavePath = path;
            
            if (data.LevelGeometry != null)
            {
                _levelGeometry = data.LevelGeometry;
                LevelGeometryManager.Rebuild(_levelGeometry);
            }

            foreach (var n in data.Nodes)
            {
                var node = new WindowNode(n.Title, n.NodeType);
                node.NodeColor = n.NodeColor;
                node.SetPosition(n.Position);
                node.SetWorldPosition(n.WorldPosition); // Explicitly set 3D pos
                _graphView.AddElement(node);
            }
            Debug.Log($"Floor Planner loaded from {path}!");
        }

        private void RepaintMonitors()
        {
            if (_masterMonitor != null) _masterMonitor.MarkDirtyRepaint();
            if (_floorplanMonitor != null) _floorplanMonitor.MarkDirtyRepaint();
        }

        private WindowNode GetNodeById(string id)
        {
            foreach (var node in _graphView.nodes.ToList())
            {
                if (node is WindowNode wNode && wNode.NodeId == id) return wNode;
            }
            return null;
        }

        private void HandleInteraction(Rect rect, Camera cam, Event e)
        {
            if (!rect.Contains(e.mousePosition) && _draggingNodeId == null) return;

            // Brush Mode Logic
            if (_isBrushMode && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
            {
                Ray ray = cam.ViewportPointToRay(new Vector3(
                    (e.mousePosition.x - rect.x) / rect.width,
                    1f - (e.mousePosition.y - rect.y) / rect.height, 0));
                
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                if (plane.Raycast(ray, out float enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    float s = _levelGeometry.GridSize;
                    int cx = Mathf.FloorToInt(hitPoint.x / s);
                    int cz = Mathf.FloorToInt(hitPoint.z / s);
                    Vector2Int cell = new Vector2Int(cx, cz);
                    
                    bool changed = false;
                    if (e.shift) // Erase
                    {
                        if (_levelGeometry.FloorCells.Contains(cell))
                        {
                            _levelGeometry.FloorCells.Remove(cell);
                            changed = true;
                        }
                    }
                    else // Paint
                    {
                        if (!_levelGeometry.FloorCells.Contains(cell))
                        {
                            _levelGeometry.FloorCells.Add(cell);
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        LevelGeometryManager.Rebuild(_levelGeometry);
                    }
                }
                e.Use();
                return;
            }

            // Detect Hover
            if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
            {
                Ray ray = cam.ViewportPointToRay(new Vector3(
                    (e.mousePosition.x - rect.x) / rect.width,
                    1f - (e.mousePosition.y - rect.y) / rect.height, 0));
                
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.gameObject.name.StartsWith("NodeProxy_"))
                    {
                        _hoveredNodeId = hit.collider.gameObject.name.Replace("NodeProxy_", "");
                    }
                    else _hoveredNodeId = null;
                }
                else _hoveredNodeId = null;
            }

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                // Raycast to find object
                Ray ray = cam.ViewportPointToRay(new Vector3(
                    (e.mousePosition.x - rect.x) / rect.width,
                    1f - (e.mousePosition.y - rect.y) / rect.height, 0));
                
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.gameObject.name.StartsWith("NodeProxy_"))
                    {
                        _draggingNodeId = hit.collider.gameObject.name.Replace("NodeProxy_", "");
                        _dragPlanePoint = hit.point;
                        
                        var node = GetNodeById(_draggingNodeId);
                        if (node != null)
                        {
                            _graphView.ClearSelection();
                            _graphView.AddToSelection(node);
                            // Removed FrameSelection to avoid jarring camera movement/flicker
                        }
                        
                        e.Use();
                    }
                }
            }
            else if (e.type == EventType.MouseDrag && e.button == 0 && _draggingNodeId != null)
            {
                // Drag on XZ plane
                Ray ray = cam.ViewportPointToRay(new Vector3(
                    (e.mousePosition.x - rect.x) / rect.width,
                    1f - (e.mousePosition.y - rect.y) / rect.height, 0));
                
                Plane plane = new Plane(Vector3.up, _dragPlanePoint);
                if (plane.Raycast(ray, out float enter))
                {
                    Vector3 newPos = ray.GetPoint(enter);
                    
                    // Snap to grid
                    float s = _levelGeometry.GridSize;
                    newPos.x = Mathf.Round(newPos.x / s) * s;
                    newPos.z = Mathf.Round(newPos.z / s) * s;
                    
                    var node = GetNodeById(_draggingNodeId);
                    if (node != null)
                    {
                        node.SetWorldPosition(newPos);
                    }
                }
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                _draggingNodeId = null;
            }
        }

        private void DrawFloatingUI(Rect rect, Event e)
        {
            string targetId = _hoveredNodeId ?? _draggingNodeId;
            if (targetId == null && _graphView.selection.Count > 0)
            {
                var selectedNode = _graphView.selection[0] as WindowNode;
                if (selectedNode != null) targetId = selectedNode.NodeId;
            }

            if (targetId != null)
            {
                var node = GetNodeById(targetId);
                if (node != null)
                {
                    // Draw in top right corner to avoid clipping
                    float boxWidth = 200;
                    float boxHeight = 110;
                    Rect uiRect = new Rect(rect.width - boxWidth - 10, 10, boxWidth, boxHeight);
                    
                    // Draw semi-transparent background
                    EditorGUI.DrawRect(uiRect, new Color(0.1f, 0.1f, 0.1f, 0.9f));
                    
                    GUIStyle titleStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 14, normal = { textColor = Color.white } };
                    GUIStyle textStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = new Color(0.8f, 0.8f, 0.8f) } };
                    
                    GUI.Label(new Rect(uiRect.x + 10, uiRect.y + 10, uiRect.width - 20, 20), $"📌 {node.WindowTitle}", titleStyle);
                    GUI.Label(new Rect(uiRect.x + 10, uiRect.y + 35, uiRect.width - 20, 20), $"Type: {node.NodeType}", textStyle);
                    GUI.Label(new Rect(uiRect.x + 10, uiRect.y + 55, uiRect.width - 20, 20), $"Pos: {node.WorldPosition.x:F1}, {node.WorldPosition.y:F1}, {node.WorldPosition.z:F1}", textStyle);
                    
                    // Color box
                    EditorGUI.DrawRect(new Rect(uiRect.x + 10, uiRect.y + 80, 20, 20), node.NodeColor);
                }
            }
        }

        private void DrawFloorplanMonitor()
        {
            Rect rect = _floorplanMonitor.layout;
            if (rect.width <= 1 || rect.height <= 1) return;

            if (_floorplanCamera == null)
            {
                var go = new GameObject("FloorplanCamera");
                go.hideFlags = HideFlags.HideAndDontSave;
                _floorplanCamera = go.AddComponent<Camera>();
                _floorplanCamera.orthographic = true;
                _floorplanCamera.clearFlags = CameraClearFlags.SolidColor;
                _floorplanCamera.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            }

            Event e = Event.current;
            HandleInteraction(rect, _floorplanCamera, e);

            if (rect.Contains(e.mousePosition))
            {
                if (e.type == EventType.ScrollWheel)
                {
                    _floorplanCameraSize += e.delta.y * 0.5f;
                    _floorplanCameraSize = Mathf.Clamp(_floorplanCameraSize, 2f, 100f);
                    e.Use();
                }
                else if (e.type == EventType.MouseDrag && e.button == 2) // Middle click pan
                {
                    _floorplanCameraPivot += new Vector3(-e.delta.x, 0, e.delta.y) * 0.05f * (_floorplanCameraSize / 10f);
                    e.Use();
                }
            }

            if (e.type != EventType.Repaint) return;

            _floorplanCamera.transform.position = _floorplanCameraPivot + new Vector3(0, 100, 0);
            _floorplanCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
            _floorplanCamera.orthographicSize = _floorplanCameraSize;
            _floorplanCamera.aspect = rect.width / rect.height;

            var rt = RenderTexture.GetTemporary((int)rect.width, (int)rect.height, 24);
            _floorplanCamera.targetTexture = rt;
            _floorplanCamera.Render();
            _floorplanCamera.targetTexture = null;

            GUI.DrawTexture(new Rect(0, 0, rect.width, rect.height), rt, ScaleMode.StretchToFill);
            RenderTexture.ReleaseTemporary(rt);

            // Draw Grid Visualizer
            Handles.BeginGUI();
            Handles.color = new Color(1, 1, 1, 0.1f);
            float s = Mathf.Max(0.1f, _levelGeometry.GridSize); // PREVENT INFINITE LOOP
            Vector3 center = _floorplanCameraPivot;
            float camSize = _floorplanCameraSize;
            float step = s;
            
            // Draw vertical lines
            for (float x = Mathf.Floor(center.x - camSize); x <= Mathf.Ceil(center.x + camSize); x += step)
            {
                float nx = Mathf.Round(x / s) * s;
                Vector3 p1 = _floorplanCamera.WorldToScreenPoint(new Vector3(nx, 0, center.z - camSize));
                Vector3 p2 = _floorplanCamera.WorldToScreenPoint(new Vector3(nx, 0, center.z + camSize));
                p1.y = rect.height - p1.y; p2.y = rect.height - p2.y;
                Handles.DrawLine(p1, p2);
            }
            // Draw horizontal lines
            for (float z = Mathf.Floor(center.z - camSize); z <= Mathf.Ceil(center.z + camSize); z += step)
            {
                float nz = Mathf.Round(z / s) * s;
                Vector3 p1 = _floorplanCamera.WorldToScreenPoint(new Vector3(center.x - camSize, 0, nz));
                Vector3 p2 = _floorplanCamera.WorldToScreenPoint(new Vector3(center.x + camSize, 0, nz));
                p1.y = rect.height - p1.y; p2.y = rect.height - p2.y;
                Handles.DrawLine(p1, p2);
            }
            Handles.EndGUI();

            GUIStyle labelStyle = new GUIStyle();
            labelStyle.richText = true;
            labelStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(10, 10, 500, 30), "<b>📐 2D FLOOR PLAN (클릭:이동 | 휠버튼:패닝 | 스크롤:줌 | Shift+클릭: 지우기)</b>", labelStyle);

            DrawFloatingUI(rect, e);
        }

        private void DrawMasterMonitor()
        {
            Rect rect = _masterMonitor.layout;
            if (rect.width <= 1 || rect.height <= 1) return;

            if (_proxyCamera == null)
            {
                var go = new GameObject("MasterMonitorCamera");
                go.hideFlags = HideFlags.HideAndDontSave;
                _proxyCamera = go.AddComponent<Camera>();
            }

            Event e = Event.current;
            HandleInteraction(rect, _proxyCamera, e);

            if (rect.Contains(e.mousePosition))
            {
                if (e.type == EventType.ScrollWheel)
                {
                    _masterCameraDistance += e.delta.y * 1.5f;
                    _masterCameraDistance = Mathf.Clamp(_masterCameraDistance, 2f, 100f);
                    e.Use();
                }
                else if (e.type == EventType.MouseDrag)
                {
                    if (e.button == 1) // Right click orbit
                    {
                        _masterCameraRotation.y += e.delta.x * 0.5f;
                        _masterCameraRotation.x += e.delta.y * 0.5f;
                        _masterCameraRotation.x = Mathf.Clamp(_masterCameraRotation.x, 5f, 85f);
                        e.Use();
                    }
                    else if (e.button == 2) // Middle click pan
                    {
                        Quaternion rot = Quaternion.Euler(0, _masterCameraRotation.y, 0);
                        Vector3 right = rot * Vector3.right;
                        Vector3 forward = rot * Vector3.forward;
                        _masterCameraPivot -= (right * e.delta.x - forward * e.delta.y) * 0.02f * (_masterCameraDistance / 10f);
                        e.Use();
                    }
                }
            }

            if (e.type != EventType.Repaint) return;
            if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null)
            {
                _proxyCamera.CopyFrom(SceneView.lastActiveSceneView.camera);
            }
            
            Quaternion camRot = Quaternion.Euler(_masterCameraRotation.x, _masterCameraRotation.y, 0);
            Vector3 camPos = _masterCameraPivot - camRot * Vector3.forward * _masterCameraDistance;
            
            _proxyCamera.transform.position = camPos;
            _proxyCamera.transform.rotation = camRot;
            _proxyCamera.aspect = rect.width / rect.height;
            _proxyCamera.clearFlags = CameraClearFlags.Skybox;

            var rt = RenderTexture.GetTemporary((int)rect.width, (int)rect.height, 24);
            _proxyCamera.targetTexture = rt;
            _proxyCamera.Render();
            _proxyCamera.targetTexture = null;

            GUI.DrawTexture(new Rect(0, 0, rect.width, rect.height), rt, ScaleMode.StretchToFill);
            RenderTexture.ReleaseTemporary(rt);
            
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.richText = true;
            labelStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(10, 10, 400, 30), "<b>🎥 MASTER SCENE VIEW (우클릭:회전 | 휠버튼:패닝 | 스크롤:줌)</b>", labelStyle);

            DrawFloatingUI(rect, e);
        }

        private void OnDestroy()
        {
            EditorApplication.update -= RepaintMonitors;
            if (_proxyCamera != null) DestroyImmediate(_proxyCamera.gameObject);
            if (_floorplanCamera != null) DestroyImmediate(_floorplanCamera.gameObject);
        }
    }
}
