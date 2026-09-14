using UnityEngine;

namespace StudyGame.Test
{
    public class TestSequenceRunner : MonoBehaviour
    {
        [SerializeField] private SequenceGraphData graphData;

        private SequenceNode currentNode;

        private void Start()
        {
            if (graphData == null || graphData.allNodes == null || graphData.allNodes.Count == 0)
            {
                Debug.LogError("[TestSequenceRunner] SequenceGraphData is null or empty!");
                return;
            }

            currentNode = graphData.entryNode != null ? graphData.entryNode : graphData.allNodes[0];
            DisplayNode(currentNode);
        }

        private void Update()
        {
            if (currentNode == null) return;

            for (int i = 0; i < 9; i++)
            {
                KeyCode key = KeyCode.Alpha1 + i;
                if (Input.GetKeyDown(key))
                {
                    SelectChoice(i);
                    break;
                }
            }
        }

        private void DisplayNode(SequenceNode node)
        {
            if (node == null)
            {
                Debug.LogWarning("[TestSequenceRunner] Reached end of sequence (null node).");
                return;
            }

            Debug.Log($"=== Node: {node.name} (Type: {node.sequenceType}) ===");
            if (node.snsData != null)
            {
                Debug.Log($"Sender: {node.snsData.senderName}");
                if (node.snsData.messages != null)
                {
                    foreach (string msg in node.snsData.messages)
                    {
                        Debug.Log($"Message: {msg}");
                    }
                }
            }
            if (node.unlockConcept != null)
            {
                Debug.Log($"[Unlock Concept]: {node.unlockConcept.title} ({node.unlockConcept.conceptId})");
                if (ConceptArchiveManager.Instance != null)
                {
                    ConceptArchiveManager.Instance.TryUnlockConcept(node.unlockConcept);
                }
            }

            if (node.choices == null || node.choices.Count == 0)
            {
                Debug.Log("[End of Dialogue] No choices available.");
                return;
            }

            Debug.Log("Select a choice (Press number key):");
            for (int i = 0; i < node.choices.Count; i++)
            {
                DialogueChoice choice = node.choices[i];
                string targetName = choice.targetNode != null ? choice.targetNode.name : "NULL";
                Debug.Log($"[{i + 1}] {choice.choiceText} -> Target: {targetName}");
            }
        }

        private void SelectChoice(int index)
        {
            if (currentNode == null || currentNode.choices == null) return;

            if (index < 0 || index >= currentNode.choices.Count)
            {
                Debug.LogWarning($"[TestSequenceRunner] Invalid choice index: {index + 1}");
                return;
            }

            DialogueChoice choice = currentNode.choices[index];
            if (choice.targetNode == null)
            {
                Debug.LogWarning($"[TestSequenceRunner] Choice [{choice.choiceText}] leads to NULL target node. Sequence terminated.");
                currentNode = null;
                return;
            }

            currentNode = choice.targetNode;
            DisplayNode(currentNode);
        }
    }
}
