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
    public class FloorLayer
    {
        public int LevelIndex;
        public List<Vector2Int> FloorCells = new List<Vector2Int>();
        public List<Vector2Int> StairCells = new List<Vector2Int>();
    }

    [Serializable]
    public class LevelGeometryData : ISerializationCallbackReceiver
    {
        public List<FloorLayer> Layers = new List<FloorLayer>();
        public float GridSize = 1.0f;
        public float WallHeight = 3.0f;
        public bool GenerateWalls = true;
        public bool GenerateCeiling = false;
        public float CeilingOpacity = 0.5f;

        [NonSerialized] public int CurrentLayerIndex = 0;

        public List<Vector2Int> CurrentFloorCells
        {
            get
            {
                EnsureLayerExists(CurrentLayerIndex);
                return Layers[CurrentLayerIndex].FloorCells;
            }
        }

        public List<Vector2Int> CurrentStairCells
        {
            get
            {
                EnsureLayerExists(CurrentLayerIndex);
                return Layers[CurrentLayerIndex].StairCells;
            }
        }

        public void EnsureLayerExists(int index)
        {
            while (Layers.Count <= index)
            {
                Layers.Add(new FloorLayer { LevelIndex = Layers.Count });
            }
        }

        [SerializeField] private List<Vector2Int> FloorCells; // Legacy for migration

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            if (FloorCells != null && FloorCells.Count > 0)
            {
                if (Layers.Count == 0)
                {
                    Layers.Add(new FloorLayer { LevelIndex = 0, FloorCells = new List<Vector2Int>(FloorCells) });
                }
                FloorCells.Clear();
            }
            
            if (Layers.Count == 0)
            {
                Layers.Add(new FloorLayer { LevelIndex = 0 });
            }
        }
    }

    [Serializable]
    public class DirectorSaveData
    {
        public List<DirectorNodeData> Nodes = new List<DirectorNodeData>();
        public LevelGeometryData LevelGeometry = new LevelGeometryData();
    }
}
