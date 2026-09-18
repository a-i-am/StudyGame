using UnityEngine;
using StudyGame.Core;

public class FieldTrigger : MonoBehaviour, IInteractable
{
    [Header("Sequence Settings")]
    [Tooltip("에디터에서 생성한 SequenceNode 데이터를 끌어다 놓으세요.")]
    public SequenceNode targetNode;
    
    [Header("Interaction Settings")]
    public bool autoTriggerOnEnter = false;
    public string promptText = "조사하기";
    
    private bool hasTriggered = false;

    public string InteractionPrompt => promptText;

    public bool CanInteract(GameObject instigator)
    {
        return !hasTriggered && targetNode != null;
    }

    public void OnInteract(GameObject instigator)
    {
        if (hasTriggered) return;
        ExecuteSequence();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!autoTriggerOnEnter || hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            ExecuteSequence();
        }
    }

    private void ExecuteSequence()
    {
        if (targetNode == null)
        {
            Debug.LogWarning("[FieldTrigger] targetNode가 인스펙터에 할당되어 있지 않습니다.");
            return;
        }

        hasTriggered = true;
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        if (StageManager.Instance != null)
        {
            StageManager.Instance.ExecuteSequence(targetNode);
        }
        else
        {
            Debug.LogWarning("[FieldTrigger] StageManager.Instance를 찾을 수 없습니다.");
        }
    }
}