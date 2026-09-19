using System;
using System.Collections.Generic;
using UnityEngine;

namespace StudyGame.Editor.Director
{
    [Serializable]
    public class DirectorNodeData
    {
        public string NodeId;
        public DirectorNodeType NodeType;
        public string Title;
        public Rect Position;
        public Color NodeColor;
        public Vector3 WorldPosition;
    }

    [Serializable]
    public class LevelGeometryData
    {
        public List<Vector2Int> FloorCells = new List<Vector2Int>();
        public float GridSize = 1.0f;
        public float WallHeight = 3.0f;
        public bool GenerateWalls = true;
        public bool GenerateCeiling = false;
    }

    [Serializable]
    public class DirectorSaveData
    {
        public List<DirectorNodeData> Nodes = new List<DirectorNodeData>();
        public LevelGeometryData LevelGeometry = new LevelGeometryData();
    }
}
