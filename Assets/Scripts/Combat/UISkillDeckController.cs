using System;
using UnityEngine;
using UnityEngine.UIElements;
using StudyGame.UI;

namespace StudyGame.Combat
{
    [RequireComponent(typeof(UIDocument))]
    public class UISkillDeckController : MonoBehaviour
    {
        private UIDocument document;
        private VisualElement container;
        private Button btnLimit;
        private Button btnDerivative;
        private Button btnIntegral;

        public event Action<MathSkill> OnSkillSelected;
        public bool IsActive => container != null && container.style.display == DisplayStyle.Flex;

        private void OnEnable()
        {
            InitializeUI();
        }

        private void OnDisable()
        {
            if (btnLimit != null) btnLimit.clicked -= OnLimitClicked;
            if (btnDerivative != null) btnDerivative.clicked -= OnDerivativeClicked;
            if (btnIntegral != null) btnIntegral.clicked -= OnIntegralClicked;
        }

        private void InitializeUI()
        {
            if (container != null) return;

            document = GetComponent<UIDocument>();
            if (document == null || document.rootVisualElement == null) return;

            container = document.rootVisualElement.Q<VisualElement>("skill-deck-container");
            btnLimit = document.rootVisualElement.Q<Button>("btn-skill-limit");
            btnDerivative = document.rootVisualElement.Q<Button>("btn-skill-derivative");
            btnIntegral = document.rootVisualElement.Q<Button>("btn-skill-integral");

            if (btnLimit != null) btnLimit.clicked += OnLimitClicked;
            if (btnDerivative != null) btnDerivative.clicked += OnDerivativeClicked;
            if (btnIntegral != null) btnIntegral.clicked += OnIntegralClicked;

            Hide();
        }

        private void OnLimitClicked() => SelectSkill(MathSkill.Limit);
        private void OnDerivativeClicked() => SelectSkill(MathSkill.Derivative);
        private void OnIntegralClicked() => SelectSkill(MathSkill.Integral);

        private void Update()
        {
            if (!IsActive) return;

            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSkill(MathSkill.Limit);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSkill(MathSkill.Derivative);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSkill(MathSkill.Integral);
        }

        public void Show()
        {
            StartCoroutine(ShowRoutine());
        }

        private System.Collections.IEnumerator ShowRoutine()
        {
            document = GetComponent<UIDocument>();
            while (document != null && document.rootVisualElement == null) yield return null;

            InitializeUI();
            if (container != null)
            {
                container.style.display = DisplayStyle.Flex;
            }
        }

        public void Hide()
        {
            if (container != null)
            {
                container.style.display = DisplayStyle.None;
            }
        }

        private void SelectSkill(MathSkill skill)
        {
            Debug.Log($"[UISkillDeckController] Skill Selected: {skill}");
            OnSkillSelected?.Invoke(skill);
        }
    }
}
