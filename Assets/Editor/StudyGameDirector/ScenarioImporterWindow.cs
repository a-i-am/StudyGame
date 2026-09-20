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

        private void GenerateNodes(string markdown, string outDir)
        {
            // Ensure directory exists in the physical file system
            string fullPath = Path.Combine(Application.dataPath, outDir.Replace("Assets/", ""));
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }

            // Split markdown by Phase
            string[] phases = markdown.Split(new string[] { "## [Phase" }, System.StringSplitOptions.RemoveEmptyEntries);
            
            int phaseIndex = 0;
            foreach (string phaseContent in phases)
            {
                if (!phaseContent.Contains("]")) continue;
                
                string fullPhase = "## [Phase" + phaseContent;
                ParsePhaseToNode(fullPhase, outDir, phaseIndex);
                phaseIndex++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Successfully generated {phaseIndex} nodes in {outDir}", "OK");
        }

        private void ParsePhaseToNode(string phaseContent, string outDir, int index)
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
            
            // Delete existing if any
            if (AssetDatabase.LoadAssetAtPath<WorkspaceNodeData>(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            AssetDatabase.CreateAsset(nodeData, assetPath);
        }
    }
}
