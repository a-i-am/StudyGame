using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace StudyGame.Editor
{
    public class StudyGameDirectorHub : EditorWindow
    {
        private int selectedTab = 0;
        private string[] tabs = { "🕸️ Episode Nodes", "📄 Data Importer", "⚔️ Combat & Camera", "▶️ Test Runner", "🎬 Sequencer" };
        // --- Sequencer Variables ---
        private float previewHeight = 170f;
        private bool isResizingPreview = false;
        private Vector2 seqScrollPos;
        private float seqTimelineWidth = 2000f;
        private float seqPlayheadTime = 0f;
        private string[] seqTracks = { "Dialogue", "Actor", "Gimmick" };
        private List<TimelineClip> seqClips = new List<TimelineClip>();
        private PreviewRenderUtility previewUtility;
        private GameObject previewCharacter;

        private class TimelineClip
        {
            public int trackIndex;
            public float startTime;
            public float duration;
            public string content;
            public Color color;
        }
        // ---------------------------

        [MenuItem("StudyGame/Director Hub (Master)")]
        public static void ShowWindow()
        {
            var window = GetWindow<StudyGameDirectorHub>("Director Hub");
            window.minSize = new Vector2(1000, 600);
            window.Show();
        }

        private void OnEnable()
        {
            // Init Sequencer Preview
            if (previewUtility == null)
            {
                previewUtility = new PreviewRenderUtility();
                previewUtility.camera.transform.position = new Vector3(0, 1, -5);
                previewUtility.camera.transform.rotation = Quaternion.identity;

                previewCharacter = GameObject.CreatePrimitive(PrimitiveType.Cube);
                previewCharacter.name = "Preview_Doyoung";
                previewCharacter.transform.position = Vector3.zero;
                previewUtility.AddSingleGO(previewCharacter);
            }
        }

        private void OnDisable()
        {
            if (previewUtility != null)
            {
                previewUtility.Cleanup();
                previewUtility = null;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            // Left Sidebar Navigation
            DrawSidebar();

            // Right Content Area
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            switch (selectedTab)
            {
                case 0: DrawFlowGraphTab(); break;     // 노드 에디터
                case 1: DrawImporterTab(); break;      // JSON 파이프라인
                case 2: DrawCombatManagerTab(); break; // 전투/카메라 셋업
                case 3: DrawTestRunnerTab(); break;    // 인게임 테스트 구동
                case 4: DrawSequencerTab(); break;     // 컷씬 (레거시)
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(200), GUILayout.ExpandHeight(true));
            GUILayout.Label("STUDYGAME", EditorStyles.whiteLargeLabel);
            GUILayout.Label("Director Hub", EditorStyles.boldLabel);
            GUILayout.Space(20);

            GUIStyle tabStyle = new GUIStyle(EditorStyles.toolbarButton);
            tabStyle.fixedHeight = 40;
            tabStyle.fontSize = 14;
            tabStyle.alignment = TextAnchor.MiddleLeft;

            for (int i = 0; i < tabs.Length; i++)
            {
                Color defaultColor = GUI.backgroundColor;
                if (selectedTab == i) GUI.backgroundColor = new Color(0.3f, 0.7f, 1f); // Highlight selected

                if (GUILayout.Button(" " + tabs[i], tabStyle))
                {
                    selectedTab = i;
                }

                GUI.backgroundColor = defaultColor;
                GUILayout.Space(5);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawFlowGraphTab()
        {
            GUILayout.Label("🕸️ Episode & Event Flow Graph", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("시나리오 노드를 배치하고 '과목 락(Lock)' 등 시스템 속성을 주입하는 메인 윈도우를 호출합니다.", MessageType.Info);

            GUILayout.Space(10);
            if (GUILayout.Button("Open Episode Node Editor", GUILayout.Height(40)))
            {
                var nodeWindow = EditorWindow.GetWindow<StudyGame.Editor.Director.DirectorEditorWindow>("Episode Editor");
                nodeWindow.Show();
            }
        }


        private void DrawSequencerTab()
        {
            // Toolbar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Load Test Yarn", EditorStyles.toolbarButton, GUILayout.Width(120)))
            {
                LoadYarnScript("Assets/Resources/Scenarios/YarnScripts/EP1/Node_Boss_Doyoung.yarn");
            }
            if (GUILayout.Button("Play", EditorStyles.toolbarButton, GUILayout.Width(50))) { }
            if (GUILayout.Button("Stop", EditorStyles.toolbarButton, GUILayout.Width(50))) { }
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Time: {seqPlayheadTime:F2}s");
            EditorGUILayout.EndHorizontal();

            // Preview Area
            DrawSequencerPreviewArea();

            // Drag Splitter (프리뷰와 트랙 사이 상하 크기 조절바)
            DrawPreviewSplitter();

            // Tracks Area
            EditorGUILayout.BeginHorizontal();

            // Headers
            EditorGUILayout.BeginVertical(GUILayout.Width(100));
            GUILayout.Space(20);
            foreach (var track in seqTracks)
            {
                Rect rect = GUILayoutUtility.GetRect(100, 40);
                EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
                GUI.Label(rect, " " + track, EditorStyles.boldLabel);
            }
            EditorGUILayout.EndVertical();

            // Timeline
            seqScrollPos = EditorGUILayout.BeginScrollView(seqScrollPos);
            Rect timelineRect = EditorGUILayout.BeginVertical(GUILayout.Width(seqTimelineWidth));

            // Ruler
            Rect rulerRect = GUILayoutUtility.GetRect(seqTimelineWidth, 20);
            EditorGUI.DrawRect(rulerRect, new Color(0.15f, 0.15f, 0.15f));
            for (int i = 0; i < seqTimelineWidth; i += 100)
            {
                GUI.Label(new Rect(rulerRect.x + i, rulerRect.y, 50, 20), (i / 100f).ToString() + "s");
                EditorGUI.DrawRect(new Rect(rulerRect.x + i, rulerRect.y + 15, 1, 5), Color.gray);
            }

            // Background
            for (int i = 0; i < seqTracks.Length; i++)
            {
                Rect trackRect = GUILayoutUtility.GetRect(seqTimelineWidth, 40);
                EditorGUI.DrawRect(trackRect, i % 2 == 0 ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.3f, 0.3f, 0.3f));
            }

            // Clips
            GUIStyle clipStyle = new GUIStyle(GUI.skin.box);
            clipStyle.normal.textColor = Color.white;
            clipStyle.alignment = TextAnchor.MiddleLeft;
            clipStyle.clipping = TextClipping.Clip;

            foreach (var clip in seqClips)
            {
                float x = timelineRect.x + (clip.startTime * 100f);
                float y = timelineRect.y + 20f + (clip.trackIndex * 40f) + 2f;
                float width = clip.duration * 100f;
                float height = 36f;

                Rect clipRect = new Rect(x, y, width, height);
                EditorGUI.DrawRect(clipRect, clip.color);
                GUI.Label(clipRect, " " + clip.content, clipStyle);
            }

            // Scrubbing
            Event e = Event.current;
            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            {
                if (timelineRect.Contains(e.mousePosition))
                {
                    seqPlayheadTime = Mathf.Max(0, (e.mousePosition.x - timelineRect.x) / 100f);
                    Repaint();
                }
            }

            // Playhead
            float playheadX = timelineRect.x + (seqPlayheadTime * 100f);
            Rect playheadRect = new Rect(playheadX, timelineRect.y, 2, timelineRect.height);
            EditorGUI.DrawRect(playheadRect, Color.red);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawImporterTab()
        {
            GUILayout.Label("📄 Anomaly Data Importer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("수능 기믹 JSON 파일을 파싱하여 시스템 속성(NodeType, Lock)이 포함된 에셋으로 굽습니다.", MessageType.Info);

            GUILayout.Space(10);
            if (GUILayout.Button("Open JSON Importer Window", GUILayout.Height(40)))
            {
                StudyGame.Editor.Director.AnomalyImporterWindow.ShowWindow();
            }
        }

        private void DrawPreviewSplitter()
        {
            Rect splitterRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(5f), GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(splitterRect, new Color(0.1f, 0.1f, 0.1f));
            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeVertical);

            Event e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (splitterRect.Contains(e.mousePosition))
                    {
                        isResizingPreview = true;
                        e.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (isResizingPreview)
                    {
                        previewHeight += e.delta.y;
                        previewHeight = Mathf.Clamp(previewHeight, 100f, 600f); // 높이 제한
                        Repaint();
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (isResizingPreview)
                    {
                        isResizingPreview = false;
                        e.Use();
                    }
                    break;
            }
        }

        private void DrawSequencerPreviewArea()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Height(previewHeight));
            GUILayout.Label("🎥 3D Visual Preview (RenderTexture)", EditorStyles.boldLabel);

            string currentDialogue = "";
            string currentGimmick = "";
            bool isCharging = false;

            foreach (var clip in seqClips)
            {
                if (seqPlayheadTime >= clip.startTime && seqPlayheadTime <= (clip.startTime + clip.duration))
                {
                    if (clip.trackIndex == 0)
                        currentDialogue = clip.content;
                    else if (clip.trackIndex == 1)
                    {
                        currentGimmick += $"[Actor] {clip.content}\n";
                        isCharging = clip.content.Contains("Charging");
                    }
                    else if (clip.trackIndex == 2)
                        currentGimmick += $"[Event] {clip.content}\n";
                }
            }

            EditorGUILayout.BeginHorizontal();

            // Left: 3D Render
            Rect previewRect = GUILayoutUtility.GetRect(position.width * 0.4f, previewHeight - 30f);
            if (previewUtility != null)
            {
                previewUtility.BeginPreview(previewRect, GUIStyle.none);
                var renderer = previewCharacter.GetComponent<Renderer>();
                var propBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propBlock);

                if (isCharging)
                {
                    propBlock.SetColor("_BaseColor", Color.red);
                    propBlock.SetColor("_Color", Color.red);
                    previewCharacter.transform.Rotate(Vector3.up, 2f);
                }
                else
                {
                    propBlock.SetColor("_BaseColor", Color.white);
                    propBlock.SetColor("_Color", Color.white);
                    previewCharacter.transform.rotation = Quaternion.identity;
                }
                renderer.SetPropertyBlock(propBlock);
                previewUtility.camera.Render();
                Texture previewTexture = previewUtility.EndPreview();
                GUI.DrawTexture(previewRect, previewTexture, ScaleMode.StretchToFill, false);

                GUIStyle subtitleStyle = new GUIStyle(EditorStyles.boldLabel);
                subtitleStyle.alignment = TextAnchor.LowerCenter;
                subtitleStyle.normal.textColor = Color.yellow;
                subtitleStyle.wordWrap = true;
                subtitleStyle.fontSize = 14;
                GUI.Label(new Rect(previewRect.x, previewRect.yMax - 30, previewRect.width, 30), currentDialogue, subtitleStyle);
            }

            // Right: Logs
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Active Events / Logs:", EditorStyles.whiteMiniLabel);
            GUILayout.Label(currentGimmick, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void LoadYarnScript(string path)
        {
            seqClips.Clear();
            if (!File.Exists(path)) { Debug.LogError($"[Sequencer] Not found at {path}"); return; }

            string[] lines = File.ReadAllLines(path);
            float currentTime = 0f;
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//") || trimmed.StartsWith("title:") || trimmed.StartsWith("---") || trimmed.StartsWith("==="))
                    continue;

                if (trimmed.StartsWith("<<") && trimmed.EndsWith(">>"))
                {
                    string command = trimmed.Substring(2, trimmed.Length - 4);
                    int trackIdx = command.Contains("animation") ? 1 : 2;
                    seqClips.Add(new TimelineClip { trackIndex = trackIdx, startTime = currentTime, duration = 1.0f, content = command, color = new Color(0.8f, 0.5f, 0.2f) });
                    currentTime += 1.0f;
                }
                else if (trimmed.StartsWith("->"))
                {
                    seqClips.Add(new TimelineClip { trackIndex = 2, startTime = currentTime, duration = 2.0f, content = "Choice: " + trimmed.Substring(2), color = new Color(0.2f, 0.6f, 0.8f) });
                    currentTime += 2.0f;
                }
                else if (trimmed.Contains(":"))
                {
                    seqClips.Add(new TimelineClip { trackIndex = 0, startTime = currentTime, duration = 2.0f, content = trimmed, color = new Color(0.2f, 0.7f, 0.3f) });
                    currentTime += 2.0f;
                }
            }
            seqTimelineWidth = Mathf.Max(2000f, (currentTime + 5f) * 100f);
        }

        private bool cameraPrefsLoaded = false;
        private Vector3 hubBackOffset = new Vector3(0, 2.5f, -5f);
        private float hubBackPitch = 15f;
        private float hubBackYaw = 0f;
        private Vector3 hubQuarterOffset = new Vector3(0, 7f, -6f);
        private float hubQuarterPitch = 45f;
        private float hubQuarterYaw = 0f;

        private void LoadCameraPrefs()
        {
            hubBackOffset = new Vector3(EditorPrefs.GetFloat("SG_BackOffsetX", 0f), EditorPrefs.GetFloat("SG_BackOffsetY", 2.5f), EditorPrefs.GetFloat("SG_BackOffsetZ", -5f));
            hubBackPitch = EditorPrefs.GetFloat("SG_BackPitch", 15f);
            hubBackYaw = EditorPrefs.GetFloat("SG_BackYaw", 0f);
            hubQuarterOffset = new Vector3(EditorPrefs.GetFloat("SG_QuarterOffsetX", 0f), EditorPrefs.GetFloat("SG_QuarterOffsetY", 7f), EditorPrefs.GetFloat("SG_QuarterOffsetZ", -6f));
            hubQuarterPitch = EditorPrefs.GetFloat("SG_QuarterPitch", 45f);
            hubQuarterYaw = EditorPrefs.GetFloat("SG_QuarterYaw", 0f);
            cameraPrefsLoaded = true;
        }

        private void SaveCameraPrefs()
        {
            EditorPrefs.SetFloat("SG_BackOffsetX", hubBackOffset.x);
            EditorPrefs.SetFloat("SG_BackOffsetY", hubBackOffset.y);
            EditorPrefs.SetFloat("SG_BackOffsetZ", hubBackOffset.z);
            EditorPrefs.SetFloat("SG_BackPitch", hubBackPitch);
            EditorPrefs.SetFloat("SG_BackYaw", hubBackYaw);
            EditorPrefs.SetFloat("SG_QuarterOffsetX", hubQuarterOffset.x);
            EditorPrefs.SetFloat("SG_QuarterOffsetY", hubQuarterOffset.y);
            EditorPrefs.SetFloat("SG_QuarterOffsetZ", hubQuarterOffset.z);
            EditorPrefs.SetFloat("SG_QuarterPitch", hubQuarterPitch);
            EditorPrefs.SetFloat("SG_QuarterYaw", hubQuarterYaw);
        }

        private void DrawCombatManagerTab()
        {
            GUILayout.Label("⚔️ Combat Manager", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("과목 스왑 액션 핫키 및 스킬 데이터(핵심 과목 카드)를 세팅하는 패널입니다.", MessageType.Info);
            if (GUILayout.Button("Initialize Subject Hotkey Data", GUILayout.Width(250))) { }

            GUILayout.Space(20);
            GUILayout.Label("🎥 Camera Settings (Runtime Tweaks & PlayMode Persist)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("플레이 모드에서 수정한 값이 종료 후에도 EditorPrefs를 통해 유지됩니다.", MessageType.Info);

            if (!cameraPrefsLoaded) LoadCameraPrefs();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("1. BackView", EditorStyles.miniBoldLabel);
            hubBackOffset = EditorGUILayout.Vector3Field("Offset (위치)", hubBackOffset);
            hubBackPitch = EditorGUILayout.Slider("Pitch (상하 각도)", hubBackPitch, -90f, 90f);
            hubBackYaw = EditorGUILayout.Slider("Yaw (좌우 각도)", hubBackYaw, -180f, 180f);
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("2. QuarterView", EditorStyles.miniBoldLabel);
            hubQuarterOffset = EditorGUILayout.Vector3Field("Offset (위치)", hubQuarterOffset);
            hubQuarterPitch = EditorGUILayout.Slider("Pitch (상하 각도)", hubQuarterPitch, -90f, 90f);
            hubQuarterYaw = EditorGUILayout.Slider("Yaw (좌우/대각선 각도)", hubQuarterYaw, -180f, 180f);
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                SaveCameraPrefs();
            }

            StudyGame.Runtime.Combat.CameraViewManager camManager = FindFirstObjectByType<StudyGame.Runtime.Combat.CameraViewManager>();
            if (camManager != null)
            {
                camManager.backViewOffset = hubBackOffset;
                camManager.backViewPitch = hubBackPitch;
                camManager.backViewYaw = hubBackYaw;
                camManager.quarterViewOffset = hubQuarterOffset;
                camManager.quarterViewPitch = hubQuarterPitch;
                camManager.quarterViewYaw = hubQuarterYaw;

                if (!EditorApplication.isPlaying)
                {
                    EditorUtility.SetDirty(camManager);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("현재 씬에 CameraViewManager 컴포넌트가 없어 동기화 대기중...", MessageType.Warning);
            }
        }
        private void DrawTestRunnerTab()
        {
            GUILayout.Label("▶️ Test Environment Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("가상 시퀀스 로그 테스트 및 1판(One Session) 인게임 테스트를 구동하기 위해 씬을 세팅합니다.", MessageType.Info);

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("🕹️ Setup In-Game PlayTest Scene", GUILayout.Height(40)))
            {
                // 기존 [MenuItem]으로 떠돌던 "Setup Test Scene" 로직을 이 버튼 안으로 이관
                StudyGame.EditorScripts.TestSetupMenu.SetupTestScene();
            }

            if (GUILayout.Button("📜 Setup Virtual Sequence Runner (Logic Only)", GUILayout.Height(40)))
            {
                // [2단계 타겟] 3D 로드 없이 추리엔진/FSM 순수 로직만 테스트하는 씬을 구축하는 로직 연동
                Debug.Log("[DirectorHub] Virtual Sequence Runner 씬 구성 로직 호출 (TBD)");
            }

            EditorGUILayout.EndHorizontal();
        }

        // StudyGameDirectorHub.cs 내부 또는 유틸 클래스에 추가
        private void BootstrapInGameTestScene()
        {
            // 1. 빈 씬 생성 (또는 현재 씬 클리어)
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects, UnityEditor.SceneManagement.NewSceneMode.Single);

            // 2. StageRunnerController (Core Manager) 생성
            GameObject managerObj = new GameObject("@Managers");
            managerObj.AddComponent<StudyGame.Managers.StageRunnerController>();
            managerObj.AddComponent<StudyGame.Managers.CursorManager>();
            managerObj.AddComponent<StudyGame.Managers.DeductionRuleEngine>();

            // 3. UI Document 통합 생성 (하나의 Canvas 역할)
            GameObject uiRoot = new GameObject("UI_Root");
            var uiDoc = uiRoot.AddComponent<UnityEngine.UIElements.UIDocument>();
            // PanelSettings 할당 (Assets/UI Toolkit/PanelSettings.asset)
            uiDoc.panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.PanelSettings>("Assets/UI Toolkit/PanelSettings.asset");

            // 각 컨트롤러 부착 (VisualTreeAsset은 각 컨트롤러 내부 Awake에서 동적 로드하도록 처리하거나 에디터에서 수동 할당 필요)
            uiRoot.AddComponent<StudyGame.UI.UIDialogueController>(); // 대화창 (tester 씬)
            uiRoot.AddComponent<StudyGame.UI.UIQuestionSynthesizerController>(); // 문장 합성 (VerificationTestScene)
            uiRoot.AddComponent<StudyGame.UI.SNSUIController>();
            uiRoot.AddComponent<StudyGame.Combat.UISkillDeckController>(); // 스킬덱

            // 4. 테스트 시나리오 데이터 자동 할당
            var runner = managerObj.GetComponent<StudyGame.Managers.StageRunnerController>();
            // 미리 만들어둔 더미 시나리오 에셋을 로드해서 러너에 꽂아줌
            var testScenario = UnityEditor.AssetDatabase.LoadAssetAtPath<StudyGame.Data.StageScenarioData>("Assets/Data/PlayTest/TestScenario.asset");

            // (선택) 즉시 실행 원할 경우
            // runner.LoadAndRunScenario(testScenario, false); 

            Debug.Log("모든 인게임 매니저와 UI가 통합된 테스트 씬 셋업 완료!");
        }
    }
}
