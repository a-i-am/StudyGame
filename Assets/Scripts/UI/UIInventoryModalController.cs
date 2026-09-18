using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using StudyGame.Managers;
using StudyGame.Data;

namespace StudyGame.UI
{
    public class UIInventoryModalController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private KeyCode toggleKey = KeyCode.I;

        private VisualElement root;
        private VisualElement itemGridContainer;
        private VisualElement emptyDetailView;
        private VisualElement activeDetailView;
        
        private Label detailTitle;
        private Label detailCategory;
        private VisualElement detailTags;
        private Label detailLoreText;
        private Button btnClose;

        private bool isModalOpen = false;
        private List<VisualElement> slotPool = new List<VisualElement>();

        private void Start()
        {
            if (document == null) document = GetComponent<UIDocument>();
            if (document != null)
            {
                root = document.rootVisualElement.Q<VisualElement>("InventoryRoot");
                if (root != null)
                {
                    itemGridContainer = root.Q<VisualElement>("ItemGridContainer");
                    emptyDetailView = root.Q<VisualElement>("EmptyDetailView");
                    activeDetailView = root.Q<VisualElement>("ActiveDetailView");
                    
                    detailTitle = root.Q<Label>("DetailTitle");
                    detailCategory = root.Q<Label>("DetailCategory");
                    detailTags = root.Q<VisualElement>("DetailTags");
                    detailLoreText = root.Q<Label>("DetailLoreText");
                    
                    btnClose = root.Q<Button>("BtnClose");
                    if (btnClose != null) btnClose.clicked += CloseModal;

                    // Hide completely on start
                    root.style.display = DisplayStyle.None;
                    root.style.opacity = 0f;
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                if (isModalOpen) CloseModal();
                else OpenModal();
            }
        }

        public void OpenModal()
        {
            if (isModalOpen || root == null) return;
            
            // Rebuild UI with current data
            RefreshInventory();

            // SSOT Registration
            CursorManager.Instance?.RegisterModalOpen();
            isModalOpen = true;

            root.style.display = DisplayStyle.Flex;
            
            // Trigger animation frame
            root.schedule.Execute(() => {
                root.style.opacity = 1f;
                root.style.scale = new StyleScale(new Vector2(1f, 1f));
            }).StartingIn(10);
        }

        public void CloseModal()
        {
            if (!isModalOpen || root == null) return;

            CursorManager.Instance?.RegisterModalClose();
            isModalOpen = false;

            root.style.opacity = 0f;
            root.style.scale = new StyleScale(new Vector2(0.95f, 0.95f));

            root.schedule.Execute(() => {
                if (!isModalOpen) root.style.display = DisplayStyle.None;
            }).StartingIn(300); // Wait for transition
        }

        private void RefreshInventory()
        {
            if (itemGridContainer == null || InventoryManager.Instance == null) return;

            itemGridContainer.Clear();
            var items = InventoryManager.Instance.GetAllItems();

            foreach (var item in items)
            {
                VisualElement slot = CreateItemSlot(item);
                itemGridContainer.Add(slot);
            }

            // Default: hide detail
            emptyDetailView.style.display = DisplayStyle.Flex;
            activeDetailView.style.display = DisplayStyle.None;
        }

        private VisualElement CreateItemSlot(SentenceItemData item)
        {
            VisualElement slot = new VisualElement();
            slot.AddToClassList("item-slot");

            // Simple text placeholder for icon if icon is missing
            Label lbl = new Label(item.displayText.Substring(0, 1));
            lbl.style.fontSize = 24;
            lbl.style.color = Color.white;
            lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            slot.Add(lbl);

            slot.RegisterCallback<ClickEvent>(evt => 
            {
                // Clear selection styling from others
                foreach (var child in itemGridContainer.Children())
                    child.RemoveFromClassList("selected");
                
                slot.AddToClassList("selected");
                ShowDetail(item);
            });

            return slot;
        }

        private void ShowDetail(SentenceItemData item)
        {
            emptyDetailView.style.display = DisplayStyle.None;
            activeDetailView.style.display = DisplayStyle.Flex;

            detailTitle.text = item.displayText;
            detailCategory.text = $"[{item.category.ToString()}]";
            detailLoreText.text = $"이 아이템은 '{item.displayText}' 에 대한 단서입니다.\nAP Cost: {item.apCost}\n" +
                                   $"아이템 태그들을 활용하여 다음 스테이지 추론에 사용하세요.";

            detailTags.Clear();
            
            // Split tags
            if (item.tags.HasFlag(ItemTag.Logic)) detailTags.Add(CreateTagBadge("Logic", "logic"));
            if (item.tags.HasFlag(ItemTag.Emotional)) detailTags.Add(CreateTagBadge("Emotional", "emotional"));
            if (item.tags.HasFlag(ItemTag.Rare)) detailTags.Add(CreateTagBadge("Rare", "rare"));
            if (item.tags.HasFlag(ItemTag.Quest)) detailTags.Add(CreateTagBadge("Quest", "rare"));
        }

        private Label CreateTagBadge(string text, string className)
        {
            Label lbl = new Label(text);
            lbl.AddToClassList("tag-badge");
            lbl.AddToClassList(className);
            return lbl;
        }
    }
}
