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
    public class DirectorSaveData
    {
        public List<DirectorNodeData> Nodes = new List<DirectorNodeData>();
    }
}
