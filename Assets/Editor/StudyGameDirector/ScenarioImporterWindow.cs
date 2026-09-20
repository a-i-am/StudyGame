using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using StudyGame.Data;
using System.Linq;

namespace StudyGame.Editor.Director
{
    public class ScenarioImporterWindow : EditorWindow
    {
        private TextAsset markdownFile;
        private string outputDirectory = "Assets/Resources/Scenarios/Episodes";

        [MenuItem("StudyGame/Scenario Importer")]
        public static void ShowWindow()
        {
            GetWindow<ScenarioImporterWindow>("Scenario Importer");
        }

        [MenuItem("StudyGame/Bake All Scenarios (Auto)")]
        public static void BakeAllScenarios()
        {
            string sourceDir = "Assets/Resources/Scenarios/Sources";
            string outDir = "Assets/Resources/Scenarios/Episodes";
            
            if (!Directory.Exists(sourceDir))
            {
                Debug.LogError("Source directory not found!");
                return;
            }

            string[] files = Directory.GetFiles(sourceDir, "*.md");
            foreach (var file in files)
            {
                string text = File.ReadAllText(file);
                string filename = Path.GetFileNameWithoutExtension(file);
                string epName = filename.Split('_')[0].ToUpper(); // e.g. EP1
                GenerateEpisodeNode(text, outDir, epName, true);
            }
            Debug.Log($"Baked {files.Length} scenarios successfully.");
        }

        private void OnGUI()
        {
            GUILayout.Label("Markdown to Node Parser", EditorStyles.boldLabel);
            GUILayout.Space(10);

            markdownFile = (TextAsset)EditorGUILayout.ObjectField("Markdown File", markdownFile, typeof(TextAsset), false);
            outputDirectory = EditorGUILayout.TextField("Output Directory (Asset Path)", outputDirectory);

            GUILayout.Space(20);
            if (GUILayout.Button("Generate Node Assets", GUILayout.Height(30)))
            {
                if (markdownFile != null)
                {
                    GenerateEpisodeNode(markdownFile.text, outputDirectory, markdownFile.name.Split('_')[0].ToUpper());
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please assign a markdown file.", "OK");
                }
            }
        }

        public static void GenerateEpisodeNode(string markdown, string outDir, string epName, bool silent = false)
        {
            // Ensure directory exists in the physical file system
            string fullPath = Path.Combine(Application.dataPath, outDir.Replace("Assets/", ""));
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }

            WorkspaceNodeData episodeNode = ScriptableObject.CreateInstance<WorkspaceNodeData>();
            episodeNode.NodeId = System.Guid.NewGuid().ToString();
            episodeNode.NodeTitle = epName;
            episodeNode.TemplateType = "에피소드";

            // Match all phases starting with '## [Phase' until the next '## [Phase' or end of string
            MatchCollection phaseMatches = Regex.Matches(markdown, @"## \[Phase(.*?)(?=(?:## \[Phase)|\z)", RegexOptions.Singleline);
            
            int phaseIndex = 0;
            foreach (Match match in phaseMatches)
            {
                string fullPhase = match.Value.Trim();
                ParsePhaseToProperties(fullPhase, episodeNode, phaseIndex);
                phaseIndex++;
            }

            string assetPath = $"{outDir}/{epName}.asset";
            StudyGame.Editor.Utils.AssetHelper.CreateOrOverwriteAsset(episodeNode, assetPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            if (!silent)
            {
                EditorUtility.DisplayDialog("Success", $"Successfully generated {epName} node in {outDir}", "OK");
            }
        }

        private static void ParsePhaseToProperties(string phaseContent, WorkspaceNodeData episodeNode, int index)
        {
            // 1. Extract Title
            string titleLine = phaseContent.Substring(0, phaseContent.IndexOf('\n')).Trim();
            string title = titleLine.Replace("## ", "");

            // 2. Parse Dialogues (- **Speaker**: "Text")
            Regex dialogRegex = new Regex(@"- \*\*(.*?)\*\*: ""(.*?)""");
            MatchCollection dialogMatches = dialogRegex.Matches(phaseContent);

            if (dialogMatches.Count > 0)
            {
                DynamicProperty dialogProp = new DynamicProperty
                {
                    PropertyName = $"{title} (Dialogues)",
                    Type = PropertyType.Table,
                    TableColumns = new List<string> { "화자", "대사" }
                };

                foreach (Match m in dialogMatches)
                {
                    TableRowData row = new TableRowData();
                    row.Cells.Add(m.Groups[1].Value);
                    row.Cells.Add(m.Groups[2].Value);
                    dialogProp.TableRows.Add(row);
                }
                episodeNode.Properties.Add(dialogProp);
            }

            // 3. Parse SNS Feeds (- 💬 **@User**: "Text")
            Regex snsRegex = new Regex(@"- (?:📱|💬|📢|📺) \*\*(.*?)\*\*: ""(.*?)""");
            MatchCollection snsMatches = snsRegex.Matches(phaseContent);
            if (snsMatches.Count > 0)
            {
                DynamicProperty snsProp = new DynamicProperty
                {
                    PropertyName = $"{title} (SNS)",
                    Type = PropertyType.Table,
                    TableColumns = new List<string> { "계정명", "내용" }
                };

                foreach (Match m in snsMatches)
                {
                    TableRowData row = new TableRowData();
                    row.Cells.Add(m.Groups[1].Value);
                    row.Cells.Add(m.Groups[2].Value);
                    snsProp.TableRows.Add(row);
                }
                episodeNode.Properties.Add(snsProp);
            }
            
            // 4. 일반 텍스트 이벤트 처리
            if (dialogMatches.Count == 0 && snsMatches.Count == 0)
            {
                DynamicProperty textProp = new DynamicProperty
                {
                    PropertyName = title,
                    Type = PropertyType.Text,
                    StringValue = "이벤트 상호작용 및 지문 처리",
                    ShowAsBadge = true
                };
                episodeNode.Properties.Add(textProp);
            }
        }
    }
}
