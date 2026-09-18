using UnityEngine;

namespace StudyGame.Editor.Director
{
    public static class SpatialMapper
    {
        // 2D 캔버스 1단위가 3D 공간의 어느 정도 비율인지 결정
        public static float ScaleFactor = 0.05f;

        public static Vector3 GraphToWorld(Vector2 graphPos)
        {
            // 2D 캔버스의 +Y(아래) 방향을 3D 공간의 -Z(뒤) 방향으로 매핑하여 드래그 방향 일치
            return new Vector3(graphPos.x * ScaleFactor, 0, -graphPos.y * ScaleFactor);
        }

        public static Vector2 WorldToGraph(Vector3 worldPos)
        {
            return new Vector2(worldPos.x / ScaleFactor, -worldPos.z / ScaleFactor);
        }
    }
}
