using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using StudyGame.Data;

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
        private StageBlueprintData _blueprint;
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

        private Stack<string> _undoStack = new Stack<string>();
        private Stack<string> _redoStack = new Stack<string>();

        private void RecordUndo()
        {
            if (_blueprint != null)
                _undoStack.Push(JsonConvert.SerializeObject(_blueprint));
            _redoStack.Clear();
        }

        private void PerformUndo()
        {
            if (_undoStack.Count > 0)
            {
                _redoStack.Push(JsonConvert.SerializeObject(_blueprint));
                _blueprint = JsonConvert.DeserializeObject<StageBlueprintData>(_undoStack.Pop());
                _selectedClip = null;
                Repaint();
            }
        }

        private void PerformRedo()
        {
            if (_redoStack.Count > 0)
            {
                _undoStack.Push(JsonConvert.SerializeObject(_blueprint));
                _blueprint = JsonConvert.DeserializeObject<StageBlueprintData>(_redoStack.Pop());
                _selectedClip = null;
                Repaint();
            }
        }

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

        [MenuItem("StudyGame/Director Hub (Master)")]
        public static void ShowWindow()
        {
            DirectorEditorWindow wnd = GetWindow<DirectorEditorWindow>();
            wnd.titleContent = new GUIContent("Director Hub");
            wnd.minSize = new Vector2(900, 400);
        }

        private void OnEnable()
        {
            if (_blueprint == null)
                _blueprint = StageBlueprintData.CreateDefault();
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
            float contentHeight = RULER_HEIGHT + _blueprint.Tracks.Count * TRACK_HEIGHT + 20;

            _timelineScroll = GUI.BeginScrollView(
                new Rect(0, startY, totalWidth, totalHeight),
                _timelineScroll,
                new Rect(0, 0, contentWidth, Mathf.Max(contentHeight, totalHeight)));

            DrawRuler();

            for (int i = 0; i < _blueprint.Tracks.Count; i++)
            {
                DrawTrack(i, _blueprint.Tracks[i]);
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
            Rect inspectorRect = new Rect(position.width - INSPECTOR_WIDTH, TOOLBAR_HEIGHT, INSPECTOR_WIDTH, position.height - TOOLBAR_HEIGHT);
            EditorGUI.DrawRect(inspectorRect, new Color(0.2f, 0.2f, 0.2f));

            GUILayout.BeginArea(new Rect(inspectorRect.x + 10, inspectorRect.y + 10, inspectorRect.width - 20, inspectorRect.height - 20));

            GUILayout.Label("📋 Clip Inspector", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (_selectedClip == null)
            {
                GUILayout.Label("No clip selected. Click a clip on the timeline.", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                string newLabel = EditorGUILayout.TextField("Name", _selectedClip.Label);
                float newStart = EditorGUILayout.FloatField("Start Time", _selectedClip.TimeStart);
                float newDur = EditorGUILayout.FloatField("Duration", _selectedClip.Duration);
                Color newColor = EditorGUILayout.ColorField("Color", _selectedClip.ClipColor);
                
                if (EditorGUI.EndChangeCheck())
                {
                    RecordUndo();
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
                            changedPath = EditorGUILayout.TextField("Floorplan JSON", _selectedClip.FloorplanJsonPath);
                            if (EditorGUI.EndChangeCheck()) { RecordUndo(); _selectedClip.FloorplanJsonPath = changedPath; }
                            GUILayout.Space(5);
                            if (GUILayout.Button("🖌️ Open Floorplanner", GUILayout.Height(30))) OpenClipEditor(_selectedClip);
                            break;

                        case CompositionTrackType.Scenario:
                            changedPath = EditorGUILayout.TextField("Episode Graph", _selectedClip.EpisodeGraphPath);
                            if (EditorGUI.EndChangeCheck()) { RecordUndo(); _selectedClip.EpisodeGraphPath = changedPath; }
                            GUILayout.Space(5);
                            if (GUILayout.Button("🕸️ Open Node Editor", GUILayout.Height(30))) OpenClipEditor(_selectedClip);
                            break;

                        case CompositionTrackType.Camera:
                            changedPath = EditorGUILayout.TextField("Preset Key", _selectedClip.CameraPresetKey);
                            if (EditorGUI.EndChangeCheck()) { RecordUndo(); _selectedClip.CameraPresetKey = changedPath; }
                            GUILayout.Space(5);
                            if (GUILayout.Button("⚔️ Edit Camera Settings", GUILayout.Height(30))) OpenClipEditor(_selectedClip);
                            break;

                        case CompositionTrackType.Factory:
                            changedPath = EditorGUILayout.TextField("Menu Command", _selectedClip.FactoryMenuCommand);
                            if (EditorGUI.EndChangeCheck()) { RecordUndo(); _selectedClip.FactoryMenuCommand = changedPath; }
                            GUILayout.Space(5);
                            if (GUILayout.Button("🏭 Run Factory Command", GUILayout.Height(30))) OpenClipEditor(_selectedClip);
                            break;

                        case CompositionTrackType.Data:
                            changedPath = EditorGUILayout.TextField("Asset Path", _selectedClip.DataAssetPath);
                            if (EditorGUI.EndChangeCheck()) { RecordUndo(); _selectedClip.DataAssetPath = changedPath; }
                            GUILayout.Space(5);
                            if (GUILayout.Button("📄 Select Data Asset", GUILayout.Height(30))) OpenClipEditor(_selectedClip);
                            break;

                        case CompositionTrackType.TestRunner:
                            changedPath = EditorGUILayout.TextField("Scene Path", _selectedClip.ScenePath);
                            if (EditorGUI.EndChangeCheck()) { RecordUndo(); _selectedClip.ScenePath = changedPath; }
                            GUILayout.Space(5);
                            if (GUILayout.Button("▶️ Open Scene", GUILayout.Height(30))) OpenClipEditor(_selectedClip);
                            break;
                    }
                }
            }

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
                    RecordUndo();
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
                    RecordUndo();
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
                RecordUndo();
                foreach (var track in _blueprint.Tracks)
                {
                    track.Clips.Remove(_selectedClip);
                }
                _selectedClip = null;
                e.Use();
            }
            else if (e.type == EventType.KeyDown && e.control && e.shift && e.keyCode == KeyCode.Z)
            {
                PerformRedo();
                e.Use();
            }
            else if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.Z)
            {
                PerformUndo();
                e.Use();
            }
            else if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.S)
            {
                SaveBlueprint();
                e.Use();
            }
        }

        private CompositionClip FindClipAtPosition(Vector2 pos)
        {
            for (int i = 0; i < _blueprint.Tracks.Count; i++)
            {
                float trackY = RULER_HEIGHT + i * TRACK_HEIGHT;
                if (pos.y < trackY || pos.y > trackY + TRACK_HEIGHT) continue;

                foreach (var clip in _blueprint.Tracks[i].Clips)
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
            for (int i = 0; i < _blueprint.Tracks.Count; i++)
            {
                float trackY = RULER_HEIGHT + i * TRACK_HEIGHT;
                if (pos.y < trackY || pos.y > trackY + TRACK_HEIGHT) continue;

                foreach (var clip in _blueprint.Tracks[i].Clips)
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
            RecordUndo();
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
            menu.AddItem(new GUIContent("🏗️ Environment"), false, () => { RecordUndo(); _blueprint.Tracks.Add(new CompositionTrack { TrackName = "🏗️ 환경", TrackType = CompositionTrackType.Environment, TrackColor = new Color(0.2f, 0.7f, 0.3f) }); });
            menu.AddItem(new GUIContent("🕸️ Scenario"), false, () => { RecordUndo(); _blueprint.Tracks.Add(new CompositionTrack { TrackName = "🕸️ 시나리오", TrackType = CompositionTrackType.Scenario, TrackColor = new Color(0.3f, 0.5f, 0.9f) }); });
            menu.AddItem(new GUIContent("⚔️ Camera/Direction"), false, () => { RecordUndo(); _blueprint.Tracks.Add(new CompositionTrack { TrackName = "⚔️ 카메라/연출", TrackType = CompositionTrackType.Camera, TrackColor = new Color(0.9f, 0.4f, 0.3f) }); });
            menu.AddItem(new GUIContent("🏭 Factory"), false, () => { RecordUndo(); _blueprint.Tracks.Add(new CompositionTrack { TrackName = "🏭 공장", TrackType = CompositionTrackType.Factory, TrackColor = new Color(0.8f, 0.6f, 0.2f) }); });
            menu.AddItem(new GUIContent("📄 Data"), false, () => { RecordUndo(); _blueprint.Tracks.Add(new CompositionTrack { TrackName = "📄 데이터", TrackType = CompositionTrackType.Data, TrackColor = new Color(0.6f, 0.4f, 0.8f) }); });
            menu.AddItem(new GUIContent("▶️ Test Runner"), false, () => { RecordUndo(); _blueprint.Tracks.Add(new CompositionTrack { TrackName = "▶️ 러너", TrackType = CompositionTrackType.TestRunner, TrackColor = new Color(0.4f, 0.8f, 0.8f) }); });
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
                renameWnd.Init(clip, () => RecordUndo());
                renameWnd.ShowAsDropDown(new Rect(Event.current.mousePosition + position.position, Vector2.zero), new Vector2(250, 50));
            });
            menu.AddSeparator("");

            if (track != null && track.TrackType == CompositionTrackType.Factory)
            {
                string[] pbMenuItems = new string[] {
                    "Tools/ProBuilder/Create Algorithm Statistics Room",
                    "Tools/ProBuilder/Create Cartography Observatory",
                    "Tools/ProBuilder/Create Central Broadcasting Studio",
                    "Tools/ProBuilder/Create Chronology Archives",
                    "Tools/ProBuilder/Create Geometry Precision Classroom",
                    "Tools/ProBuilder/Create Grand Gothic Academy Library",
                    "Tools/ProBuilder/Create Language Lab",
                    "Tools/ProBuilder/Create Language Proofreading Lab",
                    "Tools/ProBuilder/Create Large Multi-Level Arena",
                    "Tools/ProBuilder/Create Molecular Cultivation Room",
                    "Tools/ProBuilder/Create Moot Court Debate Room",
                    "Tools/ProBuilder/Create Syntax Cloister",
                    "Tools/ProBuilder/Create L-Terrace with Stairs",
                    "Tools/ProBuilder/Create Vacuum Dynamics Lab"
                };

                foreach (var menuPath in pbMenuItems)
                {
                    string shortName = menuPath.Replace("Tools/ProBuilder/Create ", "");
                    menu.AddItem(new GUIContent($"Assign Factory/{shortName}"), false, () =>
                    {
                        RecordUndo();
                        clip.FactoryMenuCommand = menuPath;
                        clip.Label = shortName;
                    });
                }

                menu.AddSeparator("Assign Factory/");
                menu.AddItem(new GUIContent("Assign Factory/🎨 Battle & Phone UI"), false, () => { RecordUndo(); clip.FactoryMenuCommand = "Tools/UI Toolkit/Create Phone UI Document"; clip.Label = "Battle & Phone UI"; });
                menu.AddItem(new GUIContent("Assign Factory/🎨 Math Proof UI"), false, () => { RecordUndo(); clip.FactoryMenuCommand = "Tools/UI Toolkit/Create Math Proof UI Document"; clip.Label = "Math Proof UI"; });
                menu.AddItem(new GUIContent("Assign Factory/⚙️ Field Triggers"), false, () => { RecordUndo(); clip.FactoryMenuCommand = "Tools/Setup Field Trigger and Math Gimmick Scene Objects"; clip.Label = "Field Triggers"; });
                menu.AddItem(new GUIContent("Assign Factory/🔤 KoPubFont"), false, () => { RecordUndo(); clip.FactoryMenuCommand = "Tools/Generate Fresh KoPubFont Asset"; clip.Label = "KoPubFont"; });
            }

            if (track != null && track.TrackType == CompositionTrackType.Camera)
            {
                menu.AddItem(new GUIContent("Edit Camera Settings..."), false, () => ShowCameraSettingsPopup(clip));
            }

            if (track != null && track.TrackType == CompositionTrackType.Data)
            {
                menu.AddItem(new GUIContent("Import Math Data JSON..."), false, () => ImportMathData(clip));
                menu.AddItem(new GUIContent("Open Anomaly Importer..."), false, () => AnomalyImporterWindow.ShowWindow());
            }

            if (track != null && track.TrackType == CompositionTrackType.TestRunner)
            {
                menu.AddItem(new GUIContent("Assign/PlayTest Scene"), false, () => { RecordUndo(); clip.ScenePath = "Assets/Scenes/VerificationTestScene.unity"; clip.Label = "PlayTest Scene"; });
                menu.AddItem(new GUIContent("Assign/Sandbox Scene"), false, () => { RecordUndo(); clip.ScenePath = "Assets/Scenes/Scene_ExplorationRunner.unity"; clip.Label = "Sandbox Scene"; });
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Delete Clip"), false, () =>
            {
                RecordUndo();
                if (track != null) track.Clips.Remove(clip);
                if (_selectedClip == clip) _selectedClip = null;
            });

            menu.ShowAsContext();
        }

        private CompositionTrack FindTrackForClip(CompositionClip clip)
        {
            foreach (var track in _blueprint.Tracks)
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
                    FloorplannerWindow.Open(clip.FloorplanJsonPath);
                    break;

                case CompositionTrackType.Scenario:
                    WorkspaceNodeWindow.Open(clip.EpisodeGraphPath);
                    break;

                case CompositionTrackType.Camera:
                    ShowCameraSettingsPopup(clip);
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
                        ShowClipContextMenu(clip);
                    }
                    break;

                case CompositionTrackType.TestRunner:
                    if (!string.IsNullOrEmpty(clip.ScenePath))
                    {
                        if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(clip.ScenePath);
                    }
                    break;
            }
        }

        private void ShowCameraSettingsPopup(CompositionClip clip)
        {
            if (!_cameraPrefsLoaded) LoadCameraPrefs();

            var popup = ScriptableObject.CreateInstance<CameraSettingsPopup>();
            popup.Init(clip, this);
            popup.ShowUtility();
        }

        private void LoadCameraPrefs()
        {
            _hubBackOffset = new Vector3(EditorPrefs.GetFloat("SG_BackOffsetX", 0f), EditorPrefs.GetFloat("SG_BackOffsetY", 2.5f), EditorPrefs.GetFloat("SG_BackOffsetZ", -5f));
            _hubBackPitch = EditorPrefs.GetFloat("SG_BackPitch", 15f);
            _hubBackYaw = EditorPrefs.GetFloat("SG_BackYaw", 0f);
            _hubQuarterOffset = new Vector3(EditorPrefs.GetFloat("SG_QuarterOffsetX", 0f), EditorPrefs.GetFloat("SG_QuarterOffsetY", 7f), EditorPrefs.GetFloat("SG_QuarterOffsetZ", -6f));
            _hubQuarterPitch = EditorPrefs.GetFloat("SG_QuarterPitch", 45f);
            _hubQuarterYaw = EditorPrefs.GetFloat("SG_QuarterYaw", 0f);
            _cameraPrefsLoaded = true;
        }

        public void SaveCameraPrefs()
        {
            EditorPrefs.SetFloat("SG_BackOffsetX", _hubBackOffset.x);
            EditorPrefs.SetFloat("SG_BackOffsetY", _hubBackOffset.y);
            EditorPrefs.SetFloat("SG_BackOffsetZ", _hubBackOffset.z);
            EditorPrefs.SetFloat("SG_BackPitch", _hubBackPitch);
            EditorPrefs.SetFloat("SG_BackYaw", _hubBackYaw);
            EditorPrefs.SetFloat("SG_QuarterOffsetX", _hubQuarterOffset.x);
            EditorPrefs.SetFloat("SG_QuarterOffsetY", _hubQuarterOffset.y);
            EditorPrefs.SetFloat("SG_QuarterOffsetZ", _hubQuarterOffset.z);
            EditorPrefs.SetFloat("SG_QuarterPitch", _hubQuarterPitch);
            EditorPrefs.SetFloat("SG_QuarterYaw", _hubQuarterYaw);
        }

        public Vector3 HubBackOffset { get => _hubBackOffset; set { _hubBackOffset = value; SaveCameraPrefs(); } }
        public float HubBackPitch { get => _hubBackPitch; set { _hubBackPitch = value; SaveCameraPrefs(); } }
        public float HubBackYaw { get => _hubBackYaw; set { _hubBackYaw = value; SaveCameraPrefs(); } }
        public Vector3 HubQuarterOffset { get => _hubQuarterOffset; set { _hubQuarterOffset = value; SaveCameraPrefs(); } }
        public float HubQuarterPitch { get => _hubQuarterPitch; set { _hubQuarterPitch = value; SaveCameraPrefs(); } }
        public float HubQuarterYaw { get => _hubQuarterYaw; set { _hubQuarterYaw = value; SaveCameraPrefs(); } }

        private void ImportMathData(CompositionClip clip)
        {
            string path = EditorUtility.OpenFilePanel("Select math_world_hierarchy.json", "", "json");
            if (string.IsNullOrEmpty(path)) return;

            string jsonText = System.IO.File.ReadAllText(path);
            MathDataDto dto = Newtonsoft.Json.JsonConvert.DeserializeObject<MathDataDto>(jsonText);

            MathData asset = ScriptableObject.CreateInstance<MathData>();
            asset.systemName = dto.system_name;
            asset.version = dto.version;
            asset.totalWorlds = dto.total_worlds;
            asset.totalEntities = dto.total_entities;

            foreach (var w in dto.worlds)
            {
                WorldData worldData = new WorldData { id = w.id, title = w.title, description = w.description };

                foreach (var b in w.biomes)
                {
                    BiomeData biomeData = new BiomeData { id = b.id, title = b.title };

                    foreach (var l in b.laws)
                    {
                        biomeData.laws.Add(new LawData
                        {
                            id = l.id, law_type = l.law_type, name = l.name, formula_pattern = l.formula_pattern,
                            interactive_rules = l.interactive_rules != null ? new InteractiveRulesData
                            {
                                @operator = l.interactive_rules.@operator,
                                target_property = l.interactive_rules.target_property,
                                input_parameter = l.interactive_rules.input_parameter,
                                action_type = l.interactive_rules.action_type
                            } : null
                        });
                    }

                    foreach (var e in b.entities)
                    {
                        biomeData.entities.Add(new EntityData
                        {
                            id = e.id, law_id = e.law_id, question_text = e.question_text, formula = e.formula,
                            choices = e.choices != null ? new List<string>(e.choices) : new List<string>(),
                            points = e.points, has_image = e.has_image, image_asset_id = e.image_asset_id,
                            render_type = e.render_type,
                            interaction_node = e.interaction_node != null ? new InteractionNodeData
                            {
                                node_type = e.interaction_node.node_type,
                                trigger_event = e.interaction_node.trigger_event,
                                required_solution = e.interaction_node.required_solution
                            } : null
                        });
                    }
                    worldData.biomes.Add(biomeData);
                }
                asset.worlds.Add(worldData);
            }

            string savePath = "Assets/Resources/MathData.asset";
            string dir = System.IO.Path.GetDirectoryName(savePath);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            AssetDatabase.CreateAsset(asset, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            RecordUndo();
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
            string json = JsonConvert.SerializeObject(_blueprint, Formatting.Indented);
            System.IO.File.WriteAllText(_currentBlueprintPath, json);
            AssetDatabase.Refresh();
            Debug.Log($"[Director Hub] Stage Blueprint saved to {_currentBlueprintPath}");
        }

        private void LoadBlueprint()
        {
            string path = EditorUtility.OpenFilePanel("Load Stage Blueprint", "Assets", "json");
            if (string.IsNullOrEmpty(path)) return;

            string json = System.IO.File.ReadAllText(path);
            var loaded = JsonConvert.DeserializeObject<StageBlueprintData>(json);
            if (loaded != null)
            {
                _blueprint = loaded;
                _currentBlueprintPath = path;
                _selectedClip = null;
                Debug.Log($"[Director Hub] Stage Blueprint loaded from {path}");
            }
        }

        private void BootstrapSandboxScene()
        {
            if (EditorApplication.isPlaying) return;

            if (!UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string blueprintJson = JsonConvert.SerializeObject(_blueprint);
            EditorPrefs.SetString("StudyGame_StageBlueprintJson", blueprintJson);
            EditorPrefs.SetBool("StudyGame_SandboxPending", true);

            foreach (var track in _blueprint.Tracks)
            {
                if (track.TrackType == CompositionTrackType.Environment)
                {
                    foreach (var clip in track.Clips)
                    {
                        if (!string.IsNullOrEmpty(clip.FloorplanJsonPath) && System.IO.File.Exists(clip.FloorplanJsonPath))
                        {
                            string geomJson = System.IO.File.ReadAllText(clip.FloorplanJsonPath);
                            EditorPrefs.SetString("StudyGame_SandboxGeometry", geomJson);
                            break;
                        }
                    }
                }

                if (track.TrackType == CompositionTrackType.Factory)
                {
                    foreach (var clip in track.Clips)
                    {
                        if (!string.IsNullOrEmpty(clip.FactoryMenuCommand))
                        {
                            EditorApplication.ExecuteMenuItem(clip.FactoryMenuCommand);
                        }
                    }
                }
            }

            string scenePath = "Assets/Scenes/VerificationTestScene.unity";
            if (System.IO.File.Exists(scenePath))
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            else
            {
                var newScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                    UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                    UnityEditor.SceneManagement.NewSceneMode.Single);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(newScene, scenePath);
            }

            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            EditorApplication.EnterPlaymode();
        }
    }

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

    public class CameraSettingsPopup : EditorWindow
    {
        private CompositionClip _clip;
        private DirectorEditorWindow _director;

        public void Init(CompositionClip clip, DirectorEditorWindow director)
        {
            _clip = clip;
            _director = director;
            titleContent = new GUIContent("⚔️ Camera Settings");
            minSize = new Vector2(400, 350);
        }

        private void OnGUI()
        {
            GUILayout.Label("🎥 Camera Settings (EditorPrefs Persist)", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("1. BackView", EditorStyles.miniBoldLabel);
            _director.HubBackOffset = EditorGUILayout.Vector3Field("Offset", _director.HubBackOffset);
            _director.HubBackPitch = EditorGUILayout.Slider("Pitch", _director.HubBackPitch, -90f, 90f);
            _director.HubBackYaw = EditorGUILayout.Slider("Yaw", _director.HubBackYaw, -180f, 180f);
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("2. QuarterView", EditorStyles.miniBoldLabel);
            _director.HubQuarterOffset = EditorGUILayout.Vector3Field("Offset", _director.HubQuarterOffset);
            _director.HubQuarterPitch = EditorGUILayout.Slider("Pitch", _director.HubQuarterPitch, -90f, 90f);
            _director.HubQuarterYaw = EditorGUILayout.Slider("Yaw", _director.HubQuarterYaw, -180f, 180f);
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                _clip.CameraPresetKey = $"Back({_director.HubBackPitch:F0}°) Quarter({_director.HubQuarterPitch:F0}°)";
                _clip.Label = _clip.CameraPresetKey;

                var camManager = UnityEngine.Object.FindFirstObjectByType<StudyGame.Runtime.Combat.CameraViewManager>();
                if (camManager != null)
                {
                    camManager.backViewOffset = _director.HubBackOffset;
                    camManager.backViewPitch = _director.HubBackPitch;
                    camManager.backViewYaw = _director.HubBackYaw;
                    camManager.quarterViewOffset = _director.HubQuarterOffset;
                    camManager.quarterViewPitch = _director.HubQuarterPitch;
                    camManager.quarterViewYaw = _director.HubQuarterYaw;
                }
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Close", GUILayout.Height(30)))
            {
                Close();
            }
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

                    var player = UnityEngine.Object.FindFirstObjectByType<StudyGame.Player.PlayerController>(FindObjectsInactive.Include);
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

    [System.Serializable]
    public class MathDataDto
    {
        public string system_name;
        public string version;
        public int total_worlds;
        public int total_entities;
        public List<WorldDto> worlds;
    }

    [System.Serializable]
    public class WorldDto
    {
        public string id;
        public string title;
        public string description;
        public List<BiomeDto> biomes;
    }

    [System.Serializable]
    public class BiomeDto
    {
        public string id;
        public string title;
        public List<LawDto> laws;
        public List<EntityDto> entities;
    }

    [System.Serializable]
    public class LawDto
    {
        public string id;
        public string law_type;
        public string name;
        public string formula_pattern;
        public InteractiveRulesDto interactive_rules;
    }

    [System.Serializable]
    public class InteractiveRulesDto
    {
        public string @operator;
        public string target_property;
        public string input_parameter;
        public string action_type;
    }

    [System.Serializable]
    public class EntityDto
    {
        public string id;
        public string law_id;
        public string question_text;
        public string formula;
        public string[] choices;
        public int points;
        public bool has_image;
        public string image_asset_id;
        public string render_type;
        public InteractionNodeDto interaction_node;
    }

    [System.Serializable]
    public class InteractionNodeDto
    {
        public string node_type;
        public string trigger_event;
        public string required_solution;
    }
}
