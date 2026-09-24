using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using StudyGame.Data;
using UnityEditor.SceneManagement; // 씬 관리를 위해 추가

namespace StudyGame.Editor.Director
{
    public enum FloorplanTool
    {
        None,
        Brush,
        Rectangle,
        Bucket,
        Stair
    }

    public class DirectorEditorWindow : EditorWindow
    {
        // 1. [개선] Undo 처리를 유니티 순정 시스템에 맡기기 위해 ScriptableObject로 래핑
        private StageBlueprintAsset _blueprintAsset;
        private string _currentBlueprintPath;

        private Vector2 _timelineScroll;
        private float _pixelsPerSecond = 80f;
        private float _timelineLength = 60f;

        private CompositionClip _selectedClip;
        private CompositionClip _draggingClip;
        private float _dragStartTime;
        private float _dragMouseStartX;

        private CompositionClip _resizingClip;
        private float _resizeOriginalDuration;
        private float _resizeMouseStartX;

        private CompositionClip _lastSelectedClip;
        private string _yarnScriptContent = "";
        private Vector2 _yarnScrollPos;

        private const float TRACK_HEADER_WIDTH = 140f;
        private const float TRACK_HEIGHT = 48f;
        private const float RULER_HEIGHT = 28f;
        private const float TOOLBAR_HEIGHT = 32f;
        private const float INSPECTOR_WIDTH = 280f;

        private bool _cameraPrefsLoaded = false;
        private Vector3 _hubBackOffset = new Vector3(0, 2.5f, -5f);
        private float _hubBackPitch = 15f;
        private float _hubBackYaw = 0f;
        private Vector3 _hubQuarterOffset = new Vector3(0, 7f, -6f);
        private float _hubQuarterPitch = 45f;
        private float _hubQuarterYaw = 0f;

        private GUIStyle _whiteLabel;
        private GUIStyle _whiteBoldLabel;
        private GUIStyle _whiteWordWrap;

        private void InitStyles()
        {
            if (_whiteLabel == null)
            {
                _whiteLabel = new GUIStyle(EditorStyles.label);
                _whiteLabel.normal.textColor = Color.white;
            }
            if (_whiteBoldLabel == null)
            {
                _whiteBoldLabel = new GUIStyle(EditorStyles.boldLabel);
                _whiteBoldLabel.normal.textColor = Color.white;
            }
            if (_whiteWordWrap == null)
            {
                _whiteWordWrap = new GUIStyle(EditorStyles.wordWrappedLabel);
                _whiteWordWrap.normal.textColor = Color.white;
            }
        }

        [MenuItem("StudyGame/Director Hub (Master)")]
        public static void ShowWindow()
        {
            DirectorEditorWindow wnd = GetWindow<DirectorEditorWindow>();
            wnd.titleContent = new GUIContent("Director Hub");
            wnd.minSize = new Vector2(900, 400);
        }

        private void OnEnable()
        {
            if (_blueprintAsset == null)
            {
                _blueprintAsset = CreateInstance<StageBlueprintAsset>();
                _blueprintAsset.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.DontSaveInEditor;
            }
                
            string lastPath = EditorPrefs.GetString("DirectorHub_LastBlueprintPath", "");
            string tempSession = EditorPrefs.GetString("DirectorHub_TempSession", "");
            
            if (!string.IsNullOrEmpty(lastPath) && System.IO.File.Exists(lastPath))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(lastPath);
                    var loaded = JsonUtility.FromJson<StageBlueprintData>(json);
                    if (loaded != null)
                    {
                        _blueprintAsset.Blueprint = loaded;
                        _currentBlueprintPath = lastPath;
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Director Hub] Failed to load last blueprint: {e.Message}");
                }
            }
            else if (!string.IsNullOrEmpty(tempSession))
            {
                // Restore Untitled session
                var loaded = JsonUtility.FromJson<StageBlueprintData>(tempSession);
                if (loaded != null)
                {
                    _blueprintAsset.Blueprint = loaded;
                    _currentBlueprintPath = "";
                    return;
                }
            }
            
            _blueprintAsset.Blueprint = StageBlueprintData.CreateDefault();
        }

        private void OnDisable()
        {
            if (_blueprintAsset != null)
            {
                string json = JsonUtility.ToJson(_blueprintAsset.Blueprint, true);
                if (!string.IsNullOrEmpty(_currentBlueprintPath))
                {
                    System.IO.File.WriteAllText(_currentBlueprintPath, json);
                    EditorPrefs.SetString("DirectorHub_LastBlueprintPath", _currentBlueprintPath);
                    EditorPrefs.SetString("DirectorHub_TempSession", "");
                }
                else
                {
                    // Backup Untitled stage
                    EditorPrefs.SetString("DirectorHub_LastBlueprintPath", "");
                    EditorPrefs.SetString("DirectorHub_TempSession", json);
                }
                
                DestroyImmediate(_blueprintAsset);
            }
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.focusable = true;

            var imguiContainer = new IMGUIContainer(OnGUI_Timeline);
            imguiContainer.style.flexGrow = 1;
            root.Add(imguiContainer);

            EditorApplication.update += () => { if (imguiContainer != null) imguiContainer.MarkDirtyRepaint(); };
        }

        private void OnGUI_Timeline()
        {
            DrawToolbar();
            DrawTimeline();
            DrawInspector();
            HandleTimelineInput();
        }

        private void DrawToolbar()
        {
            Rect toolbarRect = new Rect(0, 0, position.width, TOOLBAR_HEIGHT);
            EditorGUI.DrawRect(toolbarRect, new Color(0.18f, 0.18f, 0.18f));

            float x = 8f;
            float y = 4f;
            float btnH = 24f;

            if (GUI.Button(new Rect(x, y, 100, btnH), "💾 Save Stage"))
            {
                SaveBlueprint();
            }
            x += 108;

            if (GUI.Button(new Rect(x, y, 100, btnH), "📂 Load Stage"))
            {
                LoadBlueprint();
            }
            x += 108;

            x += 20;

            GUI.backgroundColor = new Color(0.2f, 0.7f, 0.3f);
            // 통합 샌드박스 씬 부트스트랩 실행
            if (GUI.Button(new Rect(x, y, 180, btnH), "▶️ PlayTest (Player Control)"))
            {
                BootstrapSandboxScene();
            }
            GUI.backgroundColor = Color.white;
            x += 188;

            x += 20;

            if (GUI.Button(new Rect(x, y, 80, btnH), "+ Track"))
            {
                ShowAddTrackMenu();
            }
            x += 88;

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = new Color(0.7f, 0.7f, 0.7f) } };
            string name = string.IsNullOrEmpty(_currentBlueprintPath) ? "Untitled Stage" : System.IO.Path.GetFileNameWithoutExtension(_currentBlueprintPath);
            GUI.Label(new Rect(position.width - 300, y + 2, 290, 20), $"📋 {name}", titleStyle);
        }

        private void DrawTimeline()
        {
            float startY = TOOLBAR_HEIGHT;
            float totalHeight = position.height - startY;
            float totalWidth = position.width - INSPECTOR_WIDTH;

            float contentWidth = TRACK_HEADER_WIDTH + _timelineLength * _pixelsPerSecond;
            float contentHeight = RULER_HEIGHT + _blueprintAsset.Blueprint.Tracks.Count * TRACK_HEIGHT + 20;

            _timelineScroll = GUI.BeginScrollView(
                new Rect(0, startY, totalWidth, totalHeight),
                _timelineScroll,
                new Rect(0, 0, contentWidth, Mathf.Max(contentHeight, totalHeight)));

            DrawRuler();

            for (int i = 0; i < _blueprintAsset.Blueprint.Tracks.Count; i++)
            {
                DrawTrack(i, _blueprintAsset.Blueprint.Tracks[i]);
            }

            GUI.EndScrollView();
        }

        private void DrawRuler()
        {
            float rulerY = 0;
            Rect rulerRect = new Rect(TRACK_HEADER_WIDTH, rulerY, _timelineLength * _pixelsPerSecond, RULER_HEIGHT);
            EditorGUI.DrawRect(rulerRect, new Color(0.12f, 0.12f, 0.12f));

            GUIStyle tickStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(0.6f, 0.6f, 0.6f) } };

            for (float t = 0; t <= _timelineLength; t += 1f)
            {
                float x = TRACK_HEADER_WIDTH + t * _pixelsPerSecond;
                EditorGUI.DrawRect(new Rect(x, rulerY + RULER_HEIGHT - 8, 1, 8), new Color(0.4f, 0.4f, 0.4f));

                if (t % 5 == 0)
                {
                    EditorGUI.DrawRect(new Rect(x, rulerY + RULER_HEIGHT - 14, 1, 14), new Color(0.6f, 0.6f, 0.6f));
                    GUI.Label(new Rect(x + 2, rulerY + 2, 40, 16), $"{t:F0}s", tickStyle);
                }
            }
        }

        private void DrawInspector()
        {
            InitStyles();
            Rect inspectorRect = new Rect(position.width - INSPECTOR_WIDTH, TOOLBAR_HEIGHT, INSPECTOR_WIDTH, position.height - TOOLBAR_HEIGHT);
            EditorGUI.DrawRect(inspectorRect, new Color(0.2f, 0.2f, 0.2f));

            GUILayout.BeginArea(new Rect(inspectorRect.x + 10, inspectorRect.y + 10, inspectorRect.width - 20, inspectorRect.height - 20));

            Color originalContentColor = GUI.contentColor;
            GUI.contentColor = Color.white;

            GUILayout.Label("📋 Clip Inspector", _whiteBoldLabel);
            GUILayout.Space(10);

            if (_selectedClip != _lastSelectedClip)
            {
                _lastSelectedClip = _selectedClip;
                _yarnScriptContent = "";
                if (_selectedClip != null)
                {
                    var track = FindTrackForClip(_selectedClip);
                    if (track != null && track.TrackType == CompositionTrackType.Scenario)
                    {
                        if (!string.IsNullOrEmpty(_selectedClip.EpisodeGraphPath) && System.IO.File.Exists(_selectedClip.EpisodeGraphPath))
                        {
                            _yarnScriptContent = System.IO.File.ReadAllText(_selectedClip.EpisodeGraphPath);
                        }
                    }
                }
            }

            if (_selectedClip == null)
            {
                GUILayout.Label("No clip selected. Click a clip on the timeline.", _whiteLabel);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Name", _whiteLabel, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                string newLabel = EditorGUILayout.TextField(_selectedClip.Label);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Start Time", _whiteLabel, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                float newStart = EditorGUILayout.FloatField(_selectedClip.TimeStart);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Duration", _whiteLabel, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                float newDur = EditorGUILayout.FloatField(_selectedClip.Duration);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Color", _whiteLabel, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                Color newColor = EditorGUILayout.ColorField(_selectedClip.ClipColor);
                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_blueprintAsset, "Modify Clip");
                    _selectedClip.Label = newLabel;
                    _selectedClip.TimeStart = newStart;
                    _selectedClip.Duration = newDur;
                    _selectedClip.ClipColor = newColor;
                }

                GUILayout.Space(15);
                var track = FindTrackForClip(_selectedClip);
                if (track != null)
                {
                    EditorGUI.BeginChangeCheck();
                    string changedPath = null;

                    switch (track.TrackType)
                    {
                        case CompositionTrackType.Environment:
                            GUILayout.Label("🏗️ ProBuilder Level Planner", _whiteBoldLabel);
                            
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("Template Name", _whiteLabel, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                            string newTemplate = EditorGUILayout.TextField(_selectedClip.Label);
                            EditorGUILayout.EndHorizontal();
                            if (EditorGUI.EndChangeCheck()) { Undo.RecordObject(_blueprintAsset, "Modify Environment"); _selectedClip.Label = newTemplate; }
                            
                            Vector2 size = new Vector2(10, 10);
                            if (!string.IsNullOrEmpty(_selectedClip.FloorplanJsonPath))
                            {
                                string[] parts = _selectedClip.FloorplanJsonPath.Split(',');
                                if (parts.Length == 2)
                                {
                                    float.TryParse(parts[0], out size.x);
                                    float.TryParse(parts[1], out size.y);
                                }
                            }
                            
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("Room Size (W x L)", _whiteLabel, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                            size = EditorGUILayout.Vector2Field("", size);
                            EditorGUILayout.EndHorizontal();
                            if (EditorGUI.EndChangeCheck()) 
                            { 
                                Undo.RecordObject(_blueprintAsset, "Modify Environment"); 
                                _selectedClip.FloorplanJsonPath = $"{size.x},{size.y}"; 
                                BuildProBuilderRoom(_selectedClip, size, _selectedClip.DataAssetPath, false); // Real-time rebuild
                            }
                            
                            EditorGUI.BeginChangeCheck();
                            Material currentMat = null;
                            if (!string.IsNullOrEmpty(_selectedClip.DataAssetPath))
                            {
                                currentMat = AssetDatabase.LoadAssetAtPath<Material>(_selectedClip.DataAssetPath);
                                if (currentMat == null)
                                {
                                    // Handle built-in extra resources
                                    if (_selectedClip.DataAssetPath.Contains("unity_builtin_extra"))
                                    {
                                        currentMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
                                    }
                                }
                            }
                            
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("Room Material", _whiteLabel, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                            var newMat = (Material)EditorGUILayout.ObjectField(currentMat, typeof(Material), false);
                            EditorGUILayout.EndHorizontal();
                            
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(_blueprintAsset, "Modify Material");
                                if (newMat != null)
                                {
                                    string path = AssetDatabase.GetAssetPath(newMat);
                                    _selectedClip.DataAssetPath = string.IsNullOrEmpty(path) ? "Resources/unity_builtin_extra" : path;
                                }
                                else
                                {
                                    _selectedClip.DataAssetPath = "";
                                }
                                BuildProBuilderRoom(_selectedClip, size, _selectedClip.DataAssetPath, false);
                            }

                            GUILayout.Space(10);
                            if (GUILayout.Button("🔨 Build ProBuilder Mesh in Scene", GUILayout.Height(30)))
                            {
                                BuildProBuilderRoom(_selectedClip, size, _selectedClip.DataAssetPath, true);
                            }
                            break;

                        case CompositionTrackType.Scenario:
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("Yarn Path", _whiteLabel, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                            changedPath = EditorGUILayout.TextField(_selectedClip.EpisodeGraphPath);
                            EditorGUILayout.EndHorizontal();
                            if (EditorGUI.EndChangeCheck()) { 
                                Undo.RecordObject(_blueprintAsset, "Modify Scenario"); 
                                _selectedClip.EpisodeGraphPath = changedPath; 
                                _lastSelectedClip = null; 
                            }
                            GUILayout.Space(5);
                            if (GUILayout.Button("📂 Select Yarn File", GUILayout.Height(25))) 
                            {
                                string path = EditorUtility.OpenFilePanel("Select Yarn Script", "Assets", "yarn");
                                if (!string.IsNullOrEmpty(path) && path.Contains("Assets/"))
                                {
                                    Undo.RecordObject(_blueprintAsset, "Modify Scenario");
                                    _selectedClip.EpisodeGraphPath = path.Substring(path.IndexOf("Assets/"));
                                    _selectedClip.Label = System.IO.Path.GetFileNameWithoutExtension(_selectedClip.EpisodeGraphPath);
                                    _lastSelectedClip = null;
                                }
                            }
                            
                            GUILayout.Space(10);
                            GUILayout.Label("📝 Yarn Editor (Inline)", _whiteBoldLabel);
                            if (string.IsNullOrEmpty(_selectedClip.EpisodeGraphPath))
                            {
                                GUILayout.Label("Select a valid .yarn file above.", _whiteWordWrap);
                            }
                            else
                            {
                                EditorGUILayout.BeginHorizontal();
                                GUI.backgroundColor = new Color(0.2f, 0.6f, 0.9f);
                                if (GUILayout.Button("💾 Save", GUILayout.Height(30)))
                                {
                                    System.IO.File.WriteAllText(_selectedClip.EpisodeGraphPath, _yarnScriptContent);
                                    EditorApplication.delayCall += () => AssetDatabase.Refresh();
                                    Debug.Log($"[Director Hub] Saved yarn script to {_selectedClip.EpisodeGraphPath}");
                                }
                                GUI.backgroundColor = new Color(0.8f, 0.4f, 0.8f);
                                if (GUILayout.Button("🔄 Sync to Tracks", GUILayout.Height(30)))
                                {
                                    SyncYarnCommandsToTracks(_selectedClip);
                                }
                                GUI.backgroundColor = Color.white;
                                EditorGUILayout.EndHorizontal();

                                GUILayout.Space(5);
                                _yarnScrollPos = EditorGUILayout.BeginScrollView(_yarnScrollPos, GUILayout.ExpandHeight(true));
                                GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
                                textAreaStyle.wordWrap = true;
                                _yarnScriptContent = EditorGUILayout.TextArea(_yarnScriptContent, textAreaStyle, GUILayout.ExpandHeight(true));
                                EditorGUILayout.EndScrollView();
                            }
                            break;

                        case CompositionTrackType.Camera:
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("Preset Key", _whiteLabel, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                            changedPath = EditorGUILayout.TextField(_selectedClip.CameraPresetKey);
                            EditorGUILayout.EndHorizontal();
                            if (EditorGUI.EndChangeCheck()) { Undo.RecordObject(_blueprintAsset, "Modify Camera"); _selectedClip.CameraPresetKey = changedPath; }
                            GUILayout.Space(5);
                            if (GUILayout.Button("⚔️ Edit Camera Settings", GUILayout.Height(30))) OpenClipEditor(_selectedClip);
                            break;

                        case CompositionTrackType.Factory:
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("Menu Command", _whiteLabel, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                            changedPath = EditorGUILayout.TextField(_selectedClip.FactoryMenuCommand);
                            EditorGUILayout.EndHorizontal();
                            if (EditorGUI.EndChangeCheck()) { Undo.RecordObject(_blueprintAsset, "Modify Factory"); _selectedClip.FactoryMenuCommand = changedPath; }
                            GUILayout.Space(5);
                            if (GUILayout.Button("🏭 Run Factory Command", GUILayout.Height(30))) OpenClipEditor(_selectedClip);
                            break;

                        case CompositionTrackType.Data:
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("Asset Path", _whiteLabel, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                            changedPath = EditorGUILayout.TextField(_selectedClip.DataAssetPath);
                            EditorGUILayout.EndHorizontal();
                            if (EditorGUI.EndChangeCheck()) { Undo.RecordObject(_blueprintAsset, "Modify Data"); _selectedClip.DataAssetPath = changedPath; }
                            GUILayout.Space(5);
                            if (GUILayout.Button("📄 Select Data Asset", GUILayout.Height(30))) OpenClipEditor(_selectedClip);
                            break;

                        case CompositionTrackType.TestRunner:
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("Scene Path", _whiteLabel, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                            changedPath = EditorGUILayout.TextField(_selectedClip.ScenePath);
                            EditorGUILayout.EndHorizontal();
                            if (EditorGUI.EndChangeCheck()) { Undo.RecordObject(_blueprintAsset, "Modify Scene"); _selectedClip.ScenePath = changedPath; }
                            GUILayout.Space(5);
                            if (GUILayout.Button("▶️ Open Scene", GUILayout.Height(30))) OpenClipEditor(_selectedClip);
                            break;
                    }
                }
            }

            GUI.contentColor = originalContentColor;
            GUILayout.EndArea();

            EditorGUI.DrawRect(new Rect(position.width - INSPECTOR_WIDTH, TOOLBAR_HEIGHT, 1, position.height - TOOLBAR_HEIGHT), Color.black);
        }

        private void DrawTrack(int index, CompositionTrack track)
        {
            float trackY = RULER_HEIGHT + index * TRACK_HEIGHT;

            Color bgColor = index % 2 == 0 ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.22f, 0.22f, 0.22f);
            EditorGUI.DrawRect(new Rect(0, trackY, TRACK_HEADER_WIDTH + _timelineLength * _pixelsPerSecond, TRACK_HEIGHT), bgColor);

            Rect headerRect = new Rect(0, trackY, TRACK_HEADER_WIDTH, TRACK_HEIGHT);
            EditorGUI.DrawRect(headerRect, new Color(0.15f, 0.15f, 0.15f));

            EditorGUI.DrawRect(new Rect(0, trackY, 4, TRACK_HEIGHT), track.TrackColor);

            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.white }, fontSize = 11 };
            GUI.Label(new Rect(10, trackY + 4, TRACK_HEADER_WIDTH - 40, 20), track.TrackName, headerStyle);

            if (GUI.Button(new Rect(TRACK_HEADER_WIDTH - 28, trackY + 14, 20, 20), "+"))
            {
                AddClipToTrack(track);
            }

            foreach (var clip in track.Clips)
            {
                DrawClip(trackY, clip, track);
            }
        }

        private void DrawClip(float trackY, CompositionClip clip, CompositionTrack track)
        {
            float clipX = TRACK_HEADER_WIDTH + clip.TimeStart * _pixelsPerSecond;
            float clipW = Mathf.Max(clip.Duration * _pixelsPerSecond, 30f);
            float clipY = trackY + 4;
            float clipH = TRACK_HEIGHT - 8;

            Rect clipRect = new Rect(clipX, clipY, clipW, clipH);

            Color baseColor = clip.ClipColor;
            if (clip == _selectedClip)
                baseColor = Color.Lerp(baseColor, Color.white, 0.3f);

            EditorGUI.DrawRect(clipRect, baseColor);

            EditorGUI.DrawRect(new Rect(clipX, clipY, clipW, 2), Color.Lerp(baseColor, Color.white, 0.4f));
            EditorGUI.DrawRect(new Rect(clipX, clipY + clipH - 1, clipW, 1), Color.Lerp(baseColor, Color.black, 0.3f));

            GUIStyle clipLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = Color.white },
                fontStyle = FontStyle.Bold,
                clipping = TextClipping.Clip,
                alignment = TextAnchor.MiddleLeft
            };

            GUI.Label(new Rect(clipX + 6, clipY, clipW - 12, clipH), clip.Label, clipLabelStyle);

            Rect resizeHandle = new Rect(clipX + clipW - 6, clipY, 6, clipH);
            EditorGUIUtility.AddCursorRect(resizeHandle, MouseCursor.ResizeHorizontal);
        }

        private void HandleTimelineInput()
        {
            Event e = Event.current;

            if (e.type == EventType.ScrollWheel && e.control)
            {
                _pixelsPerSecond = Mathf.Clamp(_pixelsPerSecond - e.delta.y * 5f, 20f, 300f);
                e.Use();
                return;
            }

            float startY = TOOLBAR_HEIGHT;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Vector2 mousePos = e.mousePosition + _timelineScroll - new Vector2(0, startY);

                Rect resizeHandle;
                var resizeClip = FindClipResizeHandle(mousePos, out resizeHandle);
                if (resizeClip != null)
                {
                    Undo.RecordObject(_blueprintAsset, "Resize Clip");
                    _resizingClip = resizeClip;
                    _resizeOriginalDuration = resizeClip.Duration;
                    _resizeMouseStartX = mousePos.x;
                    _selectedClip = resizeClip;
                    e.Use();
                    return;
                }

                var hitClip = FindClipAtPosition(mousePos);
                if (hitClip != null)
                {
                    Undo.RecordObject(_blueprintAsset, "Move Clip");
                    _selectedClip = hitClip;
                    _draggingClip = hitClip;
                    _dragStartTime = hitClip.TimeStart;
                    _dragMouseStartX = mousePos.x;
                    e.Use();
                    return;
                }

                _selectedClip = null;
            }
            else if (e.type == EventType.MouseDrag && e.button == 0)
            {
                Vector2 mousePos = e.mousePosition + _timelineScroll - new Vector2(0, startY);

                if (_resizingClip != null)
                {
                    float deltaX = mousePos.x - _resizeMouseStartX;
                    float deltaTime = deltaX / _pixelsPerSecond;
                    _resizingClip.Duration = Mathf.Max(0.5f, _resizeOriginalDuration + deltaTime);
                    e.Use();
                    return;
                }

                if (_draggingClip != null)
                {
                    float deltaX = mousePos.x - _dragMouseStartX;
                    float deltaTime = deltaX / _pixelsPerSecond;
                    _draggingClip.TimeStart = Mathf.Max(0, _dragStartTime + deltaTime);

                    float snapInterval = 0.5f;
                    _draggingClip.TimeStart = Mathf.Round(_draggingClip.TimeStart / snapInterval) * snapInterval;
                    e.Use();
                    return;
                }
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                if (_draggingClip != null || _resizingClip != null)
                {
                    _draggingClip = null;
                    _resizingClip = null;
                    e.Use();
                    return;
                }
            }
            else if (e.type == EventType.MouseDown && e.button == 1)
            {
                Vector2 mousePos = e.mousePosition + _timelineScroll - new Vector2(0, startY);
                var hitClip = FindClipAtPosition(mousePos);
                if (hitClip != null)
                {
                    _selectedClip = hitClip;
                    ShowClipContextMenu(hitClip);
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseDown && e.clickCount == 2 && e.button == 0)
            {
                Vector2 mousePos = e.mousePosition + _timelineScroll - new Vector2(0, startY);
                var hitClip = FindClipAtPosition(mousePos);
                if (hitClip != null)
                {
                    OpenClipEditor(hitClip);
                    e.Use();
                }
            }
            else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete && _selectedClip != null)
            {
                Undo.RecordObject(_blueprintAsset, "Delete Clip");
                foreach (var track in _blueprintAsset.Blueprint.Tracks)
                {
                    track.Clips.Remove(_selectedClip);
                }
                _selectedClip = null;
                e.Use();
            }
        }

        private CompositionClip FindClipAtPosition(Vector2 pos)
        {
            for (int i = 0; i < _blueprintAsset.Blueprint.Tracks.Count; i++)
            {
                float trackY = RULER_HEIGHT + i * TRACK_HEIGHT;
                if (pos.y < trackY || pos.y > trackY + TRACK_HEIGHT) continue;

                foreach (var clip in _blueprintAsset.Blueprint.Tracks[i].Clips)
                {
                    float clipX = TRACK_HEADER_WIDTH + clip.TimeStart * _pixelsPerSecond;
                    float clipW = Mathf.Max(clip.Duration * _pixelsPerSecond, 30f);
                    if (pos.x >= clipX && pos.x <= clipX + clipW)
                        return clip;
                }
            }
            return null;
        }

        private CompositionClip FindClipResizeHandle(Vector2 pos, out Rect handleRect)
        {
            handleRect = Rect.zero;
            for (int i = 0; i < _blueprintAsset.Blueprint.Tracks.Count; i++)
            {
                float trackY = RULER_HEIGHT + i * TRACK_HEIGHT;
                if (pos.y < trackY || pos.y > trackY + TRACK_HEIGHT) continue;

                foreach (var clip in _blueprintAsset.Blueprint.Tracks[i].Clips)
                {
                    float clipX = TRACK_HEADER_WIDTH + clip.TimeStart * _pixelsPerSecond;
                    float clipW = Mathf.Max(clip.Duration * _pixelsPerSecond, 30f);
                    handleRect = new Rect(clipX + clipW - 8, trackY + 4, 8, TRACK_HEIGHT - 8);
                    if (handleRect.Contains(pos))
                        return clip;
                }
            }
            return null;
        }

        private void AddClipToTrack(CompositionTrack track)
        {
            Undo.RecordObject(_blueprintAsset, "Add Clip");
            float maxEnd = 0;
            foreach (var c in track.Clips)
            {
                float end = c.TimeStart + c.Duration;
                if (end > maxEnd) maxEnd = end;
            }

            var clip = new CompositionClip
            {
                Label = GetDefaultClipLabel(track.TrackType),
                TimeStart = maxEnd + 0.5f,
                Duration = 3f,
                ClipColor = Color.Lerp(track.TrackColor, Color.white, 0.15f)
            };
            track.Clips.Add(clip);
            _selectedClip = clip;
        }

        private string GetDefaultClipLabel(CompositionTrackType type)
        {
            switch (type)
            {
                case CompositionTrackType.Environment: return "New Map";
                case CompositionTrackType.Scenario: return "New Event";
                case CompositionTrackType.Camera: return "Camera Preset";
                case CompositionTrackType.Factory: return "Generate Room";
                case CompositionTrackType.Data: return "Data Asset";
                case CompositionTrackType.TestRunner: return "Test Scene";
                default: return "New Clip";
            }
        }

        private void ShowAddTrackMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("🏗️ Environment"), false, () => { Undo.RecordObject(_blueprintAsset, "Add Track"); _blueprintAsset.Blueprint.Tracks.Add(new CompositionTrack { TrackName = "🏗️ 환경", TrackType = CompositionTrackType.Environment, TrackColor = new Color(0.2f, 0.7f, 0.3f) }); });
            menu.AddItem(new GUIContent("🕸️ Scenario"), false, () => { Undo.RecordObject(_blueprintAsset, "Add Track"); _blueprintAsset.Blueprint.Tracks.Add(new CompositionTrack { TrackName = "🕸️ 시나리오", TrackType = CompositionTrackType.Scenario, TrackColor = new Color(0.3f, 0.5f, 0.9f) }); });
            menu.AddItem(new GUIContent("⚔️ Camera/Direction"), false, () => { Undo.RecordObject(_blueprintAsset, "Add Track"); _blueprintAsset.Blueprint.Tracks.Add(new CompositionTrack { TrackName = "⚔️ 카메라/연출", TrackType = CompositionTrackType.Camera, TrackColor = new Color(0.9f, 0.4f, 0.3f) }); });
            menu.AddItem(new GUIContent("🏭 Factory"), false, () => { Undo.RecordObject(_blueprintAsset, "Add Track"); _blueprintAsset.Blueprint.Tracks.Add(new CompositionTrack { TrackName = "🏭 공장", TrackType = CompositionTrackType.Factory, TrackColor = new Color(0.8f, 0.6f, 0.2f) }); });
            menu.AddItem(new GUIContent("📄 Data"), false, () => { Undo.RecordObject(_blueprintAsset, "Add Track"); _blueprintAsset.Blueprint.Tracks.Add(new CompositionTrack { TrackName = "📄 데이터", TrackType = CompositionTrackType.Data, TrackColor = new Color(0.6f, 0.4f, 0.8f) }); });
            menu.AddItem(new GUIContent("▶️ Test Runner"), false, () => { Undo.RecordObject(_blueprintAsset, "Add Track"); _blueprintAsset.Blueprint.Tracks.Add(new CompositionTrack { TrackName = "▶️ 러너", TrackType = CompositionTrackType.TestRunner, TrackColor = new Color(0.4f, 0.8f, 0.8f) }); });
            menu.ShowAsContext();
        }

        private void ShowClipContextMenu(CompositionClip clip)
        {
            var track = FindTrackForClip(clip);
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Edit Clip..."), false, () => OpenClipEditor(clip));
            menu.AddItem(new GUIContent("Rename..."), false, () =>
            {
                var renameWnd = ScriptableObject.CreateInstance<ClipRenamePopup>();
                renameWnd.Init(clip, () => Undo.RecordObject(_blueprintAsset, "Rename Clip"));
                renameWnd.ShowAsDropDown(new Rect(Event.current.mousePosition + position.position, Vector2.zero), new Vector2(250, 50));
            });
            menu.AddSeparator("");

            if (track != null && track.TrackType == CompositionTrackType.Factory)
            {
                string[] pbMenuItems = new string[] {
                    "Tools/ProBuilder/Create Algorithm Statistics Room",
                    "Tools/ProBuilder/Create Cartography Observatory"
                };

                foreach (var menuPath in pbMenuItems)
                {
                    string shortName = menuPath.Replace("Tools/ProBuilder/Create ", "");
                    menu.AddItem(new GUIContent($"Assign Factory/{shortName}"), false, () =>
                    {
                        Undo.RecordObject(_blueprintAsset, "Assign Factory Command");
                        clip.FactoryMenuCommand = menuPath;
                        clip.Label = shortName;
                    });
                }
            }

            if (track != null && track.TrackType == CompositionTrackType.Data)
            {
                menu.AddItem(new GUIContent("Import Math Data JSON..."), false, () => ImportMathData(clip));
                menu.AddItem(new GUIContent("Open Anomaly Importer..."), false, () => AnomalyImporterWindow.ShowWindow());
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Delete Clip"), false, () =>
            {
                Undo.RecordObject(_blueprintAsset, "Delete Clip");
                if (track != null) track.Clips.Remove(clip);
                if (_selectedClip == clip) _selectedClip = null;
            });

            menu.ShowAsContext();
        }

        private void BuildProBuilderRoom(CompositionClip clip, Vector2 size, string materialPath, bool selectObject = false)
        {
            string sandboxPath = "Assets/Scenes/Sandbox.unity";
            if (UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path != sandboxPath)
            {
                if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    if (System.IO.File.Exists(sandboxPath))
                        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(sandboxPath);
                    else
                    {
                        Debug.LogWarning("[Director Hub] Sandbox 씬이 없습니다. 먼저 PlayTest 버튼을 눌러 생성하세요.");
                        return;
                    }
                }
                else return;
            }

            string templateName = string.IsNullOrEmpty(clip.Label) ? "New Map" : clip.Label;
            string roomName = $"Level_[{clip.ClipId}] {templateName}";
            
            // Delete old objects belonging to this clip
            foreach (var go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (go.name.StartsWith($"Level_[{clip.ClipId}]"))
                {
                    DestroyImmediate(go);
                }
            }

            var shape = UnityEngine.ProBuilder.ShapeGenerator.GenerateCube(UnityEngine.ProBuilder.PivotLocation.Center, new Vector3(size.x, 3f, size.y));
            shape.gameObject.name = roomName;
            
            foreach (var face in shape.faces)
            {
                face.Reverse();
            }

            Material mat = null;
            if (!string.IsNullOrEmpty(materialPath))
            {
                mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            }

            // Fallbacks: if empty, or if they explicitly assigned the Legacy Built-in material (which breaks in URP)
            if (mat == null || materialPath.Contains("unity_builtin_extra") || (mat != null && mat.name == "Default-Material"))
            {
                mat = UnityEngine.ProBuilder.BuiltinMaterials.defaultMaterial;
            }
                
            if (mat != null)
            {
                shape.GetComponent<MeshRenderer>().sharedMaterial = mat;
                shape.SetMaterial(shape.faces, mat);
            }

            shape.ToMesh();
            shape.Refresh();

            shape.gameObject.AddComponent<MeshCollider>();
            GameObjectUtility.SetStaticEditorFlags(shape.gameObject, StaticEditorFlags.ContributeGI | StaticEditorFlags.NavigationStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic);

            if (selectObject) Selection.activeGameObject = shape.gameObject;
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        private void SyncYarnCommandsToTracks(CompositionClip scenarioClip)
        {
            if (string.IsNullOrEmpty(_yarnScriptContent)) return;

            Undo.RecordObject(_blueprintAsset, "Sync Yarn Commands");

            float timeOffset = scenarioClip.TimeStart + 1f;

            var matches = System.Text.RegularExpressions.Regex.Matches(_yarnScriptContent, @"<<(\w+)\s+([^>]+)>>");
            int addedCount = 0;

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (match.Groups.Count < 3) continue;
                string cmd = match.Groups[1].Value.ToLower();
                string args = match.Groups[2].Value.Trim();

                CompositionTrack targetTrack = null;

                if (cmd.Contains("level"))
                {
                    targetTrack = _blueprintAsset.Blueprint.Tracks.Find(t => t.TrackType == CompositionTrackType.Environment);
                }
                else if (cmd.Contains("ui") || cmd.Contains("video") || cmd.Contains("camera"))
                {
                    targetTrack = _blueprintAsset.Blueprint.Tracks.Find(t => t.TrackType == CompositionTrackType.Factory);
                }
                else if (cmd.Contains("combat") || cmd.Contains("haptic") || cmd.Contains("sound"))
                {
                    targetTrack = _blueprintAsset.Blueprint.Tracks.Find(t => t.TrackType == CompositionTrackType.Data);
                }

                if (targetTrack != null)
                {
                    string shortArg = args.Split(' ')[0];
                    if (shortArg.Length > 10) shortArg = shortArg.Substring(0, 10) + "..";

                    var newClip = new CompositionClip
                    {
                        Label = $"{cmd}: {shortArg}",
                        TimeStart = timeOffset,
                        Duration = 2f,
                        ClipColor = Color.Lerp(targetTrack.TrackColor, Color.white, 0.2f)
                    };
                    targetTrack.Clips.Add(newClip);
                    timeOffset += 2.5f;
                    addedCount++;
                }
            }

            Debug.Log($"[Director Hub] Synced {addedCount} commands from Yarn to Timeline Tracks.");
        }

        private CompositionTrack FindTrackForClip(CompositionClip clip)
        {
            foreach (var track in _blueprintAsset.Blueprint.Tracks)
            {
                if (track.Clips.Contains(clip)) return track;
            }
            return null;
        }

        private void OpenClipEditor(CompositionClip clip)
        {
            var track = FindTrackForClip(clip);
            if (track == null) return;

            switch (track.TrackType)
            {
                case CompositionTrackType.Environment:
                    Debug.Log("[Director Hub] Environment is now handled inline via ProBuilder.");
                    break;
                case CompositionTrackType.Scenario:
                    WorkspaceNodeWindow.Open(clip.EpisodeGraphPath);
                    break;
                case CompositionTrackType.Camera:
                    if (string.IsNullOrEmpty(clip.CameraPresetKey))
                    {
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Assign Default Camera"), false, () => { clip.CameraPresetKey = "DefaultCamera"; clip.Label = "DefaultCamera"; });
                        menu.AddItem(new GUIContent("Assign Cutscene Camera"), false, () => { clip.CameraPresetKey = "CutsceneCamera"; clip.Label = "CutsceneCamera"; });
                        menu.ShowAsContext();
                    }
                    else
                    {
                        Debug.Log($"[Director Hub] Camera Settings editor would open for preset: {clip.CameraPresetKey}");
                    }
                    break;
                case CompositionTrackType.Factory:
                    if (!string.IsNullOrEmpty(clip.FactoryMenuCommand)) 
                    {
                        EditorApplication.ExecuteMenuItem(clip.FactoryMenuCommand);
                    }
                    else
                    {
                        ShowClipContextMenu(clip);
                    }
                    break;
                case CompositionTrackType.Data:
                    if (!string.IsNullOrEmpty(clip.DataAssetPath))
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(clip.DataAssetPath);
                        if (asset != null) Selection.activeObject = asset;
                    }
                    else
                    {
                        string path = EditorUtility.OpenFilePanel("Select Data Asset", "Assets", "asset,json");
                        if (!string.IsNullOrEmpty(path) && path.Contains("Assets/"))
                        {
                            clip.DataAssetPath = path.Substring(path.IndexOf("Assets/"));
                            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(clip.DataAssetPath);
                            if (asset != null) clip.Label = asset.name;
                        }
                    }
                    break;
                case CompositionTrackType.TestRunner:
                    if (!string.IsNullOrEmpty(clip.ScenePath))
                    {
                        if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(clip.ScenePath);
                        }
                    }
                    else
                    {
                        string path = EditorUtility.OpenFilePanel("Select Scene", "Assets/Scenes", "unity");
                        if (!string.IsNullOrEmpty(path) && path.Contains("Assets/"))
                        {
                            clip.ScenePath = path.Substring(path.IndexOf("Assets/"));
                            clip.Label = System.IO.Path.GetFileNameWithoutExtension(clip.ScenePath);
                        }
                    }
                    break;
            }
        }

        private void ImportMathData(CompositionClip clip)
        {
            string path = EditorUtility.OpenFilePanel("Select math_world_hierarchy.json", "", "json");
            if (string.IsNullOrEmpty(path)) return;

            string jsonText = System.IO.File.ReadAllText(path);

            // 4. [개선] 70줄짜리 하드코딩 DTO 버리고 유니티 내장 JsonUtility 사용
            MathData asset = ScriptableObject.CreateInstance<MathData>();
            JsonUtility.FromJsonOverwrite(jsonText, asset);

            string savePath = "Assets/Resources/MathData.asset";
            string dir = System.IO.Path.GetDirectoryName(savePath);
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

            AssetDatabase.CreateAsset(asset, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Undo.RecordObject(_blueprintAsset, "Import Math Data");
            clip.DataAssetPath = savePath;
            clip.Label = "MathData";

            EditorUtility.DisplayDialog("Import Success", "MathData.asset created at " + savePath, "OK");
        }

        private void SaveBlueprint()
        {
            if (string.IsNullOrEmpty(_currentBlueprintPath))
            {
                _currentBlueprintPath = EditorUtility.SaveFilePanel("Save Stage Blueprint", "Assets", "StageBlueprintData", "json");
                if (string.IsNullOrEmpty(_currentBlueprintPath)) return;
            }
            string json = JsonUtility.ToJson(_blueprintAsset.Blueprint, true);
            System.IO.File.WriteAllText(_currentBlueprintPath, json);
            EditorPrefs.SetString("DirectorHub_LastBlueprintPath", _currentBlueprintPath);
            AssetDatabase.Refresh();
            Debug.Log($"[Director Hub] Stage Blueprint saved to {_currentBlueprintPath}");
        }

        private void LoadBlueprint()
        {
            string path = EditorUtility.OpenFilePanel("Load Stage Blueprint", "Assets", "json");
            if (string.IsNullOrEmpty(path)) return;

            string json = System.IO.File.ReadAllText(path);
            var loaded = JsonUtility.FromJson<StageBlueprintData>(json);
            if (loaded != null)
            {
                Undo.RecordObject(_blueprintAsset, "Load Blueprint");
                _blueprintAsset.Blueprint = loaded;
                _currentBlueprintPath = path;
                _selectedClip = null;
                EditorPrefs.SetString("DirectorHub_LastBlueprintPath", _currentBlueprintPath);
                Debug.Log($"[Director Hub] Stage Blueprint loaded from {path}");
            }
        }

        // =================================================================
        // 2 & 5. [가장 중요한 개선] 하드디스크 더미 씬 저장 금지 및 안전한 데이터 전달
        // =================================================================
        private void BootstrapSandboxScene()
        {
            if (EditorApplication.isPlaying) return;

            if (!UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            string sandboxPath = "Assets/Scenes/Sandbox.unity";
            
            // 만약 샌드박스 씬이 없다면, 최초 1회 생성하여 기본 요소들을 배치합니다.
            if (AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(sandboxPath) == null)
            {
                if (!System.IO.Directory.Exists("Assets/Scenes")) System.IO.Directory.CreateDirectory("Assets/Scenes");

                var newScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
                
                // 1. 메인 카메라
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                camGo.AddComponent<AudioListener>();
                camGo.transform.position = new Vector3(0, 10, -10);
                camGo.transform.rotation = Quaternion.Euler(45, 0, 0);

                // 2. 플레이어 프리팹 (프로젝트 내 유일한 프리팹)
                var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player_Test.prefab");
                if (playerPrefab != null) 
                {
                    var playerObj = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
                    playerObj.transform.position = Vector3.zero;
                }
                else 
                {
                    Debug.LogWarning("[Director Hub] Player_Test.prefab을 찾을 수 없습니다. 샌드박스 씬에 수동으로 플레이어를 추가해주세요.");
                }

                // 3. Yarn Dialogue Runner & Command Handler (껍데기)
                new GameObject("DialogueRunner");
                new GameObject("EpisodeCommandHandler").AddComponent<StudyGame.Interaction.EpisodeCommandHandler>();
                
                // 4. 모듈 허브 및 매니저
                new GameObject("@CursorManager");
                new GameObject("StageManager");

                // 5. 기본 조명
                var lightGo = new GameObject("Directional Light");
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);

                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(newScene, sandboxPath);
                AssetDatabase.Refresh();
            }

            // 무조건 샌드박스 씬 열기
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(sandboxPath);

            SessionState.SetString("StudyGame_PendingSandboxMode", "true");
            SessionState.SetString("StudyGame_SandboxBlueprint", JsonUtility.ToJson(_blueprintAsset.Blueprint));

            EditorApplication.EnterPlaymode();
        }
    }

    // (ClipRenamePopup 클래스는 그대로 유지...)
    public class ClipRenamePopup : EditorWindow
    {
        private CompositionClip _clip;
        private string _newName;
        private Action _onBeforeRename;

        public void Init(CompositionClip clip, Action onBeforeRename)
        {
            _clip = clip;
            _newName = clip.Label;
            _onBeforeRename = onBeforeRename;
            titleContent = new GUIContent("Rename Clip");
        }

        private void OnGUI()
        {
            _newName = EditorGUILayout.TextField("Name", _newName);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("OK"))
            {
                _onBeforeRename?.Invoke();
                _clip.Label = _newName;
                Close();
            }
            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    // 런타임 진입 시 가로채서 즉석에서 조립하는 훅 (안전하게 변경됨)
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
                // EditorPrefs가 아닌 안전한 SessionState 사용
                if (SessionState.GetString("StudyGame_PendingSandboxMode", "false") == "true")
                {
                    SessionState.EraseString("StudyGame_PendingSandboxMode");

                    string blueprintJson = SessionState.GetString("StudyGame_SandboxBlueprint", "");
                    if (!string.IsNullOrEmpty(blueprintJson))
                    {
                        var blueprint = JsonUtility.FromJson<StageBlueprintData>(blueprintJson);

                        // 1. Factory 커맨드 등 UI 팝업 실행 (씬이 열린 직후이므로 가능)
                        foreach (var track in blueprint.Tracks)
                        {
                            if (track.TrackType == CompositionTrackType.Factory)
                            {
                                foreach (var clip in track.Clips)
                                {
                                    // 공장 커맨드로 UI 캔버스 등을 동적 생성
                                    if (!string.IsNullOrEmpty(clip.FactoryMenuCommand))
                                        EditorApplication.ExecuteMenuItem(clip.FactoryMenuCommand);
                                }
                            }
                        }

                        // 2. 맵 지오메트리 로드 및 생성 (기존 커스텀 로직 제거, ProBuilder 씬 기반으로 대체)
                        // (미리 씬에 빌드된 ProBuilder 메쉬를 그대로 사용하므로 런타임 생성이 필요 없습니다.)

                        // 3. 인풋 시뮬레이터 제거 (유저 수동 조작 보장)
                        var player = UnityEngine.Object.FindFirstObjectByType<StudyGame.Player.PlayerController>(FindObjectsInactive.Include);
                        if (player != null) player.SetMovementEnabled(true);

                        // 4. Yarn 스크립트 연결 확인
                        foreach (var track in blueprint.Tracks)
                        {
                            if (track.TrackType == CompositionTrackType.Scenario)
                            {
                                foreach (var clip in track.Clips)
                                {
                                    if (!string.IsNullOrEmpty(clip.EpisodeGraphPath) && clip.EpisodeGraphPath.EndsWith(".yarn"))
                                    {
                                        Debug.Log($"[SandboxTestRunner] 주입된 Yarn 스크립트: {clip.EpisodeGraphPath}. \n(실제 대화 출력을 원하시면 씬의 DialogueRunner에 YarnProject를 할당하세요.)");
                                    }
                                }
                            }
                        }

                        Debug.Log("🚀 통합 샌드박스 씬(Sandbox.unity) 부트스트랩 완료! 이제 수동 조작이 가능합니다.");
                    }
                }
            }
        }
    }

    // ScriptableObject 래퍼 (Undo/Redo 지원용)
    public class StageBlueprintAsset : ScriptableObject
    {
        public StageBlueprintData Blueprint;
    }
}