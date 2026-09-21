using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System.Collections.Generic;

namespace StudyGame.Editor.Director
{
    public enum FloorplanTool
    {
        None,
        Brush,
        Rectangle,
        Bucket
    }

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
        
        private FloorplanTool _currentTool = FloorplanTool.None;
        private int _brushSize = 1;
        private LevelGeometryData _levelGeometry = new LevelGeometryData();

        private Vector2Int? _rectStartCell = null;
        private Vector2Int? _rectEndCell = null;
        private GameObject _rectPreviewObject = null;
        private Vector2Int? _lastPaintedCell = null;

        [MenuItem("StudyGame/Director Hub")]
        public static void ShowWindow()
        {
            DirectorEditorWindow wnd = GetWindow<DirectorEditorWindow>();
            wnd.titleContent = new GUIContent("Director Hub");
            wnd.minSize = new Vector2(800, 600);
        }

        private VisualElement _workspaceContainer;
        private VisualElement _directorHubContainer;

        private VisualElement _aiConsoleContainer;

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.focusable = true;
            root.RegisterCallback<KeyDownEvent>(OnKeyDown);
            
            DirectorStateManager.OnTabSwitchRequested += SwitchTab;

            // Global Toolbar
            var toolbar = new UnityEditor.UIElements.Toolbar();
            var btnWorkspace = new UnityEditor.UIElements.ToolbarButton(() => SwitchTab(0)) { text = "📝 워크스페이스" };
            var btnDirector = new UnityEditor.UIElements.ToolbarButton(() => SwitchTab(1)) { text = "🏗️ 디렉터 허브" };
            var btnAIConsole = new UnityEditor.UIElements.ToolbarButton(() => SwitchTab(2)) { text = "🧠 AI 에디터 확장 콘솔" };
            
            var btnSaveGraph = new UnityEditor.UIElements.ToolbarButton(SaveGraph) { text = "💾 저장" };
            var btnLoadGraph = new UnityEditor.UIElements.ToolbarButton(LoadGraph) { text = "📂 로드" };
            
            var toolsMenu = new UnityEditor.UIElements.ToolbarMenu { text = "🪄 시나리오 노드 연결" };
            toolsMenu.menu.AppendAction("생성 및 전체 연결 (모든 에피소드)", a => AutoGenerateAndConnectEpisodes("Assets/Resources/Scenarios/Episodes"));

            toolbar.Add(btnWorkspace);
            toolbar.Add(btnDirector);
            toolbar.Add(btnAIConsole);
            toolbar.Add(new VisualElement() { style = { flexGrow = 1 } }); // Spacer
            toolbar.Add(toolsMenu);
            toolbar.Add(btnSaveGraph);
            toolbar.Add(btnLoadGraph);
            root.Add(toolbar);

            _workspaceContainer = new VisualElement();
            _workspaceContainer.style.flexGrow = 1;
            BuildWorkspaceUI(_workspaceContainer);
            root.Add(_workspaceContainer);

            _directorHubContainer = new VisualElement();
            _directorHubContainer.style.flexGrow = 1;
            BuildDirectorHubUI(_directorHubContainer);
            root.Add(_directorHubContainer);

            _aiConsoleContainer = new VisualElement();
            _aiConsoleContainer.style.flexGrow = 1;
            BuildAIConsoleUI(_aiConsoleContainer);
            root.Add(_aiConsoleContainer);

            SwitchTab(0); // Default to Workspace
            EditorApplication.update += RepaintMonitors;
        }

        private void SwitchTab(int index)
        {
            _workspaceContainer.style.display = index == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _directorHubContainer.style.display = index == 1 ? DisplayStyle.Flex : DisplayStyle.None;
            _aiConsoleContainer.style.display = index == 2 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void BuildWorkspaceUI(VisualElement root)
        {
            // Apply USS
            var styleSheet = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/StudyGameDirector/Workspace/WorkspaceStyle.uss");
            if (styleSheet != null) root.styleSheets.Add(styleSheet);

            var mainSplit = new TwoPaneSplitView(0, 260, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(mainSplit);
            
            InspectorView inspector = null;

            // Left Explorer
            var explorer = new ScrollView();
            explorer.AddToClassList("workspace-panel");
            
            // Dynamic Tree from Episode Assets
            var episodeGuids = UnityEditor.AssetDatabase.FindAssets("t:WorkspaceNodeData", new[] { "Assets/Resources/Scenarios/Episodes", "Assets/Resources/Scenarios/Anomalies" });
            
            if (episodeGuids.Length == 0)
            {
                explorer.Add(new Label("에피소드 에셋이 없습니다. (상단 마술봉 버튼으로 생성하세요)"));
            }
            else
            {
                var episodes = new System.Collections.Generic.List<StudyGame.Data.WorkspaceNodeData>();
                foreach (var guid in episodeGuids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<StudyGame.Data.WorkspaceNodeData>(path);
                    if (asset != null) episodes.Add(asset);
                }
                
                episodes.Sort((a, b) => a.name.CompareTo(b.name));
                
                foreach (var ep in episodes)
                {
                    var header = new Label($"▼ {ep.NodeTitle}");
                    header.AddToClassList("workspace-section-header");
                    header.RegisterCallback<MouseUpEvent>(evt => {
                        SelectOrSpawnNode(ep, inspector);
                    });
                    explorer.Add(header);
                    
                    foreach (var prop in ep.Properties)
                    {
                        var pLabel = new Label($"  - 📜 {prop.PropertyName}");
                        pLabel.RegisterCallback<MouseUpEvent>(evt => {
                            SelectOrSpawnNode(ep, inspector);
                            var node = GetEpisodeNodeById(ep.NodeId);
                            if (node != null) DirectorStateManager.SetActiveContext(node, prop);
                        });
                        explorer.Add(pLabel);
                    }
                }
            }

            // Add Template drawer
            var drawerTitle = new Label("▼ 템플릿 서랍 (Templates)");
            drawerTitle.AddToClassList("workspace-section-header");
            explorer.Add(drawerTitle);

            // 동적 템플릿 로딩
            var templateGuids = UnityEditor.AssetDatabase.FindAssets("t:WorkspaceNodeData", new[] { "Assets/Editor/StudyGameDirector/Workspace/Templates" });
            foreach(var guid in templateGuids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var tpl = UnityEditor.AssetDatabase.LoadAssetAtPath<StudyGame.Data.WorkspaceNodeData>(path);
                if (tpl != null)
                {
                    explorer.Add(new Button(() => CreateWorkspaceNodeFromTemplate(tpl)) { text = $"{(string.IsNullOrEmpty(tpl.ThemeIcon) ? "📦" : tpl.ThemeIcon)} {tpl.NodeTitle}" });
                }
            }

            // 하드코딩된 기본 생성 버튼 (최초용)
            if (templateGuids.Length == 0)
            {
                explorer.Add(new Button(() => CreateWorkspaceNode("이벤트 시작: OnInteract", new string[]{"트리거 🚩", "ID: 빈칸"})) { text = "🚩 필드 상호작용 이벤트" });
                explorer.Add(new Button(() => CreateWorkspaceNode("대화 재생 (Dialogue)", new string[]{"대화 🗣️", "화자: 미지정"})) { text = "🗣️ 대화 재생 액션" });
                explorer.Add(new Button(() => CreateWorkspaceNode("캐릭터 DNA", new string[]{"아트 🎨"})) { text = "👗 캐릭터 DNA 튜너" });
                explorer.Add(new Button(() => CreateWorkspaceNode("보스 퍼즐", new string[]{"퍼즐 🧩"})) { text = "🧩 질문 조립기(보스전)" });
            }
            
            mainSplit.Add(explorer);

            var rightSplit = new TwoPaneSplitView(0, 500, TwoPaneSplitViewOrientation.Horizontal);
            mainSplit.Add(rightSplit);

            // Middle Canvas
            var canvasContainer = new VisualElement();
            canvasContainer.style.flexGrow = 1;
            var epGraph = new EpisodeGraphView();
            epGraph.style.flexGrow = 1;
            canvasContainer.Add(epGraph);
            rightSplit.Add(canvasContainer);

            // Right Inspector
            inspector = new InspectorView();
            rightSplit.Add(inspector);

            // Bind selection
            epGraph.RegisterCallback<MouseUpEvent>(evt => {
                var selection = epGraph.selection;
                if (selection.Count > 0 && selection[0] is EpisodeNode node)
                {
                    inspector.BindNode(node);
                    DirectorStateManager.SetActiveContext(node, null);
                }
            });
            
            // Wire up tree item clicks to inspector now that it's created
            foreach (var lbl in explorer.Children())
            {
                lbl.RegisterCallback<MouseUpEvent>(evt => {
                    if (_workspaceGraph.selection.Count > 0 && _workspaceGraph.selection[0] is EpisodeNode n)
                    {
                        inspector.BindNode(n);
                    }
                });
            }
            
            _workspaceGraph = epGraph;
        }

        private EpisodeGraphView _workspaceGraph;

        private EpisodeNode GetEpisodeNodeById(string id)
        {
            if (_workspaceGraph == null) return null;
            foreach (var elem in _workspaceGraph.graphElements)
            {
                if (elem is EpisodeNode node && node.NodeId == id)
                    return node;
            }
            return null;
        }

        private void SelectOrSpawnNode(StudyGame.Data.WorkspaceNodeData ep, InspectorView inspector)
        {
            if (_workspaceGraph == null) return;
            var node = GetEpisodeNodeById(ep.NodeId);
            if (node == null)
            {
                var badges = new System.Collections.Generic.List<string>();
                foreach (var prop in ep.Properties)
                {
                    if (prop.ShowAsBadge)
                    {
                        string val = prop.Type == StudyGame.Data.PropertyType.Text ? prop.StringValue : 
                                    (prop.Type == StudyGame.Data.PropertyType.Number ? prop.FloatValue.ToString() : "...");
                        badges.Add($"{prop.PropertyName}: {val}");
                    }
                }
                node = _workspaceGraph.CreateNode(ep.NodeTitle, new Vector2(300, 300), badges.ToArray());
                node.NodeId = ep.NodeId;
                node.NodeData = ep;
            }
            _workspaceGraph.ClearSelection();
            _workspaceGraph.AddToSelection(node);
            if (inspector != null) inspector.BindNode(node);
        }

        private void CreateWorkspaceNodeFromTemplate(StudyGame.Data.WorkspaceNodeData template)
        {
            if (_workspaceGraph != null)
            {
                var data = ScriptableObject.CreateInstance<StudyGame.Data.WorkspaceNodeData>();
                data.NodeTitle = template.NodeTitle;
                data.TemplateType = template.TemplateType;
                data.ThemeColorHex = template.ThemeColorHex;
                data.ThemeIcon = template.ThemeIcon;
                
                var badges = new System.Collections.Generic.List<string>();
                foreach (var prop in template.Properties)
                {
                    data.Properties.Add(new StudyGame.Data.DynamicProperty {
                        PropertyName = prop.PropertyName,
                        Type = prop.Type,
                        StringValue = prop.StringValue,
                        FloatValue = prop.FloatValue,
                        ColorValue = prop.ColorValue,
                        AssetValue = prop.AssetValue,
                        ShowAsBadge = prop.ShowAsBadge
                    });

                    if (prop.ShowAsBadge)
                    {
                        string val = prop.Type == StudyGame.Data.PropertyType.Text ? prop.StringValue : 
                                    (prop.Type == StudyGame.Data.PropertyType.Number ? prop.FloatValue.ToString() : "...");
                        badges.Add($"{prop.PropertyName}: {val}");
                    }
                }
                
                var node = _workspaceGraph.CreateNode(data.NodeTitle, new Vector2(100, 100), badges.ToArray());
                node.NodeData = data;
            }
        }

        private void CreateWorkspaceNode(string title, string[] badges)
        {
            if (_workspaceGraph != null)
            {
                var data = ScriptableObject.CreateInstance<StudyGame.Data.WorkspaceNodeData>();
                data.NodeTitle = title;
                data.TemplateType = title;
                
                foreach(var badge in badges)
                {
                    data.Properties.Add(new StudyGame.Data.DynamicProperty { 
                        PropertyName = badge, 
                        ShowAsBadge = true 
                    });
                }
                
                var node = _workspaceGraph.CreateNode(title, new Vector2(100, 100), badges);
                node.NodeData = data;
            }
        }

        private void SaveGraph()
        {
            if (_workspaceGraph == null) return;
            
            var graphData = ScriptableObject.CreateInstance<StudyGame.Data.EpisodeGraphData>();
            
            foreach (var elem in _workspaceGraph.graphElements)
            {
                if (elem is EpisodeNode node)
                {
                    graphData.Nodes.Add(new StudyGame.Data.EpisodeNodeSaveData {
                        NodeGuid = node.NodeId,
                        Position = node.GetPosition().position,
                        NodeData = node.NodeData
                    });
                }
            }

            foreach (var edge in _workspaceGraph.edges.ToList())
            {
                var outputNode = edge.output.node as EpisodeNode;
                var inputNode = edge.input.node as EpisodeNode;
                
                if (outputNode != null && inputNode != null)
                {
                    graphData.NodeLinks.Add(new StudyGame.Data.NodeLinkData {
                        BaseNodeGuid = outputNode.NodeId,
                        PortName = edge.output.portName,
                        TargetNodeGuid = inputNode.NodeId
                    });
                }
            }

            string path = "Assets/Editor/StudyGameDirector/Workspace/SavedGraphs/NewEpisodeGraph.asset";
            UnityEditor.AssetDatabase.CreateAsset(graphData, path);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"그래프 저장 완료! {path}");
        }

        private void LoadGraph()
        {
            string path = UnityEditor.EditorUtility.OpenFilePanel("로드할 에피소드 그래프 선택", "Assets/Resources/Scenarios", "asset");
            if (string.IsNullOrEmpty(path)) return;
            
            path = path.Substring(path.IndexOf("Assets/"));
            var graphData = UnityEditor.AssetDatabase.LoadAssetAtPath<StudyGame.Data.EpisodeGraphData>(path);
            
            if (graphData == null) return;

            // Delete current graph elements
            var elementsToRm = _workspaceGraph.graphElements.ToList();
            foreach (var elem in elementsToRm)
            {
                _workspaceGraph.RemoveElement(elem);
            }

            var nodeDict = new Dictionary<string, EpisodeNode>();

            foreach (var nodeData in graphData.Nodes)
            {
                var badges = new System.Collections.Generic.List<string>();
                foreach (var prop in nodeData.NodeData.Properties)
                {
                    if (prop.ShowAsBadge)
                    {
                        string val = prop.Type == StudyGame.Data.PropertyType.Text ? prop.StringValue : 
                                    (prop.Type == StudyGame.Data.PropertyType.Number ? prop.FloatValue.ToString() : "...");
                        badges.Add($"{prop.PropertyName}: {val}");
                    }
                }

                var node = _workspaceGraph.CreateNode(nodeData.NodeData.NodeTitle, nodeData.Position, badges.ToArray());
                node.NodeId = nodeData.NodeGuid;
                node.NodeData = nodeData.NodeData;
                nodeDict[node.NodeId] = node;
            }

            foreach (var link in graphData.NodeLinks)
            {
                if (nodeDict.TryGetValue(link.BaseNodeGuid, out var baseNode) && 
                    nodeDict.TryGetValue(link.TargetNodeGuid, out var targetNode))
                {
                    var outputPort = baseNode.outputContainer.Q<UnityEditor.Experimental.GraphView.Port>();
                    var inputPort = targetNode.inputContainer.Q<UnityEditor.Experimental.GraphView.Port>();
                    
                    if (outputPort != null && inputPort != null)
                    {
                        var edge = outputPort.ConnectTo(inputPort);
                        _workspaceGraph.AddElement(edge);
                    }
                }
            }
            Debug.Log("그래프 로드 완료!");
        }

        private void AutoGenerateAndConnectEpisodes(string folderPath)
        {
            if (_workspaceGraph == null) return;

            var elementsToRm = _workspaceGraph.graphElements.ToList();
            foreach (var elem in elementsToRm)
            {
                _workspaceGraph.RemoveElement(elem);
            }

            var guids = UnityEditor.AssetDatabase.FindAssets("t:WorkspaceNodeData", new[] { folderPath });
            var nodesData = new List<StudyGame.Data.WorkspaceNodeData>();
            foreach(var guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<StudyGame.Data.WorkspaceNodeData>(path);
                if (asset != null) nodesData.Add(asset);
            }
            
            nodesData.Sort((a, b) => a.name.CompareTo(b.name));

            var createdNodes = new List<EpisodeNode>();
            for (int i = 0; i < nodesData.Count; i++)
            {
                var data = nodesData[i];
                var badges = new List<string>();
                foreach (var prop in data.Properties)
                {
                    if (prop.ShowAsBadge)
                    {
                        string val = prop.Type == StudyGame.Data.PropertyType.Text ? prop.StringValue : 
                                    (prop.Type == StudyGame.Data.PropertyType.Number ? prop.FloatValue.ToString() : "...");
                        badges.Add($"{prop.PropertyName}: {val}");
                    }
                }

                // Spacing 800px on X axis, fixed Y position
                Vector2 pos = new Vector2(i * 800, 300);
                var node = _workspaceGraph.CreateNode(data.NodeTitle, pos, badges.ToArray());
                node.NodeId = string.IsNullOrEmpty(data.NodeId) ? System.Guid.NewGuid().ToString() : data.NodeId;
                node.NodeData = data;
                createdNodes.Add(node);
            }

            for (int i = 0; i < createdNodes.Count - 1; i++)
            {
                var current = createdNodes[i];
                var next = createdNodes[i + 1];

                var outputPort = current.outputContainer.Q<UnityEditor.Experimental.GraphView.Port>();
                var inputPort = next.inputContainer.Q<UnityEditor.Experimental.GraphView.Port>();
                
                if (outputPort != null && inputPort != null)
                {
                    var edge = outputPort.ConnectTo(inputPort);
                    _workspaceGraph.AddElement(edge);
                }
            }
        }

        public void StartSandboxTest(StudyGame.Data.DynamicProperty testPhase = null)
        {
            if (EditorApplication.isPlaying) return;
            
            if (_workspaceGraph != null)
            {
                int nodeCount = _workspaceGraph.graphElements.ToList().OfType<EpisodeNode>().Count();
                if (nodeCount > 0)
                {
                    string path = "Assets/Resources/Scenarios/EpisodeGraph.asset";
                    
                    var graphData = ScriptableObject.CreateInstance<StudyGame.Data.EpisodeGraphData>();
                    foreach (var elem in _workspaceGraph.graphElements)
                    {
                        if (elem is EpisodeNode node)
                        {
                            graphData.Nodes.Add(new StudyGame.Data.EpisodeNodeSaveData {
                                NodeGuid = node.NodeId,
                                Position = node.GetPosition().position,
                                NodeData = node.NodeData
                            });
                        }
                    }
                    
                    StudyGame.Editor.Utils.AssetHelper.CreateOrOverwriteAsset(graphData, path);
                    Debug.Log($"[Sandbox] 그래프 저장 완료! {path}");
                }
                else
                {
                    Debug.LogWarning("[Sandbox] 워크스페이스에 노드가 없어 기존 에피소드 데이터를 유지합니다.");
                }
            }
            
            if (testPhase != null)
            {
                string json = JsonUtility.ToJson(testPhase);
                EditorPrefs.SetString("StudyGame_SandboxTestPhase", json);
            }
            else
            {
                EditorPrefs.DeleteKey("StudyGame_SandboxTestPhase");
            }
            
            if (!UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
                
            string scenePath = "Assets/Scenes/VerificationTestScene.unity";
            if (System.IO.File.Exists(scenePath))
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            }
            else
            {
                var newScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects, UnityEditor.SceneManagement.NewSceneMode.Single);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(newScene, scenePath);
            }
            
            TestSceneSetup.SetupScene();
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            
            string geomJson = JsonUtility.ToJson(_levelGeometry);
            EditorPrefs.SetString("StudyGame_SandboxGeometry", geomJson);
            EditorPrefs.SetBool("StudyGame_SandboxPending", true);
            
            EditorApplication.EnterPlaymode();
        }

        private void BuildDirectorHubUI(VisualElement root)
        {
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
            
            var sandboxBtn = new Button(() => StartSandboxTest()) { text = "▶ 샌드박스 임시 테스트 (기존 씬 훼손 없음)" };
            sandboxBtn.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f));
            sandboxBtn.style.color = Color.white;
            sandboxBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            var headerLeft = new VisualElement();
            headerLeft.style.flexDirection = FlexDirection.Row;
            headerLeft.Add(paletteTitle);
            headerLeft.Add(sandboxBtn);
            
            titleRow.Add(headerLeft);
            titleRow.Add(actionContainer);
            paletteHeader.Add(titleRow);

            var brushContainer = new VisualElement();
            brushContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.3f, 0.1f, 0.8f));
            brushContainer.style.paddingLeft = 5; brushContainer.style.paddingRight = 5;
            brushContainer.style.paddingTop = 5; brushContainer.style.paddingBottom = 5;
            brushContainer.style.marginTop = 10;
            brushContainer.style.marginBottom = 10;
            
            var toolField = new UnityEngine.UIElements.EnumField("Active Tool", _currentTool);
            toolField.RegisterValueChangedCallback(evt => {
                _currentTool = (FloorplanTool)evt.newValue;
                _lastPaintedCell = null;
            });
            brushContainer.Add(toolField);

            var autoFillBtn = new Button(ExecuteConvexHullFill) { text = "🕳️ Fill Outer Bounds (Convex Hull)" };
            autoFillBtn.style.marginTop = 5;
            autoFillBtn.style.marginBottom = 5;
            brushContainer.Add(autoFillBtn);
            
            var brushSizeField = new IntegerField("Brush Size (Cells)") { value = _brushSize };
            brushSizeField.RegisterValueChangedCallback(evt => _brushSize = Mathf.Max(1, evt.newValue));
            brushContainer.Add(brushSizeField);
            
            var gridField = new FloatField("Grid Size") { value = _levelGeometry.GridSize };
            gridField.RegisterValueChangedCallback(evt => _levelGeometry.GridSize = evt.newValue);
            brushContainer.Add(gridField);
            
            var wallToggle = new Toggle("Auto Walls") { value = _levelGeometry.GenerateWalls };
            wallToggle.RegisterValueChangedCallback(evt => { _levelGeometry.GenerateWalls = evt.newValue; LevelGeometryManager.Rebuild(_levelGeometry); });
            brushContainer.Add(wallToggle);

            var ceilToggle = new Toggle("Auto Ceiling") { value = _levelGeometry.GenerateCeiling };
            ceilToggle.RegisterValueChangedCallback(evt => {
                _levelGeometry.GenerateCeiling = evt.newValue;
                LevelGeometryManager.Rebuild(_levelGeometry);
            });
            brushContainer.Add(ceilToggle);

            var opacitySlider = new Slider("Ceiling Opacity", 0f, 1f) { value = _levelGeometry.CeilingOpacity };
            opacitySlider.RegisterValueChangedCallback(evt => {
                _levelGeometry.CeilingOpacity = evt.newValue;
                LevelGeometryManager.Rebuild(_levelGeometry);
            });
            brushContainer.Add(opacitySlider);
            
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
        }

        private void CreateNode(string title, DirectorNodeType type = DirectorNodeType.Event)
        {
            WindowNode node;
            if (type == DirectorNodeType.Database)
                node = new DatabaseNodeView(title);
            else
                node = new WindowNode(title, type);
                
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

        private bool _needsRebuild = false;

        private void HandleInteraction(Rect rect, Camera cam, Event e)
        {
            if (!rect.Contains(e.mousePosition) && _draggingNodeId == null) return;

            // Tool Logic
            if (_currentTool != FloorplanTool.None && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag || e.type == EventType.MouseUp) && e.button == 0)
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
                    Vector2Int currentCell = new Vector2Int(cx, cz);
                    
                    bool changed = false;

                    if (_currentTool == FloorplanTool.Brush)
                    {
                        if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
                        {
                            List<Vector2Int> cellsToPaint = new List<Vector2Int>();
                            if (e.type == EventType.MouseDown || _lastPaintedCell == null)
                            {
                                cellsToPaint.Add(currentCell);
                            }
                            else
                            {
                                cellsToPaint = GetCellsOnLine(_lastPaintedCell.Value, currentCell);
                            }
                            _lastPaintedCell = currentCell;

                            int radius = _brushSize - 1;
                            foreach (var c in cellsToPaint)
                            {
                                for (int xOffset = -radius; xOffset <= radius; xOffset++)
                                {
                                    for (int zOffset = -radius; zOffset <= radius; zOffset++)
                                    {
                                        Vector2Int cell = new Vector2Int(c.x + xOffset, c.y + zOffset);
                                        if (e.shift) { if (_levelGeometry.FloorCells.Contains(cell)) { _levelGeometry.FloorCells.Remove(cell); changed = true; } }
                                        else { if (!_levelGeometry.FloorCells.Contains(cell)) { _levelGeometry.FloorCells.Add(cell); changed = true; } }
                                    }
                                }
                            }
                            if (changed) _needsRebuild = true;
                        }
                        else if (e.type == EventType.MouseUp)
                        {
                            _lastPaintedCell = null;
                        }
                    }
                    else if (_currentTool == FloorplanTool.Bucket)
                    {
                        if (e.type == EventType.MouseDown)
                        {
                            ExecuteBucketFill(currentCell, !e.shift);
                            _needsRebuild = true;
                        }
                    }
                    else if (_currentTool == FloorplanTool.Rectangle)
                    {
                        if (e.type == EventType.MouseDown)
                        {
                            _rectStartCell = currentCell;
                            _rectEndCell = currentCell;
                        }
                        else if (e.type == EventType.MouseDrag && _rectStartCell.HasValue)
                        {
                            _rectEndCell = currentCell;
                        }
                        else if (e.type == EventType.MouseUp && _rectStartCell.HasValue)
                        {
                            _rectEndCell = currentCell;
                            
                            int minX = Mathf.Min(_rectStartCell.Value.x, _rectEndCell.Value.x);
                            int maxX = Mathf.Max(_rectStartCell.Value.x, _rectEndCell.Value.x);
                            int minZ = Mathf.Min(_rectStartCell.Value.y, _rectEndCell.Value.y);
                            int maxZ = Mathf.Max(_rectStartCell.Value.y, _rectEndCell.Value.y);

                            for (int x = minX; x <= maxX; x++)
                            {
                                for (int z = minZ; z <= maxZ; z++)
                                {
                                    Vector2Int cell = new Vector2Int(x, z);
                                    if (e.shift) { _levelGeometry.FloorCells.Remove(cell); changed = true; }
                                    else if (!_levelGeometry.FloorCells.Contains(cell)) { _levelGeometry.FloorCells.Add(cell); changed = true; }
                                }
                            }
                            if (changed) _needsRebuild = true;
                            _rectStartCell = null;
                            _rectEndCell = null;
                        }
                        UpdateRectanglePreview();
                    }
                }
                
                if (e.type == EventType.MouseUp)
                {
                    if (_needsRebuild)
                    {
                        LevelGeometryManager.Rebuild(_levelGeometry);
                        _needsRebuild = false;
                    }
                }
                
                if (e.type != EventType.MouseMove)
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
                    _floorplanCameraSize = Mathf.Clamp(_floorplanCameraSize, 0.5f, 1000f);
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
                    _masterCameraDistance = Mathf.Clamp(_masterCameraDistance, 0.5f, 1000f);
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

        private void ExecuteBucketFill(Vector2Int startCell, bool fill)
        {
            bool targetState = fill;
            if (_levelGeometry.FloorCells.Contains(startCell) == targetState) return;

            Queue<Vector2Int> q = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            q.Enqueue(startCell);
            visited.Add(startCell);

            int maxCells = 10000;
            int filled = 0;

            List<Vector2Int> toAdd = new List<Vector2Int>();
            List<Vector2Int> toRemove = new List<Vector2Int>();

            while(q.Count > 0 && filled < maxCells)
            {
                Vector2Int curr = q.Dequeue();
                if (targetState) toAdd.Add(curr); else toRemove.Add(curr);
                filled++;

                Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                foreach(var d in dirs)
                {
                    Vector2Int n = curr + d;
                    if (!visited.Contains(n) && _levelGeometry.FloorCells.Contains(n) != targetState)
                    {
                        visited.Add(n);
                        q.Enqueue(n);
                    }
                }
            }

            if (targetState && filled >= maxCells)
            {
                Debug.LogWarning("Bucket fill reached maximum limit! Aborting to prevent infinite fill.");
                return; 
            }

            if (targetState)
            {
                foreach(var c in toAdd) _levelGeometry.FloorCells.Add(c);
            }
            else
            {
                foreach(var c in toRemove) _levelGeometry.FloorCells.Remove(c);
            }
        }

        private void ExecuteConvexHullFill()
        {
            if (_levelGeometry.FloorCells.Count < 3) return;

            List<Vector2Int> points = new List<Vector2Int>(_levelGeometry.FloorCells);
            points.Sort((a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));

            List<Vector2Int> lower = new List<Vector2Int>();
            foreach (var p in points)
            {
                while (lower.Count >= 2 && CrossProduct(lower[lower.Count - 2], lower[lower.Count - 1], p) <= 0)
                    lower.RemoveAt(lower.Count - 1);
                lower.Add(p);
            }

            List<Vector2Int> upper = new List<Vector2Int>();
            for (int i = points.Count - 1; i >= 0; i--)
            {
                var p = points[i];
                while (upper.Count >= 2 && CrossProduct(upper[upper.Count - 2], upper[upper.Count - 1], p) <= 0)
                    upper.RemoveAt(upper.Count - 1);
                upper.Add(p);
            }

            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);
            List<Vector2Int> hull = new List<Vector2Int>(lower);
            hull.AddRange(upper);

            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;
            foreach (var p in hull)
            {
                if (p.x < minX) minX = p.x; if (p.x > maxX) maxX = p.x;
                if (p.y < minY) minY = p.y; if (p.y > maxY) maxY = p.y;
            }

            bool changed = false;
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    if (!_levelGeometry.FloorCells.Contains(cell) && IsPointInPolygon(cell, hull))
                    {
                        _levelGeometry.FloorCells.Add(cell);
                        changed = true;
                    }
                }
            }
            if (changed) LevelGeometryManager.Rebuild(_levelGeometry);
        }

        private int CrossProduct(Vector2Int o, Vector2Int a, Vector2Int b)
        {
            return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
        }

        private bool IsPointInPolygon(Vector2Int p, List<Vector2Int> polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if ((polygon[i].y > p.y) != (polygon[j].y > p.y) &&
                    p.x < (polygon[j].x - polygon[i].x) * (float)(p.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private List<Vector2Int> GetCellsOnLine(Vector2Int p0, Vector2Int p1)
        {
            List<Vector2Int> line = new List<Vector2Int>();
            int dx = Mathf.Abs(p1.x - p0.x);
            int dy = Mathf.Abs(p1.y - p0.y);
            int sx = p0.x < p1.x ? 1 : -1;
            int sy = p0.y < p1.y ? 1 : -1;
            int err = dx - dy;

            int x = p0.x;
            int y = p0.y;

            while (true)
            {
                line.Add(new Vector2Int(x, y));
                if (x == p1.x && y == p1.y) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x += sx; }
                if (e2 < dx) { err += dx; y += sy; }
            }
            return line;
        }

        private void UpdateRectanglePreview()
        {
            if (_currentTool != FloorplanTool.Rectangle || !_rectStartCell.HasValue || !_rectEndCell.HasValue)
            {
                if (_rectPreviewObject != null) _rectPreviewObject.SetActive(false);
                return;
            }
            
            if (_rectPreviewObject == null)
            {
                _rectPreviewObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                _rectPreviewObject.name = "RectPreview";
                _rectPreviewObject.transform.rotation = Quaternion.Euler(90, 0, 0);
                DestroyImmediate(_rectPreviewObject.GetComponent<Collider>());
                
                var renderer = _rectPreviewObject.GetComponent<MeshRenderer>();
                Material previewMat = new Material(Shader.Find("Transparent/Diffuse"));
                previewMat.color = new Color(0, 0, 1, 0.4f);
                renderer.sharedMaterial = previewMat;
            }
            
            _rectPreviewObject.SetActive(true);
            
            int minX = Mathf.Min(_rectStartCell.Value.x, _rectEndCell.Value.x);
            int maxX = Mathf.Max(_rectStartCell.Value.x, _rectEndCell.Value.x);
            int minZ = Mathf.Min(_rectStartCell.Value.y, _rectEndCell.Value.y);
            int maxZ = Mathf.Max(_rectStartCell.Value.y, _rectEndCell.Value.y);
            
            float s = _levelGeometry.GridSize;
            float width = (maxX - minX + 1) * s;
            float height = (maxZ - minZ + 1) * s;
            
            _rectPreviewObject.transform.localScale = new Vector3(width, height, 1);
            _rectPreviewObject.transform.position = new Vector3(
                minX * s + width / 2f, 
                0.1f, 
                minZ * s + height / 2f);
        }

        private void BuildAIConsoleUI(VisualElement root)
        {
            root.style.paddingTop = root.style.paddingBottom = root.style.paddingLeft = root.style.paddingRight = 20;

            var title = new Label("🧠 AI 에디터 확장 콘솔 (Meta-Editor Prompts)");
            title.style.fontSize = 20;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 20;
            root.Add(title);

            var desc = new Label("여기에 에디터 개조(C# 스크립트 수정, UI 신설 등)를 위한 프롬프트를 표 형태로 작성하세요.\n작성 후 [▶ MCP에 전송] 버튼을 누르면 AI가 이를 읽고 즉시 에디터를 업데이트합니다.");
            desc.style.marginBottom = 20;
            root.Add(desc);

            var tableContainer = new ScrollView();
            tableContainer.style.flexGrow = 1;
            tableContainer.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
            tableContainer.style.paddingTop = tableContainer.style.paddingBottom = tableContainer.style.paddingLeft = tableContainer.style.paddingRight = 10;
            root.Add(tableContainer);

            // Mockup of a Table
            var headerRow = new VisualElement() { style = { flexDirection = FlexDirection.Row, marginBottom = 10 } };
            headerRow.Add(new Label("개조 대상 (Target)") { style = { flexGrow = 1, unityFontStyleAndWeight = FontStyle.Bold } });
            headerRow.Add(new Label("요청 내용 (Prompt)") { style = { flexGrow = 3, unityFontStyleAndWeight = FontStyle.Bold } });
            headerRow.Add(new Label("우선순위 (Priority)") { style = { flexGrow = 1, unityFontStyleAndWeight = FontStyle.Bold } });
            tableContainer.Add(headerRow);

            var prompts = new System.Collections.Generic.List<VisualElement>();

            System.Action addPromptRow = () => {
                var dataRow = new VisualElement() { style = { flexDirection = FlexDirection.Row, marginBottom = 5 } };
                var targetInput = new TextField() { value = "DirectorEditorWindow.cs", style = { flexGrow = 1 } };
                var descInput = new TextField() { value = "예: 툴바 우측에 [로그 초기화] 추가", style = { flexGrow = 3 } };
                var prioInput = new TextField() { value = "High", style = { flexGrow = 1 } };
                dataRow.Add(targetInput);
                dataRow.Add(descInput);
                dataRow.Add(prioInput);
                prompts.Add(dataRow);
                tableContainer.Add(dataRow);
            };

            addPromptRow(); // default row
            
            // Check for Pending AI Prompt from State Manager
            root.RegisterCallback<GeometryChangedEvent>(evt => {
                if (!string.IsNullOrEmpty(DirectorStateManager.PendingAIPrompt))
                {
                    if (prompts.Count > 0)
                    {
                        var targetField = prompts[0][0] as TextField;
                        var descField = prompts[0][1] as TextField;
                        targetField.value = "Scenario Node: " + (DirectorStateManager.ActivePhaseProperty != null ? DirectorStateManager.ActivePhaseProperty.PropertyName : "Dialogue");
                        descField.value = DirectorStateManager.PendingAIPrompt;
                        DirectorStateManager.ClearPendingAIPrompt();
                    }
                }
            });

            var btnRow = new VisualElement() { style = { flexDirection = FlexDirection.Row, marginTop = 10 } };
            var addBtn = new Button(addPromptRow) { text = "➕ 프롬프트 행 추가" };
            addBtn.style.flexGrow = 1;
            btnRow.Add(addBtn);
            tableContainer.Add(btnRow);

            var sendBtn = new Button(() => { 
                var exportList = new System.Collections.Generic.List<string>();
                foreach(var row in prompts)
                {
                    if (row.childCount >= 3)
                    {
                        var t = (row[0] as TextField).value;
                        var d = (row[1] as TextField).value;
                        var p = (row[2] as TextField).value;
                        exportList.Add($"Target: {t} | Prompt: {d} | Priority: {p}");
                    }
                }
                System.Text.StringBuilder promptBuilder = new System.Text.StringBuilder();
                promptBuilder.AppendLine("<에디터 개조 요청>");
                foreach (var item in exportList)
                {
                    promptBuilder.AppendLine(item);
                }
                
                string finalPrompt = promptBuilder.ToString();
                
                // 클립보드에 복사
                EditorGUIUtility.systemCopyBuffer = finalPrompt;
                
                // 유저에게 안내 팝업
                EditorUtility.DisplayDialog("MCP 개조 요청", 
                    "요청 프롬프트가 클립보드에 복사되었습니다!\n\nGemini(Antigravity) 채팅창에 붙여넣기(Ctrl+V) 한 후 전송하시면, AI가 즉시 개조를 시작합니다.", 
                    "확인");
                    
                Debug.Log("MCP에 전송할 프롬프트가 클립보드에 복사되었습니다."); 
            }) { text = "▶ MCP(AI)에게 에디터 개조 요청하기" };
            sendBtn.style.marginTop = 20;
            sendBtn.style.height = 40;
            sendBtn.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f));
            root.Add(sendBtn);
        }

        private void OnDestroy()
        {
            EditorApplication.update -= RepaintMonitors;
            if (_proxyCamera != null) DestroyImmediate(_proxyCamera.gameObject);
            if (_floorplanCamera != null) DestroyImmediate(_floorplanCamera.gameObject);
        }
    }

    [InitializeOnLoad]
    public static class SandboxTestRunner
    {
        static SandboxTestRunner()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (EditorPrefs.GetBool("StudyGame_SandboxPending", false))
                {
                    EditorPrefs.SetBool("StudyGame_SandboxPending", false);
                    string json = EditorPrefs.GetString("StudyGame_SandboxGeometry", "");
                    if (!string.IsNullOrEmpty(json))
                    {
                        var data = JsonUtility.FromJson<LevelGeometryData>(json);
                        LevelGeometryManager.Rebuild(data);
                    }

                    var player = Object.FindFirstObjectByType<StudyGame.Player.PlayerController>(FindObjectsInactive.Include);
                    if (player != null)
                    {
                        player.SetMovementEnabled(true);
                        Debug.Log("[SandboxTestRunner] Player movement enabled for sandbox testing.");
                    }

                    var simulatorGo = new GameObject("GameViewInputSimulator");
                    var simulator = simulatorGo.AddComponent<StudyGame.Testing.GameViewInputSimulator>();
                    simulator.RunTestSequence();
                }
            }
        }
    }
}
