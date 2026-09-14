using UnityEngine;

public class FieldTrigger : MonoBehaviour
{
    [Header("연결할 시퀀스 노드")]
    [Tooltip("에디터에서 생성한 SequenceNode 데이터를 끌어다 놓으세요.")]
    public SequenceNode targetNode;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (targetNode == null)
            {
                Debug.LogWarning("[FieldTrigger] targetNode가 인스펙터에 할당되어 있지 않습니다.");
                return;
            }

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
}