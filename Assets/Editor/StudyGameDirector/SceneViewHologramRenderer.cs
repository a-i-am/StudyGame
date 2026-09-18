using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudyGame.Editor.Director
{
    [InitializeOnLoad]
    public static class SceneViewHologramRenderer
    {
        // 각 노드의 ID와 2D 캔버스 상의 위치 저장
        public static Dictionary<string, Vector2> ActiveNodes = new Dictionary<string, Vector2>();

        static SceneViewHologramRenderer()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (ActiveNodes.Count == 0) return;

            foreach (var kvp in ActiveNodes)
            {
                var graphPos = kvp.Value;
                var worldPos = SpatialMapper.GraphToWorld(graphPos);
                
                float size = HandleUtility.GetHandleSize(worldPos) * 0.3f;

                // CCTV 카메라 본체 (빨간색)
                Handles.color = new Color(1f, 0.2f, 0.2f, 0.9f);
                Handles.DrawWireCube(worldPos + Vector3.up * 2f, new Vector3(size, size * 0.5f, size * 0.8f));
                
                // 카메라가 쏘는 시야각/레이저 선
                Handles.DrawLine(worldPos + Vector3.up * 2f, worldPos);

                // 바닥 투영 원 (파란색 홀로그램)
                Handles.color = new Color(0.2f, 0.8f, 1.0f, 0.2f);
                Handles.DrawSolidDisc(worldPos, Vector3.up, size * 3f);
                Handles.color = new Color(0.2f, 0.8f, 1.0f, 0.8f);
                Handles.DrawWireDisc(worldPos, Vector3.up, size * 3f);
                
                // CCTV 라벨
                GUIStyle labelStyle = new GUIStyle();
                labelStyle.normal.textColor = Color.white;
                labelStyle.alignment = TextAnchor.MiddleCenter;
                Handles.Label(worldPos + Vector3.up * 2.5f, $"🎥 CCTV: {kvp.Key}", labelStyle);
            }
            
            // 2D 캔버스에서 드래그 시 즉각적으로 반영되도록 Repaint
            sceneView.Repaint();
        }
    }
}
