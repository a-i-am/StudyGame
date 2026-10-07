using UnityEngine;
using Yarn.Unity;

namespace StudyGame.Interaction
{
    [RequireComponent(typeof(BoxCollider))]
    public class TriggerZone : MonoBehaviour
    {
        [Header("Trigger Settings")]
        public string YarnNodeName;
        public bool TriggerOnce = true;
        
        [Header("Gizmo Settings")]
        public Color GizmoColor = new Color(0, 1, 0, 0.3f);

        private bool _hasTriggered = false;

        private void Reset()
        {
            var col = GetComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(2f, 2f, 2f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasTriggered && TriggerOnce) return;

            // 플레이어 태그 확인
            if (other.CompareTag("Player"))
            {
                if (!string.IsNullOrEmpty(YarnNodeName))
                {
                    var dialogueRunner = FindObjectOfType<DialogueRunner>();
                    if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
                    {
                        dialogueRunner.StartDialogue(YarnNodeName);
                        _hasTriggered = true;
                        Debug.Log($"[TriggerZone] '{gameObject.name}' triggered Yarn Node: {YarnNodeName}");
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            var col = GetComponent<BoxCollider>();
            if (col == null) return;

            Gizmos.color = GizmoColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.center, col.size);
            
            Gizmos.color = new Color(GizmoColor.r, GizmoColor.g, GizmoColor.b, 1f);
            Gizmos.DrawWireCube(col.center, col.size);
        }
    }
}
