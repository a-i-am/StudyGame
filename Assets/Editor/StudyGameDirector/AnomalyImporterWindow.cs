using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using StudyGame.Data;
using Newtonsoft.Json.Linq; // JSON 파싱을 위해 Newtonsoft 활용

namespace StudyGame.Editor.Director
{
    public class AnomalyImporterWindow : EditorWindow
    {
        private string sourceDataDirectory = @"C:\Users\sien0\Desktop\StudyGame\스터디게임 데이터\3_ParsedData\Anomaly";
        private string outputAssetDirectory = "Assets/Resources/Scenarios/Anomalies";

        [MenuItem("StudyGame/Anomaly JSON Importer")]
        public static void ShowWindow()
        {
            GetWindow<AnomalyImporterWindow>("Anomaly Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Anomaly JSON to Workspace Node Parser", EditorStyles.boldLabel);
            GUILayout.Space(10);

            sourceDataDirectory = EditorGUILayout.TextField("Source JSON Dir", sourceDataDirectory);
            outputAssetDirectory = EditorGUILayout.TextField("Output Asset Dir", outputAssetDirectory);

            GUILayout.Space(20);
            if (GUILayout.Button("Bake All Anomalies (Auto)", GUILayout.Height(30)))
            {
                BakeAllAnomalies();
            }
        }

        private void BakeAllAnomalies()
        {
            if (!Directory.Exists(sourceDataDirectory))
            {
                Debug.LogError($"[Anomaly Importer] Source directory not found: {sourceDataDirectory}");
                return;
            }

            string[] files = Directory.GetFiles(sourceDataDirectory, "*.json");
            int bakedCount = 0;

            foreach (var file in files)
            {
                string jsonText = File.ReadAllText(file);
                string fileName = Path.GetFileNameWithoutExtension(file);
                
                string subjectFolder = "Explore";
                string themeIcon = "🔮";
                string themeColor = "#A020F0"; // Purple

                if (fileName.Contains("수학")) { subjectFolder = "Math"; themeIcon = "📐"; themeColor = "#3366FF"; }
                else if (fileName.Contains("국어")) { subjectFolder = "Korean"; themeIcon = "📖"; themeColor = "#FF3366"; }
                else if (fileName.Contains("물리") || fileName.Contains("화학") || fileName.Contains("생명") || fileName.Contains("지구")) { subjectFolder = "Science"; themeIcon = "🔬"; themeColor = "#33CC33"; }
                else if (fileName.Contains("윤리") || fileName.Contains("지리") || fileName.Contains("역사") || fileName.Contains("사회")) { subjectFolder = "Social"; themeIcon = "⚖️"; themeColor = "#FF9933"; }

                string outDir = Path.Combine(outputAssetDirectory, subjectFolder).Replace("\\", "/");
                
                // 디렉토리가 없으면 생성 (물리 파일 시스템 기준)
                string fullPath = Path.Combine(Application.dataPath, outDir.Replace("Assets/", ""));
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                try
                {
                    JArray array = JArray.Parse(jsonText);
                    foreach (JObject item in array)
                    {
                        string monsterId = item["monster_id"]?.ToString() ?? System.Guid.NewGuid().ToString();
                        string originalFormula = item["original_formula"]?.ToString() ?? "Unknown Question";
                        
                        // WorkspaceNodeData 에셋 생성
                        WorkspaceNodeData node = ScriptableObject.CreateInstance<WorkspaceNodeData>();
                        node.NodeId = monsterId;
                        
                        // 노드 타이틀 요약 (UI 렌더링용)
                        string title = originalFormula;
                        if (title.Length > 25) title = title.Substring(0, 25) + "...";
                        
                        node.NodeTitle = $"[{subjectFolder}] {title}";
                        node.TemplateType = "기믹 (Anomaly)";
                        node.ThemeIcon = themeIcon;
                        node.ThemeColorHex = themeColor;
                        
                        // 원본 지문/수식 프로퍼티
                        node.Properties.Add(new DynamicProperty { 
                            PropertyName = "Original Formula", 
                            Type = PropertyType.Text, 
                            StringValue = originalFormula, 
                            ShowAsBadge = false 
                        });

                        // 개별 기믹(Behavior) 프로퍼티 매핑
                        JToken nodesToken = item["nodes"];
                        if (nodesToken != null && nodesToken.Type == JTokenType.Array)
                        {
                            int idx = 1;
                            foreach (JObject n in nodesToken)
                            {
                                string behavior = n["Behavior"]?.ToString() ?? "";
                                string constraint = n["Constraint"]?.ToString() ?? "";
                                
                                // 노드 요약 배지로 렌더링되게 설정
                                node.Properties.Add(new DynamicProperty 
                                { 
                                    PropertyName = $"🎯 기믹 {idx}: {behavior}", 
                                    Type = PropertyType.Text, 
                                    StringValue = $"약점: {constraint}", 
                                    ShowAsBadge = true 
                                });
                                
                                idx++;
                            }
                        }

                        // 파일명 안전 규칙 적용 및 에셋 저장
                        string safeName = string.Join("_", monsterId.Split(Path.GetInvalidFileNameChars()));
                        string assetPath = $"{outDir}/{safeName}.asset";
                        
                        StudyGame.Editor.Utils.AssetHelper.CreateOrOverwriteAsset(node, assetPath);
                        bakedCount++;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[Anomaly Importer] Error parsing {file}: {e.Message}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Bake Complete", $"Successfully baked {bakedCount} Anomaly nodes into {outputAssetDirectory}", "OK");
        }
    }
}
