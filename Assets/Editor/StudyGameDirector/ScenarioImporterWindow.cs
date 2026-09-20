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
        private string outputDirectory = "Assets/Resources/Scenarios/Ep1";

        [MenuItem("StudyGame/Scenario Importer")]
        public static void ShowWindow()
        {
            GetWindow<ScenarioImporterWindow>("Scenario Importer");
        }

        [MenuItem("StudyGame/Bake All Scenarios (Auto)")]
        public static void BakeAllScenarios()
        {
            string sourceDir = "Assets/Resources/Scenarios/Sources";
            
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
                string outDir = $"Assets/Resources/Scenarios/{epName}";
                GenerateNodes(text, outDir, true);
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
                    GenerateNodes(markdownFile.text, outputDirectory);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please assign a markdown file.", "OK");
                }
            }
        }

        public static void GenerateNodes(string markdown, string outDir, bool silent = false)
        {
            // Ensure directory exists in the physical file system
            string fullPath = Path.Combine(Application.dataPath, outDir.Replace("Assets/", ""));
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }

            // Match all phases starting with '## [Phase' until the next '## [Phase' or end of string
            MatchCollection phaseMatches = Regex.Matches(markdown, @"## \[Phase(.*?)(?=(?:## \[Phase)|\z)", RegexOptions.Singleline);
            
            int phaseIndex = 0;
            foreach (Match match in phaseMatches)
            {
                string fullPhase = match.Value.Trim();
                ParsePhaseToNode(fullPhase, outDir, phaseIndex);
                phaseIndex++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            if (!silent)
            {
                EditorUtility.DisplayDialog("Success", $"Successfully generated {phaseIndex} nodes in {outDir}", "OK");
            }
        }

        private static void ParsePhaseToNode(string phaseContent, string outDir, int index)
        {
            // 1. Extract Title
            string titleLine = phaseContent.Substring(0, phaseContent.IndexOf('\n')).Trim();
            string title = titleLine.Replace("## ", "");

            WorkspaceNodeData nodeData = ScriptableObject.CreateInstance<WorkspaceNodeData>();
            nodeData.NodeId = System.Guid.NewGuid().ToString();
            nodeData.NodeTitle = title;
            nodeData.TemplateType = "일반";

            // 2. Parse Dialogues (- **Speaker**: "Text")
            Regex dialogRegex = new Regex(@"- \*\*(.*?)\*\*: ""(.*?)""");
            MatchCollection dialogMatches = dialogRegex.Matches(phaseContent);

            if (dialogMatches.Count > 0)
            {
                nodeData.TemplateType = "다이얼로그";
                DynamicProperty dialogProp = new DynamicProperty
                {
                    PropertyName = "Dialogues",
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
                nodeData.Properties.Add(dialogProp);
            }

            // 3. Parse SNS Feeds (- 💬 **@User**: "Text")
            Regex snsRegex = new Regex(@"- (?:📱|💬|📢|📺) \*\*(.*?)\*\*: ""(.*?)""");
            MatchCollection snsMatches = snsRegex.Matches(phaseContent);
            if (snsMatches.Count > 0)
            {
                // If it already has dialogues, we keep it as general or composite, but let's override to SNS if SNS exists.
                nodeData.TemplateType = "SNS";
                DynamicProperty snsProp = new DynamicProperty
                {
                    PropertyName = "SNS Feed",
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
                nodeData.Properties.Add(snsProp);
            }

            // 4. Save Asset
            // Sanitize filename
            string safeTitle = string.Join("_", title.Split(Path.GetInvalidFileNameChars()));
            safeTitle = safeTitle.Replace("[", "").Replace("]", "").Replace(" ", "_");
            
            string assetPath = $"{outDir}/Node_{index}_{safeTitle}.asset";
            
            StudyGame.Editor.Utils.AssetHelper.CreateOrOverwriteAsset(nodeData, assetPath);
        }
    }
}
