using System;
using System.Collections.Generic;
using UnityEngine;

namespace StudyGame.Data
{
    [Serializable]
    public class NodeLinkData
    {
        public string BaseNodeGuid;
        public string PortName;
        public string TargetNodeGuid;
    }

    [Serializable]
    public class EpisodeNodeSaveData
    {
        public string NodeGuid;
        public Vector2 Position;
        public WorkspaceNodeData NodeData; // The actual content
    }

    [CreateAssetMenu(fileName = "NewEpisodeGraph", menuName = "StudyGame/Episode Graph Data")]
    public class EpisodeGraphData : ScriptableObject
    {
        public List<EpisodeNodeSaveData> Nodes = new List<EpisodeNodeSaveData>();
        public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();
    }
}
