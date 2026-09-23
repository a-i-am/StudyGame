using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using StudyGame.Data;
using StudyGame.Managers;

namespace StudyGame.UI
{
    public class UIQuestionSynthesizerController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private ValidationRuleSO currentRule;

        private VisualElement rootVisualElement;
        private VisualElement slotSubject;
        private VisualElement slotOperator;
        private VisualElement slotTarget;
        private VisualElement inventoryContainer;
        private Button btnSynthesize;
        private Button btnClose;
        private Label lblFeedback;

        private SentenceItemData selectedSubject;
        private SentenceItemData selectedOperator;
        private SentenceItemData selectedTarget;

        private List<SentenceItemData> cachedItems = new List<SentenceItemData>();
        private QuestionSynthesizerValidator validator = new QuestionSynthesizerValidator();
        private List<GenericDragAndDropHandler<SentenceItemData>> activeDragHandlers = new List<GenericDragAndDropHandler<SentenceItemData>>();

        public event Action<SynthesisResult, int> OnSynthesisSubmittedWithDamage;

        // 보스의 현재 약점 키워드 (데미지 배율 계산용)
        private string currentBossConstraint = "";
        private void Awake()
        {
            EnsureUIInitialized();
        }

        private void OnEnable()
        {
            EnsureUIInitialized();
        }

        private void OnDisable()
        {
            ClearDragHandlers();
        }



        private void EnsureUIInitialized()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            if (uiDocument != null && uiDocument.rootVisualElement != null)
            {
                rootVisualElement = uiDocument.rootVisualElement;
                InitializeUI();
            }
        }

        private void InitializeUI()
        {
            if (rootVisualElement == null) return;

            slotSubject = rootVisualElement.Q<VisualElement>("slot-subject");
            slotOperator = rootVisualElement.Q<VisualElement>("slot-operator");
            slotTarget = rootVisualElement.Q<VisualElement>("slot-target");
            inventoryContainer = rootVisualElement.Q<VisualElement>("inventory-container");

            btnSynthesize = rootVisualElement.Q<Button>("btn-synthesize");
            btnClose = rootVisualElement.Q<Button>("btn-close");
            lblFeedback = rootVisualElement.Q<Label>("lbl-feedback");

            if (btnSynthesize != null)
            {
                btnSynthesize.clicked -= OnSynthesizeClicked;
                btnSynthesize.clicked += OnSynthesizeClicked;
            }

            if (btnClose != null)
            {
                btnClose.clicked -= HideModal;
                btnClose.clicked += HideModal;
            }
        }
        public void SetBossConstraint(string constraint)
        {
            currentBossConstraint = constraint;
        }

        // [추가됨] 가상 시퀀스 러너용: UI 조작 없이 코드로 직접 문장을 합성하고 결과를 반환
        public void ForceSynthesizeVirtual(SentenceItemData subject, SentenceItemData op, SentenceItemData target)
        {
            selectedSubject = subject;
            selectedOperator = op;
            selectedTarget = target;

            Debug.Log($"[Virtual Test] 가상 문장 합성 시도: [{subject?.displayText}] + [{op?.displayText}] + [{target?.displayText}]");
            OnSynthesizeClicked();
        }
        public void PopulateInventory(List<SentenceItemData> items)
        {
            if (items != null)
            {
                cachedItems = items;
            }

            EnsureUIInitialized();

            ClearDragHandlers();
            if (inventoryContainer == null) return;
            inventoryContainer.Clear();

            List<SentenceItemData> itemsToDisplay = (cachedItems != null && cachedItems.Count > 0) ? cachedItems : CreateFallbackItems();

            foreach (var item in itemsToDisplay)
            {
                if (item == null) continue;

                VisualElement itemCard = new VisualElement();
                itemCard.AddToClassList("sentence-item-card");

                itemCard.style.width = 140;
                itemCard.style.height = 80;
                itemCard.style.backgroundColor = new StyleColor(new Color(0.25f, 0.25f, 0.3f, 1f));
                itemCard.style.borderLeftWidth = 1;
                itemCard.style.borderRightWidth = 1;
                itemCard.style.borderTopWidth = 1;
                itemCard.style.borderBottomWidth = 1;
                itemCard.style.borderLeftColor = new StyleColor(new Color(0.7f, 0.7f, 0.8f, 1f));
                itemCard.style.borderRightColor = new StyleColor(new Color(0.7f, 0.7f, 0.8f, 1f));
                itemCard.style.borderTopColor = new StyleColor(new Color(0.7f, 0.7f, 0.8f, 1f));
                itemCard.style.borderBottomColor = new StyleColor(new Color(0.7f, 0.7f, 0.8f, 1f));
                itemCard.style.borderTopLeftRadius = 8;
                itemCard.style.borderTopRightRadius = 8;
                itemCard.style.borderBottomLeftRadius = 8;
                itemCard.style.borderBottomRightRadius = 8;
                itemCard.style.marginTop = 8;
                itemCard.style.marginBottom = 8;
                itemCard.style.marginLeft = 8;
                itemCard.style.marginRight = 8;
                itemCard.style.alignItems = Align.Center;
                itemCard.style.justifyContent = Justify.Center;

                string displayStr = !string.IsNullOrEmpty(item.displayText) ? item.displayText : (string.IsNullOrEmpty(item.itemId) ? item.name : item.itemId);
                Label lbl = new Label(displayStr);
                lbl.pickingMode = PickingMode.Ignore;
                lbl.style.color = new StyleColor(Color.white);
                lbl.style.fontSize = 14;
                lbl.style.whiteSpace = WhiteSpace.Normal;
                lbl.style.unityTextAlign = TextAnchor.MiddleCenter;
                itemCard.Add(lbl);

                inventoryContainer.Add(itemCard);

                var handler = new GenericDragAndDropHandler<SentenceItemData>(itemCard, item, rootVisualElement);
                handler.OnDragEnded += OnItemDropped;
                activeDragHandlers.Add(handler);
            }
        }

        private List<SentenceItemData> CreateFallbackItems()
        {
            var list = new List<SentenceItemData>();

            var sub = ScriptableObject.CreateInstance<SentenceItemData>();
            sub.itemId = "subj_doyoung";
            sub.displayText = "도영의 문제";
            sub.category = SentenceCategory.Subject;
            list.Add(sub);

            var op = ScriptableObject.CreateInstance<SentenceItemData>();
            op.itemId = "op_multiply";
            op.displayText = "수렴 값 고정";
            op.category = SentenceCategory.Operator;
            list.Add(op);

            var tar = ScriptableObject.CreateInstance<SentenceItemData>();
            tar.itemId = "target_concept";
            tar.displayText = "극한 개념";
            tar.category = SentenceCategory.TargetConcept;
            list.Add(tar);

            return list;
        }

        private void OnItemDropped(GenericDragAndDropHandler<SentenceItemData> handler, Vector2 dropPos, VisualElement targetElement)
        {
            if (targetElement == null) return;

            SentenceItemData item = handler.BoundData;

            if (targetElement == slotSubject || slotSubject?.Contains(targetElement) == true)
            {
                SetSlot(ref selectedSubject, slotSubject, item);
            }
            else if (targetElement == slotOperator || slotOperator?.Contains(targetElement) == true)
            {
                SetSlot(ref selectedOperator, slotOperator, item);
            }
            else if (targetElement == slotTarget || slotTarget?.Contains(targetElement) == true)
            {
                SetSlot(ref selectedTarget, slotTarget, item);
            }

            ValidateCurrentSlots();
        }

        private void SetSlot(ref SentenceItemData targetSlotData, VisualElement slotVisual, SentenceItemData item)
        {
            targetSlotData = item;
            if (slotVisual != null)
            {
                slotVisual.Clear();
                string txt = item != null ? (!string.IsNullOrEmpty(item.displayText) ? item.displayText : item.itemId) : "";
                Label lbl = new Label(txt);
                lbl.style.color = new StyleColor(Color.white);
                lbl.style.fontSize = 14;
                lbl.style.unityTextAlign = TextAnchor.MiddleCenter;
                slotVisual.Add(lbl);
            }
        }

        private void ValidateCurrentSlots()
        {
            if (currentRule == null)
            {
                if (lblFeedback != null) lblFeedback.text = "규칙(Rule)이 설정되지 않았습니다.";
                return;
            }

            List<SentenceItemData> items = new List<SentenceItemData> { selectedSubject, selectedOperator, selectedTarget };
            SynthesisResult result = validator.ValidateSynthesis(currentRule, items);

            if (lblFeedback != null)
            {
                lblFeedback.text = result.feedbackMessage;
            }
        }

        private void OnSynthesizeClicked()
        {
            if (currentRule == null)
            {
                if (lblFeedback != null) lblFeedback.text = "규칙(Rule)이 설정되지 않았습니다.";
                return;
            }

            List<SentenceItemData> items = new List<SentenceItemData> { selectedSubject, selectedOperator, selectedTarget };
            SynthesisResult result = validator.ValidateSynthesis(currentRule, items);

            if (result.isValid)
            {
                // [추가됨] 데미지 연산 로직: 조합된 문장이 보스의 현재 약점과 얼마나 일치하는지 평가
                int calculatedDamage = CalculateDamage(items, currentBossConstraint);

                Debug.Log($"[추리 엔진] 문장 조합 성공! 데미지: {calculatedDamage}");

                // 확장된 이벤트 호출
                OnSynthesisSubmittedWithDamage?.Invoke(result, calculatedDamage);

                // 가상 모드가 아닐 때만 모달 닫기
                if (rootVisualElement != null && rootVisualElement.style.display == DisplayStyle.Flex)
                {
                    HideModal();
                }
            }
            else
            {
                if (lblFeedback != null)
                {
                    lblFeedback.text = result.feedbackMessage;
                }
                Debug.Log($"[추리 엔진] 문장 조합 실패: {result.feedbackMessage}");
            }
        }
        private int CalculateDamage(List<SentenceItemData> items, string bossConstraint)
        {
            int baseDamage = 10;
            int multiplier = 1;

            if (string.IsNullOrEmpty(bossConstraint)) return baseDamage;

            // 예시: 조합된 문장의 아이템 태그 중 보스의 약점(Constraint) 키워드가 포함되어 있다면 데미지 3배
            foreach (var item in items)
            {
                if (item != null)
                {
                    string tag = item.tags.ToString();
                    if (!string.IsNullOrEmpty(tag) && bossConstraint.Contains(tag))
                    {
                        multiplier += 2;
                    }
                }
            }

            return baseDamage * multiplier;
        }
        public void ShowModal()
        {
            EnsureUIInitialized();

            if (rootVisualElement != null)
            {
                rootVisualElement.style.display = DisplayStyle.Flex;
            }

            if (inventoryContainer == null || inventoryContainer.childCount == 0)
            {
                PopulateInventory(cachedItems);
            }

            CursorManager.Instance.RegisterModalOpen();
        }

        public void HideModal()
        {
            if (rootVisualElement != null)
            {
                rootVisualElement.style.display = DisplayStyle.None;
            }

            CursorManager.Instance.RegisterModalClose();
        }

        private void ClearDragHandlers()
        {
            foreach (var handler in activeDragHandlers)
            {
                handler.Unregister();
            }
            activeDragHandlers.Clear();
        }
    }
}
