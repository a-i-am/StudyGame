using UnityEngine;
using Yarn.Unity;

namespace StudyGame.Interaction
{
    /// <summary>
    /// 필드 위의 오브젝트와 Yarn Spinner 스크립트를 연결하는 만능 레고 블록 (Ponytail Gain)
    /// </summary>
    public class YarnInteractable : MonoBehaviour
    {
        [Header("Yarn 연동")]
        [Tooltip("이 오브젝트와 상호작용 시 실행할 Yarn 노드 이름")]
        public string targetNodeName = "Start";
        
        [Tooltip("자동으로 DialogueRunner를 찾을지 여부")]
        public bool autoFindDialogueRunner = true;
        
        [SerializeField] private DialogueRunner dialogueRunner;

        private void Start()
        {
            if (autoFindDialogueRunner && dialogueRunner == null)
            {
                dialogueRunner = FindObjectOfType<DialogueRunner>();
            }
        }

        /// <summary>
        /// 플레이어가 클릭하거나 특정 트리거를 만족했을 때 호출되는 함수
        /// </summary>
        public void Interact()
        {
            if (dialogueRunner == null)
            {
                Debug.LogError($"[YarnInteractable] {gameObject.name}에 연결된 DialogueRunner가 없습니다!");
                return;
            }

            if (dialogueRunner.IsDialogueRunning)
            {
                Debug.LogWarning($"[YarnInteractable] 이미 다른 대화가 진행 중입니다. ({targetNodeName} 실행 취소)");
                return;
            }

            if (string.IsNullOrEmpty(targetNodeName))
            {
                Debug.LogError($"[YarnInteractable] {gameObject.name}의 실행할 노드 이름이 비어있습니다.");
                return;
            }

            Debug.Log($"[YarnInteractable] {gameObject.name} 상호작용 발생 -> Yarn 노드 '{targetNodeName}' 실행");
            dialogueRunner.StartDialogue(targetNodeName);
        }
    }
}
