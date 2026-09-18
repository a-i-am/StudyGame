using System.Collections.Generic;
using UnityEngine;
using StudyGame.Data;

namespace StudyGame.Managers
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        private List<SentenceItemData> collectedItems = new List<SentenceItemData>();
        
        public event System.Action OnInventoryUpdated;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void AddItem(SentenceItemData item)
        {
            if (item != null && !collectedItems.Contains(item))
            {
                collectedItems.Add(item);
                OnInventoryUpdated?.Invoke();
            }
        }

        public void RemoveItem(SentenceItemData item)
        {
            if (collectedItems.Remove(item))
            {
                OnInventoryUpdated?.Invoke();
            }
        }

        public List<SentenceItemData> GetAllItems()
        {
            return new List<SentenceItemData>(collectedItems);
        }

        // Tag Vector Search (Bitwise AND)
        public List<SentenceItemData> GetItemsByTag(ItemTag requiredTags)
        {
            List<SentenceItemData> result = new List<SentenceItemData>();
            foreach (var item in collectedItems)
            {
                if ((item.tags & requiredTags) == requiredTags)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public List<SentenceItemData> GetItemsByCategory(SentenceCategory category)
        {
            List<SentenceItemData> result = new List<SentenceItemData>();
            foreach (var item in collectedItems)
            {
                if (item.category == category)
                {
                    result.Add(item);
                }
            }
            return result;
        }
        
        public void ClearInventory()
        {
            collectedItems.Clear();
            OnInventoryUpdated?.Invoke();
        }
    }
}
