using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System.Collections.Generic;
using StudyGame.Data;

namespace StudyGame.Editor.Director
{
    public class WorkspaceNodeWindow : EditorWindow
    {
        private EpisodeGraphView _workspaceGraph;

        public static void Open(string episodeGraphPath = null)
        {
            var wnd = GetWindow<WorkspaceNodeWindow>();
            wnd.titleContent = new GUIContent("🕸️ Episode Node Editor");
            wnd.minSize = new Vector2(800, 600);
            if (!string.IsNullOrEmpty(episodeGraphPath))
            {
                wnd.LoadGraphFromPath(episodeGraphPath);
            }
            wnd.Show();
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.focusable = true;
            root.RegisterCallback<KeyDownEvent>(OnKeyDown);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/StudyGameDirector/Workspace/WorkspaceStyle.uss");
            if (styleSheet != null) root.styleSheets.Add(styleSheet);

            var toolbar = new UnityEditor.UIElements.Toolbar();
            toolbar.Add(new UnityEditor.UIElements.ToolbarButton(SaveGraph) { text = "💾 노드 저장" });
            toolbar.Add(new UnityEditor.UIElements.ToolbarButton(LoadGraph) { text = "📂 노드 로드" });

            var toolsMenu = new UnityEditor.UIElements.ToolbarMenu { text = "🪄 시나리오 자동 연결" };
            toolsMenu.menu.AppendAction("생성 및 전체 연결 (모든 에피소드)", a => AutoGenerateAndConnectEpisodes("Assets/Resources/Scenarios/Episodes"));
            toolbar.Add(toolsMenu);

            var sandboxBtn = new UnityEditor.UIElements.ToolbarButton(() => StartSandboxTest()) { text = "▶ 샌드박스 테스트" };
            toolbar.Add(sandboxBtn);

            root.Add(toolbar);

            var mainSplit = new TwoPaneSplitView(0, 260, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(mainSplit);

            InspectorView inspector = null;

            var explorer = new ScrollView();
            explorer.AddToClassList("workspace-panel");

            var episodeGuids = AssetDatabase.FindAssets("t:WorkspaceNodeData", new[] { "Assets/Resources/Scenarios/Episodes", "Assets/Resources/Scenarios/Anomalies" });

            if (episodeGuids.Length == 0)
            {
                explorer.Add(new Label("에피소드 에셋이 없습니다. (상단 마술봉 버튼으로 생성하세요)"));
            }
            else
            {
                var episodes = new List<WorkspaceNodeData>();
                foreach (var guid in episodeGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<WorkspaceNodeData>(path);
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

            var drawerTitle = new Label("▼ 템플릿 서랍 (Templates)");
            drawerTitle.AddToClassList("workspace-section-header");
            explorer.Add(drawerTitle);

            var templateGuids = AssetDatabase.FindAssets("t:WorkspaceNodeData", new[] { "Assets/Editor/StudyGameDirector/Workspace/Templates" });
            foreach (var guid in templateGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var tpl = AssetDatabase.LoadAssetAtPath<WorkspaceNodeData>(path);
                if (tpl != null)
                {
                    explorer.Add(new Button(() => CreateWorkspaceNodeFromTemplate(tpl)) { text = $"{(string.IsNullOrEmpty(tpl.ThemeIcon) ? "📦" : tpl.ThemeIcon)} {tpl.NodeTitle}" });
                }
            }

            if (templateGuids.Length == 0)
            {
                explorer.Add(new Button(() => CreateWorkspaceNode("이벤트 시작: OnInteract", new string[] { "트리거 🚩", "ID: 빈칸" })) { text = "🚩 필드 상호작용 이벤트" });
                explorer.Add(new Button(() => CreateWorkspaceNode("대화 재생 (Dialogue)", new string[] { "대화 🗣️", "화자: 미지정" })) { text = "🗣️ 대화 재생 액션" });
                explorer.Add(new Button(() => CreateWorkspaceNode("캐릭터 DNA", new string[] { "아트 🎨" })) { text = "👗 캐릭터 DNA 튜너" });
                explorer.Add(new Button(() => CreateWorkspaceNode("보스 퍼즐", new string[] { "퍼즐 🧩" })) { text = "🧩 질문 조립기(보스전)" });
            }

            mainSplit.Add(explorer);

            var rightSplit = new TwoPaneSplitView(0, 500, TwoPaneSplitViewOrientation.Horizontal);
            mainSplit.Add(rightSplit);

            var canvasContainer = new VisualElement();
            canvasContainer.style.flexGrow = 1;
            var epGraph = new EpisodeGraphView();
            epGraph.style.flexGrow = 1;
            canvasContainer.Add(epGraph);
            rightSplit.Add(canvasContainer);

            inspector = new InspectorView();
            rightSplit.Add(inspector);

            epGraph.RegisterCallback<MouseUpEvent>(evt => {
                var selection = epGraph.selection;
                if (selection.Count > 0 && selection[0] is EpisodeNode node)
                {
                    inspector.BindNode(node);
                    DirectorStateManager.SetActiveContext(node, null);
                }
            });

            _workspaceGraph = epGraph;
        }

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

        private void SelectOrSpawnNode(WorkspaceNodeData ep, InspectorView inspector)
        {
            if (_workspaceGraph == null) return;
            var node = GetEpisodeNodeById(ep.NodeId);
            if (node == null)
            {
                var badges = new List<string>();
                foreach (var prop in ep.Properties)
                {
                    if (prop.ShowAsBadge)
                    {
                        string val = prop.Type == PropertyType.Text ? prop.StringValue :
                                    (prop.Type == PropertyType.Number ? prop.FloatValue.ToString() : "...");
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

        private void CreateWorkspaceNodeFromTemplate(WorkspaceNodeData template)
        {
            if (_workspaceGraph == null) return;

            var data = ScriptableObject.CreateInstance<WorkspaceNodeData>();
            data.NodeTitle = template.NodeTitle;
            data.TemplateType = template.TemplateType;
            data.ThemeColorHex = template.ThemeColorHex;
            data.ThemeIcon = template.ThemeIcon;

            var badges = new List<string>();
            foreach (var prop in template.Properties)
            {
                data.Properties.Add(new DynamicProperty
                {
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
                    string val = prop.Type == PropertyType.Text ? prop.StringValue :
                                (prop.Type == PropertyType.Number ? prop.FloatValue.ToString() : "...");
                    badges.Add($"{prop.PropertyName}: {val}");
                }
            }

            var node = _workspaceGraph.CreateNode(data.NodeTitle, new Vector2(100, 100), badges.ToArray());
            node.NodeData = data;
        }

        private void CreateWorkspaceNode(string title, string[] badges)
        {
            if (_workspaceGraph == null) return;

            var data = ScriptableObject.CreateInstance<WorkspaceNodeData>();
            data.NodeTitle = title;
            data.TemplateType = title;

            foreach (var badge in badges)
            {
                data.Properties.Add(new DynamicProperty
                {
                    PropertyName = badge,
                    ShowAsBadge = true
                });
            }

            var node = _workspaceGraph.CreateNode(title, new Vector2(100, 100), badges);
            node.NodeData = data;
        }

        private void SaveGraph()
        {
            if (_workspaceGraph == null) return;

            var graphData = ScriptableObject.CreateInstance<EpisodeGraphData>();

            foreach (var elem in _workspaceGraph.graphElements)
            {
                if (elem is EpisodeNode node)
                {
                    graphData.Nodes.Add(new EpisodeNodeSaveData
                    {
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
                    graphData.NodeLinks.Add(new NodeLinkData
                    {
                        BaseNodeGuid = outputNode.NodeId,
                        PortName = edge.output.portName,
                        TargetNodeGuid = inputNode.NodeId
                    });
                }
            }

            string path = "Assets/Editor/StudyGameDirector/Workspace/SavedGraphs/NewEpisodeGraph.asset";
            AssetDatabase.CreateAsset(graphData, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"그래프 저장 완료! {path}");
        }

        private void LoadGraph()
        {
            string path = EditorUtility.OpenFilePanel("로드할 에피소드 그래프 선택", "Assets/Resources/Scenarios", "asset");
            if (string.IsNullOrEmpty(path)) return;

            path = path.Substring(path.IndexOf("Assets/"));
            LoadGraphFromPath(path);
        }

        public void LoadGraphFromPath(string path)
        {
            var graphData = AssetDatabase.LoadAssetAtPath<EpisodeGraphData>(path);
            if (graphData == null) return;
            if (_workspaceGraph == null) return;

            var elementsToRm = _workspaceGraph.graphElements.ToList();
            foreach (var elem in elementsToRm)
                _workspaceGraph.RemoveElement(elem);

            var nodeDict = new Dictionary<string, EpisodeNode>();

            foreach (var nodeData in graphData.Nodes)
            {
                var badges = new List<string>();
                foreach (var prop in nodeData.NodeData.Properties)
                {
                    if (prop.ShowAsBadge)
                    {
                        string val = prop.Type == PropertyType.Text ? prop.StringValue :
                                    (prop.Type == PropertyType.Number ? prop.FloatValue.ToString() : "...");
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
                    var outputPort = baseNode.outputContainer.Q<Port>();
                    var inputPort = targetNode.inputContainer.Q<Port>();

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
                _workspaceGraph.RemoveElement(elem);

            var guids = AssetDatabase.FindAssets("t:WorkspaceNodeData", new[] { folderPath });
            var nodesData = new List<WorkspaceNodeData>();
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<WorkspaceNodeData>(path);
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
                        string val = prop.Type == PropertyType.Text ? prop.StringValue :
                                    (prop.Type == PropertyType.Number ? prop.FloatValue.ToString() : "...");
                        badges.Add($"{prop.PropertyName}: {val}");
                    }
                }

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

                var outputPort = current.outputContainer.Q<Port>();
                var inputPort = next.inputContainer.Q<Port>();

                if (outputPort != null && inputPort != null)
                {
                    var edge = outputPort.ConnectTo(inputPort);
                    _workspaceGraph.AddElement(edge);
                }
            }
        }

        public void StartSandboxTest(DynamicProperty testPhase = null)
        {
            if (EditorApplication.isPlaying) return;

            if (_workspaceGraph != null)
            {
                int nodeCount = _workspaceGraph.graphElements.ToList().OfType<EpisodeNode>().Count();
                if (nodeCount > 0)
                {
                    string path = "Assets/Resources/Scenarios/EpisodeGraph.asset";

                    var graphData = ScriptableObject.CreateInstance<EpisodeGraphData>();
                    foreach (var elem in _workspaceGraph.graphElements)
                    {
                        if (elem is EpisodeNode node)
                        {
                            graphData.Nodes.Add(new EpisodeNodeSaveData
                            {
                                NodeGuid = node.NodeId,
                                Position = node.GetPosition().position,
                                NodeData = node.NodeData
                            });
                        }
                    }

                    StudyGame.Editor.Utils.AssetHelper.CreateOrOverwriteAsset(graphData, path);
                    Debug.Log($"[Sandbox] 그래프 저장 완료! {path}");
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
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            else
            {
                var newScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects, UnityEditor.SceneManagement.NewSceneMode.Single);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(newScene, scenePath);
            }

            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            EditorPrefs.SetBool("StudyGame_SandboxPending", true);
            EditorApplication.EnterPlaymode();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.ctrlKey && evt.keyCode == KeyCode.S)
            {
                SaveGraph();
                evt.StopPropagation();
            }
        }
    }
}
