using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace StudyGame.Editor
{
    [Serializable]
    public class EpisodeData
    {
        public string episodeName = "새 에피소드";
        public string rawSynopsis = "";
        
        public string artAssetRequirements = "[배경/환경 에셋 (Environment)]\n-\n\n[인물/몬스터 에셋 (Character)]\n-\n\n[UI/아이콘 에셋 (UI)]\n-\n\n[사운드/VFX 에셋 (Audio/VFX)]\n-\n";
        
        public string generatedSpecMd = "";
        public string yarnContent = "";
    }

    [Serializable]
    public class ChapterData
    {
        public string chapterName = "새 챕터";
        public string chapterYarnSummary = "";
        public List<EpisodeData> episodes = new List<EpisodeData>();
    }

    [Serializable]
    public class ScenarioDeskSaveData
    {
        public string exportPath = "Assets/GameFlowDocs/";
        public List<ChapterData> chapters = new List<ChapterData>();
    }

    public class ScenarioDeskWindow : EditorWindow
    {
        private ScenarioDeskSaveData saveData;

        // UI 상태 변수
        private int selectedChapterIndex = 0;
        private int selectedEpisodeIndex = -1;
        private Vector2 sidebarScroll;
        private Vector2 mainScroll;

        private const string SaveFilePath = "Library/ScenarioDeskSession.json";

        [MenuItem("StudyGame/Scenario Desk (스토리 통합 관리)")]
        public static void Open()
        {
            var wnd = GetWindow<ScenarioDeskWindow>("Scenario Desk");
            wnd.minSize = new Vector2(900, 700);
        }

        private void OnEnable()
        {
            LoadData();
        }

        private void OnDisable()
        {
            SaveData();
        }

        private void LoadData()
        {
            if (File.Exists(SaveFilePath))
            {
                string json = File.ReadAllText(SaveFilePath);
                saveData = JsonUtility.FromJson<ScenarioDeskSaveData>(json);
            }

            if (saveData == null)
            {
                saveData = new ScenarioDeskSaveData();
                saveData.chapters.Add(new ChapterData { chapterName = "챕터 1" });
            }
        }

        private void SaveData()
        {
            if (saveData == null) return;
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SaveFilePath, json);
        }

        private void OnGUI()
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.S && (e.control || e.command))
            {
                SaveData();
                e.Use();
            }

            DrawTopToolbar();
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical("box", GUILayout.Width(250));
            DrawSidebar();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            DrawMainWorkspace();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (GUI.changed) SaveData();
        }

        private void DrawTopToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("📁 데이터 저장 경로: ");
            saveData.exportPath = GUILayout.TextField(saveData.exportPath, GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("💾 수동 저장", EditorStyles.toolbarButton, GUILayout.Width(80))) SaveData();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSidebar()
        {
            sidebarScroll = EditorGUILayout.BeginScrollView(sidebarScroll);

            for (int c = 0; c < saveData.chapters.Count; c++)
            {
                var chapter = saveData.chapters[c];

                EditorGUILayout.BeginHorizontal();
                bool isChapterSelected = (selectedChapterIndex == c && selectedEpisodeIndex == -1);
                GUI.backgroundColor = isChapterSelected ? Color.cyan : Color.white;

                if (GUILayout.Button(chapter.chapterName, EditorStyles.boldLabel))
                {
                    selectedChapterIndex = c;
                    selectedEpisodeIndex = -1;
                    GUI.FocusControl(null);
                }
                GUI.backgroundColor = Color.white;

                if (GUILayout.Button("+", GUILayout.Width(25)))
                {
                    chapter.episodes.Add(new EpisodeData { episodeName = $"에피소드 {chapter.episodes.Count + 1}" });
                }
                if (GUILayout.Button("-", GUILayout.Width(25)) && saveData.chapters.Count > 1)
                {
                    saveData.chapters.RemoveAt(c);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                for (int ep = 0; ep < chapter.episodes.Count; ep++)
                {
                    EditorGUILayout.BeginHorizontal();
                    bool isEpSelected = (selectedChapterIndex == c && selectedEpisodeIndex == ep);
                    GUI.backgroundColor = isEpSelected ? Color.green : Color.white;

                    if (GUILayout.Button(chapter.episodes[ep].episodeName, EditorStyles.label))
                    {
                        selectedChapterIndex = c;
                        selectedEpisodeIndex = ep;
                        GUI.FocusControl(null);
                    }
                    GUI.backgroundColor = Color.white;

                    if (GUILayout.Button("x", GUILayout.Width(20)))
                    {
                        chapter.episodes.RemoveAt(ep);
                        if (selectedEpisodeIndex == ep) selectedEpisodeIndex = -1;
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.Space(10);
            if (GUILayout.Button("새 챕터 추가"))
            {
                saveData.chapters.Add(new ChapterData { chapterName = $"챕터 {saveData.chapters.Count + 1}" });
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawMainWorkspace()
        {
            if (saveData.chapters.Count == 0 || selectedChapterIndex >= saveData.chapters.Count) return;

            mainScroll = EditorGUILayout.BeginScrollView(mainScroll);
            var activeChapter = saveData.chapters[selectedChapterIndex];

            if (selectedEpisodeIndex == -1)
            {
                EditorGUILayout.LabelField("챕터 설정", EditorStyles.boldLabel);
                activeChapter.chapterName = EditorGUILayout.TextField("챕터 이름", activeChapter.chapterName);

                EditorGUILayout.Space(15);
                EditorGUILayout.LabelField("챕터 단위 .yarn 흐름 요약", EditorStyles.boldLabel);
                activeChapter.chapterYarnSummary = EditorGUILayout.TextArea(activeChapter.chapterYarnSummary, GUILayout.MinHeight(300));
            }
            else if (selectedEpisodeIndex < activeChapter.episodes.Count)
            {
                var ep = activeChapter.episodes[selectedEpisodeIndex];

                EditorGUILayout.LabelField("에피소드 설정", EditorStyles.boldLabel);
                ep.episodeName = EditorGUILayout.TextField("에피소드 이름", ep.episodeName);
                
                EditorGUILayout.Space(15);
                
                // [오토 필 기능 - 문서 불러오기]
                GUI.backgroundColor = new Color(0.8f, 1f, 0.8f);
                if (GUILayout.Button("🔄 외부 파일에서 최신 문서 불러오기 (Auto-Fill)", GUILayout.Height(30)))
                {
                    ep.generatedSpecMd = LoadTextFile($"{activeChapter.chapterName}_{ep.episodeName}.md", ep.generatedSpecMd);
                    ep.yarnContent = LoadTextFile($"{activeChapter.chapterName}_{ep.episodeName}.yarn", ep.yarnContent);
                    GUI.FocusControl(null); // 포커스 해제하여 텍스트 갱신 반영
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space(10);

                // Step 0
                EditorGUILayout.LabelField("Step 0. 기획 원문 (Synopsis)", EditorStyles.boldLabel);
                ep.rawSynopsis = EditorGUILayout.TextArea(ep.rawSynopsis, GUILayout.MinHeight(100));
                EditorGUILayout.Space(10);

                // Step 1
                EditorGUILayout.LabelField("Step 1. 필요 아트 에셋 리스트 (Art Assets)", EditorStyles.boldLabel);
                ep.artAssetRequirements = EditorGUILayout.TextArea(ep.artAssetRequirements, GUILayout.MinHeight(120));
                EditorGUILayout.Space(10);

                // Step 2
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Step 2. 시스템 명세서 (.md)", EditorStyles.boldLabel);
                if (GUILayout.Button("디스크에 명세서 저장", GUILayout.Width(150)))
                {
                    SaveTextFile(ep.generatedSpecMd, $"{activeChapter.chapterName}_{ep.episodeName}.md");
                }
                EditorGUILayout.EndHorizontal();
                ep.generatedSpecMd = EditorGUILayout.TextArea(ep.generatedSpecMd, GUILayout.MinHeight(150));
                EditorGUILayout.Space(10);

                // Step 3
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Step 3. Yarn Spinner 스크립트 (.yarn)", EditorStyles.boldLabel);
                if (GUILayout.Button("디스크에 Yarn 스크립트 저장", GUILayout.Width(180)))
                {
                    SaveTextFile(ep.yarnContent, $"{activeChapter.chapterName}_{ep.episodeName}.yarn");
                }
                EditorGUILayout.EndHorizontal();
                ep.yarnContent = EditorGUILayout.TextArea(ep.yarnContent, GUILayout.MinHeight(200));
            }

            EditorGUILayout.EndScrollView();
        }

        private void SaveTextFile(string content, string fileName)
        {
            if (string.IsNullOrWhiteSpace(content)) return;
            if (!Directory.Exists(saveData.exportPath)) Directory.CreateDirectory(saveData.exportPath);

            string safeFileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            string fullPath = Path.Combine(saveData.exportPath, safeFileName);

            File.WriteAllText(fullPath, content, Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log($"[Scenario Desk] 파일 저장 완료: {fullPath}");
        }

        private string LoadTextFile(string fileName, string fallback)
        {
            string safeFileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            string fullPath = Path.Combine(saveData.exportPath, safeFileName);

            if (File.Exists(fullPath)) 
            {
                Debug.Log($"[Scenario Desk] 파일 불러오기 성공: {fullPath}");
                return File.ReadAllText(fullPath, Encoding.UTF8);
            }

            Debug.LogWarning($"[Scenario Desk] 파일을 찾을 수 없습니다: {fullPath}");
            return fallback;
        }
    }
}