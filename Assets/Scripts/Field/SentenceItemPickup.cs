using UnityEngine;
using StudyGame.Data;
using StudyGame.Managers;

namespace StudyGame.Field
{
    public class SentenceItemPickup : MonoBehaviour
    {
        public SentenceItemData itemData;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (itemData == null) return;

            InventoryManager.Instance.AddItem(itemData);
            Debug.Log($"<color=yellow>[Pickup] '{itemData.displayText}' 를 획득했습니다! 인벤토리 추가 완료.</color>");
            gameObject.SetActive(false);
        }
    }
}
