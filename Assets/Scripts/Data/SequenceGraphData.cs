using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sequence Graph", menuName = "StudyGame/Data/Sequence Graph")]
public class SequenceGraphData : ScriptableObject
{
    public SequenceNode entryNode;
    public List<SequenceNode> allNodes = new List<SequenceNode>();

    public SequenceNode FindNodeByConcept(ConceptData targetConcept)
    {
        if (targetConcept == null || allNodes == null) return null;
        return allNodes.Find(node => node != null && node.unlockConcept == targetConcept);
    }
}
